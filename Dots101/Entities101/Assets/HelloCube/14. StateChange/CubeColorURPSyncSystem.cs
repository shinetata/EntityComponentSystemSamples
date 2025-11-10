using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace HelloCube.StateChange
{
    [BurstCompile]
    public partial struct CubeColorURPSyncSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ExecuteStateChange>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (color, urpColor) in
                     SystemAPI.Query<RefRO<CubeColor>, RefRW<URPMaterialPropertyBaseColor>>())
            {
                urpColor.ValueRW.Value = color.ValueRO.Value;
            }
        }
    }
}
