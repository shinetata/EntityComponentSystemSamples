using Unity.Entities;

namespace ValidationRTS.Components
{
    public struct UnitTag : IComponentData
    {
        public int UnitId;
        public byte FactionId;
    }
}
