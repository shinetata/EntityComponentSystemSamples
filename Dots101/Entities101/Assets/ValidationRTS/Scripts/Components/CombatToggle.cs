using Unity.Entities;

namespace ValidationRTS.Components
{
    public struct CombatToggle : IComponentData
    {
        public bool Active;

        public static CombatToggle Enabled => new CombatToggle { Active = true };
        public static CombatToggle Disabled => new CombatToggle { Active = false };
    }
}
