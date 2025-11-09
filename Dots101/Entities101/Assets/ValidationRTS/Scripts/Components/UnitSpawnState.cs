using Unity.Entities;

namespace ValidationRTS.Components
{
    public struct UnitSpawnState : IComponentData
    {
        public bool InitialSpawnDone;
    }
}
