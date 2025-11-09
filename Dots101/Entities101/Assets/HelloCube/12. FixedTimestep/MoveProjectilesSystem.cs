using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace HelloCube.FixedTimestep
{
    public partial struct MoveProjectilesSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ExecuteFixedTimestep>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var timeSinceLoad = (float)SystemAPI.Time.ElapsedTime;
            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);

            foreach (var (transform, projectile, entity) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<Projectile>>().WithEntityAccess())
            {
                float aliveTime = timeSinceLoad - projectile.ValueRO.SpawnTime;
                if (aliveTime > 5.0f)
                {
                    ecb.DestroyEntity(entity);
                    continue;
                }

                transform.ValueRW.Position.x = projectile.ValueRO.SpawnPos.x + aliveTime * 5.0f;
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}
      