using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace HelloCube.StateChange
{
    public partial struct CubeSpawnSystem : ISystem
    {
        Config priorConfig;
        static readonly float4 ColorWhite = new float4(1f, 1f, 1f, 1f);

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Config>();
            state.RequireForUpdate<ExecuteStateChange>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<Config>();

            if (ConfigEquals(priorConfig, config))
            {
                return;
            }
            priorConfig = config;

            var stateCubeQuery = SystemAPI.QueryBuilder().WithAll<StateCubeTag>().Build();
            state.EntityManager.DestroyEntity(stateCubeQuery);

            var center = (config.Size - 1) / 2f;
            var count = (int)(config.Size * config.Size);
            for (int i = 0; i < count; i++)
            {
                var entity = state.EntityManager.Instantiate(config.Prefab);

                if (!state.EntityManager.HasComponent<StateCubeTag>(entity))
                {
                    state.EntityManager.AddComponent<StateCubeTag>(entity);
                }

                var transform = state.EntityManager.GetComponentData<LocalTransform>(entity);
                transform.Scale = 1;
                transform.Position.x = (i % config.Size - center) * 1.5f;
                transform.Position.z = (i / config.Size - center) * 1.5f;
                state.EntityManager.SetComponentData(entity, transform);

                var spinState = new SpinState
                {
                    HasSpinComponent = config.Mode != Mode.STRUCTURAL_CHANGE,
                    IsSpinning = false
                };

                if (state.EntityManager.HasComponent<SpinState>(entity))
                {
                    state.EntityManager.SetComponentData(entity, spinState);
                }
                else
                {
                    state.EntityManager.AddComponentData(entity, spinState);
                }

                var color = new CubeColor { Value = ColorWhite };
                if (state.EntityManager.HasComponent<CubeColor>(entity))
                {
                    state.EntityManager.SetComponentData(entity, color);
                }
                else
                {
                    state.EntityManager.AddComponentData(entity, color);
                }
            }
        }

        bool ConfigEquals(Config c1, Config c2)
        {
            return c1.Size == c2.Size && c1.Radius == c2.Radius && c1.Mode == c2.Mode;
        }
    }
}
