using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;

namespace HelloCube.ClosestTarget
{
    public partial struct TargetingSystem : ISystem
    {
        public enum SpatialPartitioningType
        {
            None,
            Simple,
            KDTree,
        }

        static NativeArray<ProfilerMarker> s_ProfilerMarkers;

        public void OnCreate(ref SystemState state)
        {
            s_ProfilerMarkers = new NativeArray<ProfilerMarker>(3, Allocator.Persistent);
            s_ProfilerMarkers[0] = new(nameof(TargetingSystem) + "." + SpatialPartitioningType.None);
            s_ProfilerMarkers[1] = new(nameof(TargetingSystem) + "." + SpatialPartitioningType.Simple);
            s_ProfilerMarkers[2] = new(nameof(TargetingSystem) + "." + SpatialPartitioningType.KDTree);

            state.RequireForUpdate<Settings>();
            state.RequireForUpdate<ExecuteClosestTarget>();
        }

        public void OnDestroy(ref SystemState state)
        {
            s_ProfilerMarkers.Dispose();
        }

        public void OnUpdate(ref SystemState state)
        {
            var targetQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform>().WithNone<Target, Settings>().Build();
            var kdQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform, Target>().Build();

            var spatialPartitioningType = SystemAPI.GetSingleton<Settings>().SpatialPartitioning;

            using var profileMarker = s_ProfilerMarkers[(int)spatialPartitioningType].Auto();

            var targetEntities = targetQuery.ToEntityArray(Allocator.TempJob);
            var targetTransforms = targetQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var targetData = CollectionHelper.CreateNativeArray<TargetData>(targetEntities.Length, Allocator.TempJob);

            for (int i = 0; i < targetData.Length; i += 1)
            {
                targetData[i] = new TargetData
                {
                    Entity = targetEntities[i],
                    Position = targetTransforms[i].Position
                };
            }

            switch (spatialPartitioningType)
            {
                case SpatialPartitioningType.None:
                {
                    var noPartitioning = new NoPartitioning
                        { Targets = targetData };
                    state.Dependency = noPartitioning.ScheduleParallel(state.Dependency);
                    break;
                }
                case SpatialPartitioningType.Simple:
                {
                    var positions = CollectionHelper.CreateNativeArray<PositionAndEntity>(targetData.Length,
                        state.WorldUpdateAllocator);

                    for (int i = 0; i < positions.Length; i += 1)
                    {
                        positions[i] = new PositionAndEntity
                        {
                            Entity = targetData[i].Entity,
                            Position = targetData[i].Position.xz
                        };
                    }

                    state.Dependency = positions.SortJob(new AxisXComparer()).Schedule(state.Dependency);

                    var simple = new SimplePartitioning { Positions = positions };
                    state.Dependency = simple.ScheduleParallel(state.Dependency);
                    state.Dependency = positions.Dispose(state.Dependency);
                    break;
                }
                case SpatialPartitioningType.KDTree:
                {
                    var tree = new KDTree(targetData.Length, Allocator.TempJob, 64);

                    // init KD tree
                    for (int i = 0; i < targetData.Length; i += 1)
                    {
                        // NOTE - the first parameter is ignored, only the index matters
                        tree.AddEntry(i, targetData[i].Position);
                    }

                    state.Dependency = tree.BuildTree(targetData.Length, state.Dependency);

                    var queryKdTree = new QueryKDTreeJob
                    {
                        Tree = tree,
                        Targets = targetData
                    };
                    state.Dependency = queryKdTree.ScheduleParallel(kdQuery, state.Dependency);

                    state.Dependency.Complete();
                    tree.Dispose();
                    break;
                }
            }

            state.Dependency.Complete();
            targetEntities.Dispose();
            targetTransforms.Dispose();
            targetData.Dispose();
        }
    }

    [WithAll(typeof(Target))]
    [BurstCompile]
    public partial struct QueryKDTreeJob : IJobEntity
    {
        [ReadOnly] public NativeArray<TargetData> Targets;
        public KDTree Tree;

        void Execute([ChunkIndexInQuery] int chunkIndex, ref Target target, in LocalTransform transform)
        {
            var neighbours = new NativePriorityHeap<KDTree.Neighbour>(1, Allocator.Temp);
            try
            {
                Tree.GetEntriesInRangeWithHeap(chunkIndex, transform.Position, float.MaxValue, ref neighbours);
                var nearest = neighbours.Peek().index;
                target.Value = Targets[nearest].Entity;
            }
            finally
            {
                neighbours.Dispose();
            }
        }
    }

    [BurstCompile]
    public partial struct SimplePartitioning : IJobEntity
    {
        [ReadOnly] public NativeArray<PositionAndEntity> Positions;

        public void Execute(ref Target target, in LocalTransform translation)
        {
            var ownpos = new PositionAndEntity { Position = translation.Position.xz };
            var index = Positions.BinarySearch(ownpos, new AxisXComparer());
            if (index < 0) index = ~index;
            if (index >= Positions.Length) index = Positions.Length - 1;

            var closestDistSq = math.distancesq(ownpos.Position, Positions[index].Position);
            var closestEntity = index;

            Search(index + 1, Positions.Length, +1, ref closestDistSq, ref closestEntity, ownpos);
            Search(index - 1, -1, -1, ref closestDistSq, ref closestEntity, ownpos);

            target.Value = Positions[closestEntity].Entity;
        }

        void Search(int startIndex, int endIndex, int step, ref float closestDistSqRef, ref int closestEntityRef,
            PositionAndEntity ownpos)
        {
            for (int i = startIndex; i != endIndex; i += step)
            {
                var xdiff = ownpos.Position.x - Positions[i].Position.x;
                xdiff *= xdiff;

                if (xdiff > closestDistSqRef) break;

                var distSq = math.distancesq(Positions[i].Position, ownpos.Position);

                if (distSq < closestDistSqRef)
                {
                    closestDistSqRef = distSq;
                    closestEntityRef = i;
                }
            }
        }
    }


    [BurstCompile]
    public partial struct NoPartitioning : IJobEntity
    {
        [ReadOnly] public NativeArray<TargetData> Targets;

        public void Execute(ref Target target, in LocalTransform translation)
        {
            var closestDistSq = float.MaxValue;
            var closestEntity = Entity.Null;

            for (int i = 0; i < Targets.Length; i += 1)
            {
                var distSq = math.distancesq(Targets[i].Position, translation.Position);
                if (distSq < closestDistSq)
                {
                    closestDistSq = distSq;
                    closestEntity = Targets[i].Entity;
                }
            }

            target.Value = closestEntity;
        }
    }

    public struct AxisXComparer : IComparer<PositionAndEntity>
    {
        public int Compare(PositionAndEntity a, PositionAndEntity b)
        {
            return a.Position.x.CompareTo(b.Position.x);
        }
    }

    public struct PositionAndEntity
    {
        public Entity Entity;
        public float2 Position;
    }

    public struct TargetData
    {
        public Entity Entity;
        public float3 Position;
    }

}
