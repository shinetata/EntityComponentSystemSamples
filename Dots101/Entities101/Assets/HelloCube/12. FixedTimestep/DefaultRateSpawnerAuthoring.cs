using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace HelloCube.FixedTimestep
{
    public class DefaultRateSpawnerAuthoring : MonoBehaviour
    {
        public GameObject projectilePrefab;
        public float spawnIntervalSeconds = 0.2f;

        class Baker : Baker<DefaultRateSpawnerAuthoring>
        {
            public override void Bake(DefaultRateSpawnerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);

                var spawnerData = new DefaultRateSpawner
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

    public struct DefaultRateSpawner : IComponentData
    {
        public Entity Prefab;
        public float3 SpawnPos;
        public float SpawnInterval;
        public float Accumulator;
    }
}
