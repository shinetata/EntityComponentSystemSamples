#if UNITY_EDITOR
using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace HelloCube.CrossQuery
{
    [BurstCompile]
    public partial struct CrossQueryColorURPSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ExecuteCrossQuery>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (color, urpColor) in
                     SystemAPI.Query<RefRO<CrossQueryColor>, RefRW<URPMaterialPropertyBaseColor>>())
            {
                urpColor.ValueRW.Value = color.ValueRO.Value;
            }
        }
    }
}
#endif
