using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using ValidationRTS.Components;

namespace ValidationRTS.Authoring
{
    [DisallowMultipleComponent]
    public class ValidationRTSBootstrap : MonoBehaviour
    {
        [Header("Prefabs & Mesh")]
        public GameObject unitPrefab;
        public Mesh unitMesh;

        [Header("Visuals")]
        public Color[] factionColors = { Color.white };

        [Header("Simulation")]
        [Min(0.01f)]
        public float spawnRadius = 25f;

        [Range(0.005f, 0.1f)]
        public float fixedStepSeconds = 0.02f;

        [Header("Pooling")]
        [Min(0)]
        public int initialPoolSize = 256;

        private class Baker : Unity.Entities.Baker<ValidationRTSBootstrap>
        {
            public override void Bake(ValidationRTSBootstrap authoring)
            {
                var bootstrapEntity = GetEntity(TransformUsageFlags.None);

                AddComponent(bootstrapEntity, new ValidationRTSConfig
                {
                    SpawnRadius = math.max(1f, authoring.spawnRadius),
                    FixedStepSeconds = math.clamp(authoring.fixedStepSeconds, 0.005f, 0.1f)
                });

                AddComponent(bootstrapEntity, new UnitPoolConfig
                {
                    InitialPoolSize = math.max(0, authoring.initialPoolSize)
                });
                AddComponent(bootstrapEntity, new UnitSpawnState { InitialSpawnDone = false });

                if (authoring.unitPrefab != null)
                {
                    var prefabEntity = GetEntity(authoring.unitPrefab, TransformUsageFlags.Dynamic);
                    AddComponent(bootstrapEntity, new UnitPrefabReference { Prefab = prefabEntity });
                }

                if (authoring.factionColors != null && authoring.factionColors.Length > 0)
                {
                    using var builder = new BlobBuilder(Allocator.Temp);
                    ref var paletteRoot = ref builder.ConstructRoot<UnitPaletteBlob>();
                    var colors = builder.Allocate(ref paletteRoot.Colors, authoring.factionColors.Length);

                    for (int i = 0; i < authoring.factionColors.Length; i++)
                    {
                        var color = authoring.factionColors[i];
                        colors[i] = new float4(color.r, color.g, color.b, color.a);
                    }

                    var paletteRef = builder.CreateBlobAssetReference<UnitPaletteBlob>(Allocator.Persistent);
                    AddComponent(bootstrapEntity, new UnitPalette { Value = paletteRef });
                }
            }
        }
    }
}
