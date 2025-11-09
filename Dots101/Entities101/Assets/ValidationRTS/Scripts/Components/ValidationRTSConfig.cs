using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace ValidationRTS.Components
{
    public struct ValidationRTSConfig : IComponentData
    {
        public float SpawnRadius;
        public float FixedStepSeconds;
    }

    public struct UnitPoolConfig : IComponentData
    {
        public int InitialPoolSize;
    }

    public struct UnitPrefabReference : IComponentData
    {
        public Entity Prefab;
    }

    public struct UnitPalette : IComponentData
    {
        public BlobAssetReference<UnitPaletteBlob> Value;
    }

    public struct UnitPaletteBlob
    {
        public BlobArray<float4> Colors;
    }
}
