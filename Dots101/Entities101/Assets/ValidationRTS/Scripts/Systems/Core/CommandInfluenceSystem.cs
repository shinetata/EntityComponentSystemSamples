using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using ValidationRTS.Components;

namespace ValidationRTS.Systems.Core
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(CommandPulseSystem))]
    public partial struct CommandInfluenceSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GlobalCommandTarget>();
        }

        [BurstCompile]
        public partial struct CommandJob : IJobEntity
        {
            public float3 TargetPosition;

            private void Execute(ref UnitCommandState command, in LocalTransform transform)
            {
                var direction = TargetPosition - transform.Position;
                command.DesiredPosition = TargetPosition;
                command.DesiredDirection = math.normalizesafe(direction);
                command.Weight = math.saturate(math.length(direction));
            }
        }

        public void OnUpdate(ref SystemState state)
        {
            var target = SystemAPI.GetSingleton<GlobalCommandTarget>();

            var job = new CommandJob
            {
                TargetPosition = target.Position
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }
}
