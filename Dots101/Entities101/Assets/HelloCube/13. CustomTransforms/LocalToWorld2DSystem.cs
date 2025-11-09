using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace HelloCube.CustomTransforms
{
    [BurstCompile]
    public partial struct LocalToWorld2DSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<LocalTransform2D>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var childQuery = SystemAPI.QueryBuilder()
                .WithAll<LocalTransform2D, Parent>()
                .Build();

            var rootsQuery = SystemAPI.QueryBuilder()
                .WithAll<LocalTransform2D>()
                .WithNone<Parent>()
                .Build();

            int estimatedChildCount = math.max(childQuery.CalculateEntityCount(), 1);
            var childMap = new NativeParallelMultiHashMap<int, Entity>(estimatedChildCount, Allocator.Temp);

            foreach (var (parent, entity) in
                     SystemAPI.Query<RefRO<Parent>>()
                         .WithAll<LocalTransform2D>()
                         .WithEntityAccess())
            {
                childMap.Add(parent.ValueRO.Value.Index, entity);
            }

            var stack = new NativeList<StackItem>(Allocator.Temp);
            var roots = rootsQuery.ToEntityArray(Allocator.Temp);

            foreach (var root in roots)
            {
                stack.Add(new StackItem
                {
                    Entity = root,
                    ParentMatrix = float4x4.identity
                });
            }

            while (stack.Length > 0)
            {
                int last = stack.Length - 1;
                var current = stack[last];
                stack.RemoveAtSwapBack(last);

                if (!SystemAPI.HasComponent<LocalTransform2D>(current.Entity))
                    continue;

                var transform2D = SystemAPI.GetComponent<LocalTransform2D>(current.Entity);
                float4x4 localMatrix = transform2D.ToMatrix();

                if (SystemAPI.HasComponent<PostTransformMatrix>(current.Entity))
                {
                    var postTransform = SystemAPI.GetComponent<PostTransformMatrix>(current.Entity);
                    localMatrix = math.mul(localMatrix, postTransform.Value);
                }

                var worldMatrix = math.mul(current.ParentMatrix, localMatrix);
                SystemAPI.SetComponent(current.Entity, new LocalToWorld { Value = worldMatrix });

                if (childMap.TryGetFirstValue(current.Entity.Index, out var child, out var iterator))
                {
                    do
                    {
                        stack.Add(new StackItem
                        {
                            Entity = child,
                            ParentMatrix = worldMatrix
                        });
                    }
                    while (childMap.TryGetNextValue(out child, ref iterator));
                }
            }

            roots.Dispose();
            stack.Dispose();
            childMap.Dispose();
        }

        struct StackItem
        {
            public Entity Entity;
            public float4x4 ParentMatrix;
        }
    }
}
