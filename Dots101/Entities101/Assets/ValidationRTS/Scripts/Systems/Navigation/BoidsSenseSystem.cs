using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ValidationRTS.Components;

namespace ValidationRTS.Systems.Navigation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ValidationRTS.Systems.Core.CommandInfluenceSystem))]
    public partial struct BoidsSenseSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UnitTag>();
        }

        [BurstCompile]
        public partial struct SteeringJob : IJobEntity
        {
            [ReadOnly] public NativeArray<float3> Positions;
            [ReadOnly] public NativeArray<float3> Velocities;
            [ReadOnly] public NativeArray<byte> Factions;
            [ReadOnly] public NativeParallelHashMap<Entity, int> IndexMap;
            public float CohesionWeight;
            public float AlignmentWeight;
            public float SeparationRadius;

            private void Execute(Entity entity, ref UnitSteering steering, in UnitTag tag)
            {
                if (!IndexMap.TryGetValue(entity, out var index))
                {
                    return;
                }

                var origin = Positions[index];
                var velocity = Velocities[index];
                float3 cohesion = float3.zero;
                float3 alignment = velocity;
                float3 separation = float3.zero;
                int neighborCount = 0;

                for (int i = 0; i < Positions.Length; i++)
                {
                    if (i == index)
                    {
                        continue;
                    }

                    if (Factions[i] != tag.FactionId)
                    {
                        continue;
                    }

                    var offset = Positions[i] - origin;
                    var distance = math.length(offset);
                    if (distance < 0.0001f)
                    {
                        continue;
                    }

                    neighborCount++;
                    cohesion += Positions[i];
                    alignment += Velocities[i];

                    if (distance < SeparationRadius)
                    {
                        separation -= offset / math.max(distance, 0.001f);
                    }
                }

                if (neighborCount > 0)
                {
                    cohesion = (cohesion / neighborCount - origin) * CohesionWeight;
                    alignment = math.normalizesafe(alignment / (neighborCount + 1)) * AlignmentWeight;
                }
                else
                {
                    cohesion = float3.zero;
                    alignment = math.normalizesafe(velocity) * AlignmentWeight;
                }

                steering.Cohesion = cohesion;
                steering.Alignment = alignment;
                steering.Separation = separation;
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder()
                .WithAll<UnitTag, LocalTransform, UnitVelocity>()
                .Build();

            var entityCount = math.max(1, query.CalculateEntityCount());
            var positions = new NativeList<float3>(entityCount, Allocator.TempJob);
            var velocities = new NativeList<float3>(entityCount, Allocator.TempJob);
            var factions = new NativeList<byte>(entityCount, Allocator.TempJob);
            var indexMap = new NativeParallelHashMap<Entity, int>(entityCount, Allocator.TempJob);

            foreach (var (transform, velocity, tag, entity) in SystemAPI
                         .Query<RefRO<LocalTransform>, RefRO<UnitVelocity>, RefRO<UnitTag>>()
                         .WithEntityAccess())
            {
                var idx = positions.Length;
                positions.Add(transform.ValueRO.Position);
                velocities.Add(velocity.ValueRO.Value);
                factions.Add(tag.ValueRO.FactionId);
                indexMap.TryAdd(entity, idx);
            }

            var job = new SteeringJob
            {
                Positions = positions.AsArray(),
                Velocities = velocities.AsArray(),
                Factions = factions.AsArray(),
                IndexMap = indexMap,
                CohesionWeight = UnitConstants.DefaultCohesionWeight,
                AlignmentWeight = UnitConstants.DefaultAlignmentWeight,
                SeparationRadius = UnitConstants.DefaultSeparationRadius
            };

            var handle = job.ScheduleParallel(state.Dependency);
            var disposeHandle = positions.Dispose(handle);
            disposeHandle = velocities.Dispose(disposeHandle);
            disposeHandle = factions.Dispose(disposeHandle);
            disposeHandle = indexMap.Dispose(disposeHandle);
            state.Dependency = disposeHandle;
        }
    }
}
