using Unity.Entities;
using Unity.Mathematics;
using ValidationRTS.Components;

namespace ValidationRTS.Systems.Core
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct ValidationRTSInitializationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ValidationRTSConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (!SystemAPI.TryGetSingletonEntity<GlobalCommandTarget>(out _))
            {
                var entity = state.EntityManager.CreateEntity();
                state.EntityManager.AddComponentData(entity, new GlobalCommandTarget
                {
                    Position = float3.zero,
                    Radius = 10f
                });
                state.EntityManager.AddComponentData(entity, new CommandPulseTimer
                {
                    Elapsed = 0f,
                    PulseInterval = 2.5f
                });
            }
        }
    }
}
