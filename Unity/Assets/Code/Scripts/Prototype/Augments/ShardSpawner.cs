using NaughtyAttributes;
using NobunAtelier;
using System.Collections;
using UnityEngine;

public class ShardSpawner : UnityPoolBehaviour<PoolableShard>
{
    [Header("Shards Spawner")]
    [SerializeField, MinMaxSlider(0, 20f)] private Vector2 m_numberOfShards = Vector2.one;

    [SerializeField, MinMaxSlider(0, 10f)] private Vector2 m_spawnAreaRadius = Vector2.one; // Size of the spawn area
    [SerializeField, MinMaxSlider(0, 1f)] private Vector2 m_spawnDelayRange = Vector2.left; // Delay between spawning each shard

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void SpawnShards()
    {
        var boundaries = WorldPerspectiveManager.Instance.ActiveBoundaries;
        int randomShardCound = (int)Random.Range(m_numberOfShards.x, m_numberOfShards.y);
        for (int i = 0; i < randomShardCound; i++)
        {
            Vector3 spawnPosition = transform.position + Random.insideUnitSphere * Random.Range(m_spawnAreaRadius.x, m_spawnAreaRadius.y);
            spawnPosition = boundaries.ClampPositionToBoundaries(spawnPosition);

            StartCoroutine(WaitForSecondsRealtime(Random.Range(m_spawnDelayRange.x, m_spawnDelayRange.y), spawnPosition));
        }
    }

    private IEnumerator WaitForSecondsRealtime(float delay, Vector3 position)
    {
        yield return new WaitForSeconds(delay);

        PoolableShard shard = Pool.Get();
        shard.transform.SetPositionAndRotation(position, Quaternion.Euler(new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f))));
        shard.Spawn();
    }
}
