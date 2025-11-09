using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ValidationRTS.Components;

namespace ValidationRTS.Systems.Navigation
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(BoidsSenseSystem))]
    public partial struct UnitMovementSystem : ISystem
    {
        [BurstCompile]
        public partial struct MovementJob : IJobEntity
        {
            public float DeltaTime;

            private void Execute(
                ref LocalTransform transform,
                ref UnitVelocity velocity,
                ref UnitSteering steering,
                in UnitCommandState command,
                in UnitStats stats)
            {
                var desired = steering.Cohesion + steering.Alignment + steering.Separation
                              + command.DesiredDirection * math.saturate(command.Weight);
                var nextVelocity = stats.ClampVelocity(desired, DeltaTime);
                transform.Position += nextVelocity * DeltaTime;

                if (math.lengthsq(nextVelocity) > 1e-4f)
                {
                    transform.Rotation = quaternion.LookRotationSafe(math.normalizesafe(nextVelocity), UnitConstants.DefaultUp);
                }

                velocity.Value = nextVelocity;
                steering.Reset();
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var job = new MovementJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}
