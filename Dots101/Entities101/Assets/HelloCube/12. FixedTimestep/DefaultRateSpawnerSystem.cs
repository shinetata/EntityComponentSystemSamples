using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace HelloCube.FixedTimestep
{
    public partial struct DefaultRateSpawnerSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ExecuteFixedTimestep>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            float spawnTime = (float)SystemAPI.Time.ElapsedTime;

            foreach (var spawner in SystemAPI.Query<RefRW<DefaultRateSpawner>>())
            {
                spawner.ValueRW.Accumulator += deltaTime;
                while (spawner.ValueRW.Accumulator >= spawner.ValueRO.SpawnInterval)
                {
                    spawner.ValueRW.Accumulator -= spawner.ValueRO.SpawnInterval;

                    var projectileEntity = state.EntityManager.Instantiate(spawner.ValueRO.Prefab);
                    var spawnPos = spawner.ValueRO.SpawnPos;
                    spawnPos.y += 0.3f * math.sin(5.0f * spawnTime);

                    SystemAPI.SetComponent(projectileEntity, LocalTransform.FromPosition(spawnPos));
                    SystemAPI.SetComponent(projectileEntity, new Projectile
                    {
                        SpawnTime = spawnTime,
                        SpawnPos = spawnPos,
                    });
                }
            }
        }
    }
}
