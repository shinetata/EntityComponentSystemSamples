using Unity.Mathematics;

namespace ValidationRTS.Components
{
    /// <summary>
    /// Common constants shared across unit systems.
    /// </summary>
    public static class UnitConstants
    {
        public const float DefaultMoveSpeed = 4f;
        public const float DefaultAcceleration = 12f;
        public const float DefaultMass = 1f;
        public const float DefaultSeparationRadius = 1.5f;
        public const float DefaultCohesionWeight = 0.65f;
        public const float DefaultAlignmentWeight = 0.35f;

        public static readonly float3 DefaultUp = math.up();
    }
}
