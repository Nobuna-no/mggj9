using System.Collections;
using NaughtyAttributes;
using UnityEngine;

public class ShardSpawner : UnityPoolBehaviour<PoolableShard>
{
    [Header("Shards Spawner")]
    [SerializeField] private int m_numberOfShards = 10;
    [SerializeField, MinMaxSlider(0, 10)] private Vector2 m_spawnAreaRadius = Vector2.one; // Size of the spawn area
    [SerializeField, MinMaxSlider(0,1)] private Vector2 m_spawnDelayRange = Vector2.left; // Delay between spawning each shard

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void SpawnShards()
    {
        var boundaries = WorldBoundariesDirector.Instance.ActiveBoundaries;

        for (int i = 0; i < m_numberOfShards; i++)
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * Random.Range(m_spawnAreaRadius.x, m_spawnAreaRadius.y);
            spawnPosition = boundaries.ClampPositionToBoundaries(spawnPosition);

            StartCoroutine(WaitForSecondsRealtime(Random.Range(m_spawnDelayRange.x, m_spawnDelayRange.y), spawnPosition));
        }
    }

    IEnumerator WaitForSecondsRealtime(float delay, Vector3 position)
    {
        yield return new WaitForSeconds(delay);

        PoolableShard shard = Pool.Get();
        shard.transform.SetPositionAndRotation(position, Quaternion.Euler(new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f))));
        shard.Spawn();
    }

}
