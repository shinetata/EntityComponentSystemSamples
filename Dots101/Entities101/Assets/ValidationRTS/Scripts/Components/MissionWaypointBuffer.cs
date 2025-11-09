using Unity.Entities;
using Unity.Mathematics;

namespace ValidationRTS.Components
{
    [InternalBufferCapacity(4)]
    public struct MissionWaypoint : IBufferElementData
    {
        public float3 Position;
        public float Weight;
    }
}
