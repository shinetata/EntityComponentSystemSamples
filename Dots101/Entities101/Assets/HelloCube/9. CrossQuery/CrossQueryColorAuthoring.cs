using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace HelloCube.CrossQuery
{
    public class CrossQueryColorAuthoring : MonoBehaviour
    {
        public Color InitialColor = Color.white;

        class Baker : Baker<CrossQueryColorAuthoring>
        {
            public override void Bake(CrossQueryColorAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                var color = (Vector4)authoring.InitialColor;
                AddComponent(entity, new CrossQueryColor
                {
                    Value = new float4(color.x, color.y, color.z, color.w)
                });
                AddComponent(entity, new CrossQueryIndex());
            }
        }
    }

    public struct CrossQueryColor : IComponentData
    {
        public float4 Value;
    }

    public struct CrossQueryIndex : IComponentData
    {
        public int Value;
    }
}
