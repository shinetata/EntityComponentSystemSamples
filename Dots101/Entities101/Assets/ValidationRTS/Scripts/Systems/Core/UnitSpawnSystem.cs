using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ValidationRTS.Components;

namespace ValidationRTS.Systems.Core
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct UnitSpawnSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ValidationRTSConfig>();
            state.RequireForUpdate<UnitPoolConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var bootstrapEntity = SystemAPI.GetSingletonEntity<ValidationRTSConfig>();

            if (!SystemAPI.HasComponent<UnitSpawnState>(bootstrapEntity) ||
                SystemAPI.GetComponent<UnitSpawnState>(bootstrapEntity).InitialSpawnDone)
            {
                return;
            }

            if (!SystemAPI.HasComponent<UnitPrefabReference>(bootstrapEntity))
            {
                return;
            }

            var prefab = SystemAPI.GetComponent<UnitPrefabReference>(bootstrapEntity).Prefab;
            if (prefab == Entity.Null)
            {
                return;
            }

            var config = SystemAPI.GetSingleton<ValidationRTSConfig>();
            var pool = SystemAPI.GetSingleton<UnitPoolConfig>();

            var randomSeed = (uint)math.max(1, (int)(SystemAPI.Time.ElapsedTime * 1000.0));
            var random = new Random(randomSeed);

            var entityManager = state.EntityManager;
            var defaultStats = UnitStats.CreateDefault(0, 0);

            byte factionCount = 1;
            if (SystemAPI.TryGetSingleton<UnitPalette>(out var palette) && palette.Value.IsCreated)
            {
                factionCount = (byte)math.max(1, palette.Value.Value.Colors.Length);
            }

            for (int i = 0; i < pool.InitialPoolSize; i++)
            {
                var unit = entityManager.Instantiate(prefab);
                var angle = random.NextFloat(0, math.PI * 2f);
                var radius = random.NextFloat(0.5f, config.SpawnRadius);
                var position = new float3(math.cos(angle) * radius, 0f, math.sin(angle) * radius);

                SetOrAdd(entityManager, unit, LocalTransform.FromPositionRotationScale(position, quaternion.identity, 1f));
                var stats = defaultStats;
                stats.FactionId = (byte)(i % factionCount);
                SetOrAdd(entityManager, unit, stats);
                SetOrAdd(entityManager, unit, new UnitVelocity { Value = float3.zero });
                SetOrAdd(entityManager, unit, new UnitSteering());
                SetOrAdd(entityManager, unit, new UnitCommandState
                {
                    DesiredPosition = position,
                    DesiredDirection = float3.zero,
                    Weight = 0f
                });
                SetOrAdd(entityManager, unit, new UnitTag
                {
                    UnitId = i,
                    FactionId = stats.FactionId
                });
            }

            var spawnState = SystemAPI.GetComponent<UnitSpawnState>(bootstrapEntity);
            spawnState.InitialSpawnDone = true;
            SystemAPI.SetComponent(bootstrapEntity, spawnState);

            static void SetOrAdd<T>(EntityManager manager, Entity entity, T componentData) where T : unmanaged, IComponentData
            {
                if (manager.HasComponent<T>(entity))
                {
                    manager.SetComponentData(entity, componentData);
                }
                else
                {
                    manager.AddComponentData(entity, componentData);
                }
            }
        }
    }
}
