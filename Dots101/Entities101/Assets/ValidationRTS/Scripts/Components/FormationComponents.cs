using Unity.Entities;
using Unity.Mathematics;

namespace ValidationRTS.Components
{
    public struct FormationAnchor : IComponentData
    {
        public int FormationId;
        public float3 Offset;
        public byte FactionId;
    }

    public struct FormationAssignment : IComponentData
    {
        public int FormationId;
        public float3 TargetOffset;
    }
}
