using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Profiling.LowLevel.Unsafe;

namespace HelloCube.StateChange
{
    public partial struct SetStateSystem : ISystem
    {
        static readonly float4 ColorRed = new float4(1f, 0f, 0f, 1f);
        static readonly float4 ColorWhite = new float4(1f, 1f, 1f, 1f);

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Hit>();
            state.RequireForUpdate<Config>();
            state.RequireForUpdate<ExecuteStateChange>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var config = SystemAPI.GetSingleton<Config>();
            var hit = SystemAPI.GetSingleton<Hit>();

            if (!hit.HitChanged)
            {
#if UNITY_EDITOR
                SystemAPI.GetSingletonRW<StateChangeProfilerModule.FrameData>().ValueRW.SetStatePerf = 0;
#endif
                return;
            }

            var radiusSq = config.Radius * config.Radius;

            state.Dependency.Complete();
            var before = ProfilerUnsafeUtility.Timestamp;

            if (config.Mode == Mode.VALUE)
            {
                new SetValueJob
                {
                    RadiusSq = radiusSq,
                    Hit = hit.Value,
                    ColorInside = ColorRed,
                    ColorOutside = ColorWhite
                }.ScheduleParallel();
            }
            else if (config.Mode == Mode.STRUCTURAL_CHANGE)
            {
                new AddSpinJob
                {
                    RadiusSq = radiusSq,
                    Hit = hit.Value,
                    ColorInside = ColorRed
                }.ScheduleParallel();

                new RemoveSpinJob
                {
                    RadiusSq = radiusSq,
                    Hit = hit.Value,
                    ColorOutside = ColorWhite
                }.ScheduleParallel();
            }
            else if (config.Mode == Mode.ENABLEABLE_COMPONENT)
            {
                new EnableSpinJob
                {
                    RadiusSq = radiusSq,
                    Hit = hit.Value,
                    ColorInside = ColorRed
                }.ScheduleParallel();

                new DisableSpinJob
                {
                    RadiusSq = radiusSq,
                    Hit = hit.Value,
                    ColorOutside = ColorWhite
                }.ScheduleParallel();
            }

            state.Dependency.Complete();
            var after = ProfilerUnsafeUtility.Timestamp;

#if UNITY_EDITOR
            // profiling
            var conversionRatio = ProfilerUnsafeUtility.TimestampToNanosecondsConversionRatio;
            var elapsed = (after - before) * conversionRatio.Numerator / conversionRatio.Denominator;
            SystemAPI.GetSingletonRW<StateChangeProfilerModule.FrameData>().ValueRW.SetStatePerf = elapsed;
#endif
        }
    }

    [WithAll(typeof(StateCubeTag))]
    [BurstCompile]
    partial struct SetValueJob : IJobEntity
    {
        public float RadiusSq;
        public float3 Hit;
        public float4 ColorInside;
        public float4 ColorOutside;

        void Execute(ref CubeColor color, ref SpinState spin, in LocalTransform transform)
        {
            var isInside = math.distancesq(transform.Position, Hit) <= RadiusSq;
            color.Value = isInside ? ColorInside : ColorOutside;
            spin.HasSpinComponent = true;
            spin.IsSpinning = isInside;
        }
    }

    [WithAll(typeof(StateCubeTag))]
    [BurstCompile]
    partial struct AddSpinJob : IJobEntity
    {
        public float RadiusSq;
        public float3 Hit;
        public float4 ColorInside;

        void Execute(ref CubeColor color, ref SpinState spin, in LocalTransform transform)
        {
            // If cube is inside the hit radius.
            if (math.distancesq(transform.Position, Hit) <= RadiusSq)
            {
                color.Value = ColorInside;
                spin.HasSpinComponent = true;
                spin.IsSpinning = true;
            }
        }
    }

    [WithAll(typeof(StateCubeTag))]
    [BurstCompile]
    partial struct RemoveSpinJob : IJobEntity
    {
        public float RadiusSq;
        public float3 Hit;
        public float4 ColorOutside;

        void Execute(ref CubeColor color, ref SpinState spin, in LocalTransform transform)
        {
            var isInside = math.distancesq(transform.Position, Hit) <= RadiusSq;
            if (isInside)
                return;

            color.Value = ColorOutside;
            spin.HasSpinComponent = false;
            spin.IsSpinning = false;
        }
    }

    [WithAll(typeof(StateCubeTag))]
    [BurstCompile]
    public partial struct EnableSpinJob : IJobEntity
    {
        public float RadiusSq;
        public float3 Hit;
        public float4 ColorInside;

        void Execute(ref CubeColor color, ref SpinState spin, in LocalTransform transform)
        {
            // If cube is inside the hit radius.
            if (math.distancesq(transform.Position, Hit) <= RadiusSq)
            {
                color.Value = ColorInside;
                spin.HasSpinComponent = true;
                spin.IsSpinning = true;
            }
        }
    }

    [WithAll(typeof(StateCubeTag))]
    [BurstCompile]
    public partial struct DisableSpinJob : IJobEntity
    {
        public float RadiusSq;
        public float3 Hit;
        public float4 ColorOutside;

        void Execute(ref CubeColor color, ref SpinState spin, in LocalTransform transform)
        {
            // If cube is NOT inside the hit radius.
            if (math.distancesq(transform.Position, Hit) > RadiusSq)
            {
                color.Value = ColorOutside;
                spin.IsSpinning = false;
            }
        }
    }
}
