using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace HelloCube.JobChunk
{
    public partial struct RotationSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ExecuteIJobChunk>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var job = new RotationJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            state.Dependency = job.ScheduleParallel(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct RotationJob : IJobEntity
    {
        public float DeltaTime;

        void Execute(ref LocalTransform transform, in RotationSpeed rotationSpeed)
        {
            transform = transform.RotateY(rotationSpeed.RadiansPerSecond * DeltaTime);
        }
    }
}
