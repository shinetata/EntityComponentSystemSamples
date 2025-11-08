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
            var childRelations = SystemAPI.QueryBuilder()
                .WithAll<LocalTransform2D, Parent>()
                .Build();

            int childCount = childRelations.CalculateEntityCount();
            var childMap = new NativeParallelMultiHashMap<Entity, Entity>(childCount, Allocator.Temp);

            foreach (var (parent, entity) in SystemAPI.Query<RefRO<Parent>>().WithAll<LocalTransform2D>().WithEntityAccess())
            {
                childMap.Add(parent.ValueRO.Value, entity);
            }

            var stack = new NativeList<StackItem>(Allocator.Temp);
            var roots = SystemAPI.QueryBuilder()
                .WithAll<LocalTransform2D>()
                .WithNone<Parent>()
                .Build()
                .ToEntityArray(Allocator.Temp);

            for (int i = 0; i < roots.Length; i++)
            {
                stack.Add(new StackItem
                {
                    Entity = roots[i],
                    ParentMatrix = float4x4.identity
                });
            }

            var iterator = default(NativeParallelMultiHashMapIterator<Entity>);
            while (stack.Length > 0)
            {
                int last = stack.Length - 1;
                var item = stack[last];
                stack.RemoveAtSwapBack(last);

                if (!SystemAPI.HasComponent<LocalTransform2D>(item.Entity))
                    continue;

                var transform2D = SystemAPI.GetComponent<LocalTransform2D>(item.Entity);
                float4x4 localMatrix = transform2D.ToMatrix();

                if (SystemAPI.HasComponent<PostTransformMatrix>(item.Entity))
                {
                    var postTransform = SystemAPI.GetComponent<PostTransformMatrix>(item.Entity);
                    localMatrix = math.mul(localMatrix, postTransform.Value);
                }

                var worldMatrix = math.mul(item.ParentMatrix, localMatrix);
                SystemAPI.SetComponent(item.Entity, new LocalToWorld { Value = worldMatrix });

                if (childMap.TryGetFirstValue(item.Entity, out var child, out iterator))
                {
                    do
                    {
                        stack.Add(new StackItem
                        {
                            Entity = child,
                            ParentMatrix = worldMatrix
                        });
                    } while (childMap.TryGetNextValue(out child, ref iterator));
                }
            }

            stack.Dispose();
            childMap.Dispose();
            roots.Dispose();
        }

        struct StackItem
        {
            public Entity Entity;
            public float4x4 ParentMatrix;
        }
    }
}
