using Unity.Entities;
using Unity.Mathematics;

namespace ValidationRTS.Components
{
    public struct UnitVelocity : IComponentData
    {
        public float3 Value;
    }
}
