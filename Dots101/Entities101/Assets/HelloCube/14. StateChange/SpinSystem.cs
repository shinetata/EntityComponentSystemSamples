using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Profiling.LowLevel.Unsafe;

namespace HelloCube.StateChange
{
    public partial struct SpinSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<ExecuteStateChange>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            SystemAPI.GetSingleton<Config>();

            state.Dependency.Complete();
            var before = ProfilerUnsafeUtility.Timestamp;

            new SpinJob
            {
                Offset = quaternion.RotateY(SystemAPI.Time.DeltaTime * math.PI)
            }.ScheduleParallel();

            state.Dependency.Complete();
            var after = ProfilerUnsafeUtility.Timestamp;

#if UNITY_EDITOR
            // profiling
            var conversionRatio = ProfilerUnsafeUtility.TimestampToNanosecondsConversionRatio;
            var elapsed = (after - before) * conversionRatio.Numerator / conversionRatio.Denominator;
            SystemAPI.GetSingletonRW<StateChangeProfilerModule.FrameData>().ValueRW.SpinPerf = elapsed;
#endif
        }
    }

    [WithAll(typeof(StateCubeTag))]
    [BurstCompile]
    partial struct SpinJob : IJobEntity
    {
        public quaternion Offset;

        void Execute(ref LocalTransform transform, in SpinState spin)
        {
            if (spin.HasSpinComponent && spin.IsSpinning)
            {
                transform = transform.Rotate(Offset);
            }
        }
    }
}
