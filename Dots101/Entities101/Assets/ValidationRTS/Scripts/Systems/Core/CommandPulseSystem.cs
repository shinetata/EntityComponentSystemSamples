using Unity.Entities;
using Unity.Mathematics;
using ValidationRTS.Components;

namespace ValidationRTS.Systems.Core
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(UnitSpawnSystem))]
    public partial struct CommandPulseSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GlobalCommandTarget>();
            state.RequireForUpdate<ValidationRTSConfig>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            var target = SystemAPI.GetSingleton<GlobalCommandTarget>();
            var timer = SystemAPI.GetSingleton<CommandPulseTimer>();
            timer.Elapsed += deltaTime;

            if (timer.Elapsed >= timer.PulseInterval)
            {
                timer.Elapsed = 0f;
                var config = SystemAPI.GetSingleton<ValidationRTSConfig>();
                var random = new Random((uint)(1 + SystemAPI.Time.ElapsedTime * 1000));
                var radius = random.NextFloat(config.SpawnRadius * 0.25f, config.SpawnRadius);
                var angle = random.NextFloat(0, math.PI * 2f);
                target.Position = new float3(
                    math.cos(angle) * radius,
                    0f,
                    math.sin(angle) * radius);
            }

            SystemAPI.SetSingleton(target);
            SystemAPI.SetSingleton(timer);
        }
    }
}
