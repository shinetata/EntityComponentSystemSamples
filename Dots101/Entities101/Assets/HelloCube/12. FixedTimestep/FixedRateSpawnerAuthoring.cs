using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace HelloCube.FixedTimestep
{
    public class FixedRateSpawnerAuthoring : MonoBehaviour
    {
        public GameObject projectilePrefab;
        public float spawnIntervalSeconds = 1f / 60f;

        class Baker : Baker<FixedRateSpawnerAuthoring>
        {
            public override void Bake(FixedRateSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var spawnerData = new FixedRateSpawner
                {
                    Prefab = GetEntity(authoring.projectilePrefab, TransformUsageFlags.Dynamic),
                    SpawnPos = GetComponent<Transform>().position,
                    SpawnInterval = math.max(0.01f, authoring.spawnIntervalSeconds),
                    Accumulator = 0f
                };
                AddComponent(entity, spawnerData);
            }
        }
    }

    public struct FixedRateSpawner : IComponentData
    {
        public Entity Prefab;
        public float3 SpawnPos;
        public float SpawnInterval;
        public float Accumulator;
    }
}
