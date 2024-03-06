using NaughtyAttributes;
using NobunAtelier;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class EnemySpawnerDirector : MonoBehaviour
{
    [SerializeField] private EnemyWavesDefinition m_activeWavesDefinition;
    [SerializeField] private WorldBoundariesDefinition m_boundariesDefinition;

    private Spline m_scaledPath;
    private SplineContainer m_container;

    private void Awake()
    {
        GameObject spawnedEntity = new GameObject();
        spawnedEntity.name = "Spawner Gen";
        spawnedEntity.transform.position = Vector3.zero;

        m_container = spawnedEntity.AddComponent<SplineContainer>();
    }

    public void Start()
    {
        Debug.Assert(WorldPerspectiveManager.IsSingletonValid);
        WorldPerspectiveManager.Instance.OnWorldPerspectiveChanged += SetWorldPerspective;
        SetWorldPerspective(WorldPerspectiveManager.Instance.ActiveBoundaries);
    }

    public void SetWorldPerspective(WorldBoundariesDefinition worldPerspectiveDefinition)
    {
        m_boundariesDefinition = worldPerspectiveDefinition;
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void SpawnSequence()
    {
        foreach (var sequence in m_activeWavesDefinition.WavesSequence)
        {
            StartCoroutine(WaitAndSpawnSequence(sequence));
        }
    }

    // [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void SpawnEntities(EnemyWavesDefinition.Sequence sequence)
    {
        if (sequence == null)
        {
            Debug.LogWarning("Sequence is not set.");
            return;
        }

        m_scaledPath = new Spline(sequence.Spline.Path);
        for (int j = 0; j < m_scaledPath.Count; j++)
        {
            BezierKnot knot = m_scaledPath[j];
            knot.Position = m_boundariesDefinition.RemapPositionToBoundariesUnclamped(knot.Position);//Vector3.Scale(knot.Position, m_worldSize);
            m_scaledPath[j] = knot;
            m_scaledPath.SetTangentMode(TangentMode.AutoSmooth);
        }
        m_container.Spline = m_scaledPath;

        StartCoroutine(WaitAndSpawnEntity(sequence));
    }

    //private Vector3 RemapPositionToBoundaries(Vector3 position)
    //{
    //    // Remap the position to the un-clamped boundaries.
    //    return new Vector3(
    //        Mathf.LerpUnclamped(m_boundariesDefinition.AxisRangeX.x, m_boundariesDefinition.AxisRangeX.y,
    //            Mathf.InverseLerp(-1f, 1f, position.x) + Mathf.Sign(position.x) * (Mathf.Abs(position.x) > 1 ? Mathf.Abs(position.x) - 1 : 0)),
    //        Mathf.LerpUnclamped(m_boundariesDefinition.AxisRangeY.x, m_boundariesDefinition.AxisRangeY.y,
    //            Mathf.InverseLerp(-1f, 1f, position.y) + Mathf.Sign(position.y) * (Mathf.Abs(position.y) > 1 ? Mathf.Abs(position.y) - 1 : 0)),
    //        Mathf.LerpUnclamped(m_boundariesDefinition.AxisRangeZ.x, m_boundariesDefinition.AxisRangeZ.y,
    //            Mathf.InverseLerp(-1f, 1f, position.z) + Mathf.Sign(position.z) * (Mathf.Abs(position.z) > 1 ? Mathf.Abs(position.z) - 1 : 0))
    //        );
    //}

    private void SpawnEntity(EnemyWavesDefinition.Sequence sequence)
    {
        Vector3 scaledOrigin = m_boundariesDefinition.RemapPositionToBoundariesUnclamped(sequence.Tween.Origin);
        GameObject spawnedEntity = Instantiate(sequence.PROTO_PrefabToSpawn, scaledOrigin, Quaternion.identity);

        if (sequence.UseSpline)
        {
            var animator = spawnedEntity.AddComponent<SplineAnimate>();
            animator.Container = m_container;
            animator.Duration = sequence.TweenDuration;
            animator.Easing = sequence.Spline.EasingMode;
            animator.Loop = sequence.Spline.LoopMode;
            animator.Play();
        }
        else
        {
            Vector3 scaledDestination = m_boundariesDefinition.RemapPositionToBoundariesUnclamped(sequence.Tween.Destination);//Vector3.Scale(m_spawningSequence.Tween.Destination, m_worldSize);
            StartCoroutine(MoveToDestination(spawnedEntity, scaledDestination, sequence.TweenDuration, sequence.Tween.Curve));
        }

        if (sequence.DespawnWhenReachingDestinaton)
        {
            StartCoroutine(DespawnAfterTimeout(spawnedEntity, sequence.TweenDuration));
        }
    }

    private IEnumerator MoveToDestination(GameObject entity, Vector3 destination, float duration, AnimationCurve curve)
    {
        float t = 0f;
        Vector3 startPosition = entity.transform.position;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            entity.transform.position = Vector3.Lerp(startPosition, destination, curve.Evaluate(t));
            yield return null;
        }

        entity.transform.position = destination;

    }

    private IEnumerator WaitAndSpawnSequence(EnemyWavesDefinition.Sequence sequence)
    {
        yield return new WaitForSecondsRealtime(sequence.Delay);
        SpawnEntities(sequence);
    }

    private IEnumerator DespawnAfterTimeout(GameObject entity, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        // Replace by pooling logic
        Destroy(entity);
    }

    private IEnumerator WaitAndSpawnEntity(EnemyWavesDefinition.Sequence sequence)
    {
        int remainingSpawn = sequence.EntityToSpawnCount;

        do
        {
            SpawnEntity(sequence);
            --remainingSpawn;
            yield return new WaitForSeconds(sequence.SpawnDelay);
        }
        while (remainingSpawn > 0);
    }
}