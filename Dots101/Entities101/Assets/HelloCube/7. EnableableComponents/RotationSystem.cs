using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace HelloCube.EnableableComponents
{
    public partial struct RotationSystem : ISystem
    {
        float timer;
        const float interval = 1.3f;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            timer = interval;
            state.RequireForUpdate<ExecuteEnableableComponents>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            timer -= deltaTime;

            // Toggle the enabled state of every RotationSpeed
            if (timer < 0)
            {
                foreach (var toggle in SystemAPI.Query<RefRW<RotationToggle>>())
                {
                    toggle.ValueRW.IsEnabled = !toggle.ValueRO.IsEnabled;
                }

                timer = interval;
            }

            // Entities rotate only when their toggle is set to true.
            foreach (var (transform, speed, toggle) in
                     SystemAPI.Query<RefRW<LocalTransform>, RefRO<RotationSpeed>, RefRO<RotationToggle>>())
            {
                if (!toggle.ValueRO.IsEnabled)
                {
                    continue;
                }

                transform.ValueRW = transform.ValueRO.RotateY(
                    speed.ValueRO.RadiansPerSecond * deltaTime);
            }
        }
    }
}
