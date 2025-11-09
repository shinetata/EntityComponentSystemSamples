using Unity.Entities;
using Unity.Mathematics;

namespace ValidationRTS.Components
{
    public struct WaypointAnchor : IComponentData
    {
        public float3 Position;
        public int Index;
    }
}
