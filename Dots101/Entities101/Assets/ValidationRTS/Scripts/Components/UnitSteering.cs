using Unity.Entities;
using Unity.Mathematics;

namespace ValidationRTS.Components
{
    public struct UnitSteering : IComponentData
    {
        public float3 Cohesion;
        public float3 Separation;
        public float3 Alignment;

        public void Reset()
        {
            Cohesion = float3.zero;
            Separation = float3.zero;
            Alignment = float3.zero;
        }
    }
}
