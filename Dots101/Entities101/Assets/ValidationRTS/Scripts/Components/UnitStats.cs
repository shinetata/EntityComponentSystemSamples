using Unity.Entities;
using Unity.Mathematics;

namespace ValidationRTS.Components
{
    public struct UnitStats : IComponentData
    {
        public float MoveSpeed;
        public float Acceleration;
        public float Mass;
        public byte FactionId;
        public byte FormationId;

        public static UnitStats CreateDefault(byte factionId, byte formationId)
        {
            return new UnitStats
            {
                MoveSpeed = UnitConstants.DefaultMoveSpeed,
                Acceleration = UnitConstants.DefaultAcceleration,
                Mass = UnitConstants.DefaultMass,
                FactionId = factionId,
                FormationId = formationId
            };
        }

        public float3 ClampVelocity(float3 desired, float deltaTime)
        {
            var limit = math.max(0.01f, MoveSpeed);
            var accelLimit = math.max(0.01f, Acceleration) * math.max(0.0001f, deltaTime);
            var accelerated = math.normalizesafe(desired) * math.min(math.length(desired), accelLimit);
            var clamped = math.normalizesafe(accelerated) * math.min(math.length(accelerated), limit);
            return math.select(float3.zero, clamped, math.lengthsq(clamped) > 1e-5f);
        }
    }
}
