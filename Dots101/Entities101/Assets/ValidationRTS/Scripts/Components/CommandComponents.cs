using Unity.Entities;
using Unity.Mathematics;

namespace ValidationRTS.Components
{
    public struct GlobalCommandTarget : IComponentData
    {
        public float3 Position;
        public float Radius;
    }

    public struct CommandPulseTimer : IComponentData
    {
        public float Elapsed;
        public float PulseInterval;
    }

    public struct UnitCommandState : IComponentData
    {
        public float3 DesiredPosition;
        public float3 DesiredDirection;
        public float Weight;
    }
}
