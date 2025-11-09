using Unity.Entities;
using UnityEngine;
using ValidationRTS.Components;

namespace ValidationRTS.Authoring
{
    public class FormationAuthoring : MonoBehaviour
    {
        public int formationId = 0;
        public byte factionId = 0;
        public Vector3 offset = Vector3.zero;

        private class Baker : Unity.Entities.Baker<FormationAuthoring>
        {
            public override void Bake(FormationAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FormationAnchor
                {
                    FormationId = authoring.formationId,
                    FactionId = authoring.factionId,
                    Offset = authoring.offset
                });
            }
        }
    }
}
