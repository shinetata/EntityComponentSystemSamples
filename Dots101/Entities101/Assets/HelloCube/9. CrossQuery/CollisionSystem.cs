using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace HelloCube.CrossQuery
{
    public partial struct CollisionSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ExecuteCrossQuery>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder()
                .WithAll<LocalTransform, DefaultColor, CrossQueryColor, CrossQueryIndex>()
                .Build();

            var count = query.CalculateEntityCount();
            var positions = new NativeArray<float3>(count, Allocator.Temp);

            foreach (var (transform, index) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<CrossQueryIndex>>())
            {
                positions[index.ValueRO.Value] = transform.ValueRO.Position;
            }

            foreach (var (transform, defaultColor, color, index) in
                     SystemAPI.Query<RefRO<LocalTransform>, RefRO<DefaultColor>,
                         RefRW<CrossQueryColor>, RefRO<CrossQueryIndex>>())
            {
                color.ValueRW.Value = defaultColor.ValueRO.Value;

                var selfIndex = index.ValueRO.Value;
                var selfPos = transform.ValueRO.Position;

                for (int i = 0; i < positions.Length; i++)
                {
                    if (i == selfIndex)
                        continue;

                    if (math.distancesq(selfPos, positions[i]) < 1)
                    {
                        color.ValueRW.Value.y = 0.5f;
                        break;
                    }
                }
            }

            positions.Dispose();
        }
    }
}
