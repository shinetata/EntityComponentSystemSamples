using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ValidationRTS.Components;

namespace ValidationRTS.Systems.Lifecycle
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(ValidationRTS.Systems.Navigation.UnitMovementSystem))]
    public partial struct ArenaBoundsSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ValidationRTSConfig>();
        }

        [BurstCompile]
        public partial struct ClampJob : IJobEntity
        {
            public float MaxRadius;

            private void Execute(ref LocalTransform transform)
            {
                var pos = transform.Position;
                var horizontal = new float2(pos.x, pos.z);
                var length = math.length(horizontal);
                if (length > MaxRadius)
                {
                    var clamped = horizontal / math.max(length, 0.0001f) * MaxRadius;
                    transform.Position = new float3(clamped.x, pos.y, clamped.y);
                }
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<ValidationRTSConfig>();
            var job = new ClampJob
            {
                MaxRadius = config.SpawnRadius * 1.2f
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}
