using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace HelloCube.CrossQuery
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [BurstCompile]
    public partial struct SpawnSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrefabCollection>();
            state.RequireForUpdate<ExecuteCrossQuery>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            var prefabCollection = SystemAPI.GetSingleton<PrefabCollection>();

            // spawn boxes
            for (int i = 0; i < 20; i++)
            {
                var entity = state.EntityManager.Instantiate(prefabCollection.Box);
                var velocity = state.EntityManager.GetComponentData<Velocity>(entity);
                var transform = state.EntityManager.GetComponentData<LocalTransform>(entity);
                var defaultColor = state.EntityManager.GetComponentData<DefaultColor>(entity);

                float4 colorValue;
                if (i < 10)
                {
                    // black box on left
                    velocity.Value = new float3(2, 0, 0);
                    var verticalOffset = i * 2;
                    transform.Position = new float3(-3, -8 + verticalOffset, 0);
                    defaultColor.Value = new float4(0, 0, 0, 1);
                    colorValue = defaultColor.Value;
                }
                else
                {
                    // white box on right
                    velocity.Value = new float3(-2, 0, 0);
                    var verticalOffset = (i - 10) * 2;
                    transform.Position = new float3(3, -8 + verticalOffset, 0);
                    defaultColor.Value = new float4(1, 1, 1, 1);
                    colorValue = defaultColor.Value;
                }

                state.EntityManager.SetComponentData(entity, velocity);
                state.EntityManager.SetComponentData(entity, transform);
                state.EntityManager.SetComponentData(entity, defaultColor);

                if (state.EntityManager.HasComponent<CrossQueryColor>(entity))
                {
                    state.EntityManager.SetComponentData(entity, new CrossQueryColor { Value = colorValue });
                }
                else
                {
                    state.EntityManager.AddComponentData(entity, new CrossQueryColor { Value = colorValue });
                }

                if (state.EntityManager.HasComponent<CrossQueryIndex>(entity))
                {
                    state.EntityManager.SetComponentData(entity, new CrossQueryIndex { Value = i });
                }
                else
                {
                    state.EntityManager.AddComponentData(entity, new CrossQueryIndex { Value = i });
                }
            }
        }
    }
}
