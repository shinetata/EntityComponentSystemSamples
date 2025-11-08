using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;

namespace HelloCube.RandomSpawn
{
    public partial struct MovementSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ExecuteRandomSpawn>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var movement = new float3(0, SystemAPI.Time.DeltaTime * -20f, 0);

            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (transform, entity) in SystemAPI.Query<RefRW<LocalTransform>>()
                         .WithAll<Cube>()
                         .WithEntityAccess())
            {
                transform.ValueRW.Position += movement;
                if (transform.ValueRO.Position.y < 0)
                {
                    ecb.DestroyEntity(entity);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
