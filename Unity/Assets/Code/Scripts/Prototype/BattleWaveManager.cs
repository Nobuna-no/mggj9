using NaughtyAttributes;
using NobunAtelier;
using NobunAtelier.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

public class BattleWaveManager : MonoBehaviour
{
    [Header("Battle Wave Manager")]
    [SerializeField] private BattleWaveCollection m_battlesCollection;
    [SerializeField] private bool m_startNextBattleAutomatically = true;
    [SerializeField] private float m_delayBetweenWaves = 1f;
    private BattleWaveDefinition m_activeWavesDefinition;
    [SerializeField] private WorldBoundariesDefinition m_boundariesDefinition;

    [SerializeField] private UnityEvent m_onCurrentWaveCompleted;
    [SerializeField] private UnityEvent m_onAllWaveCompleted;
    [SerializeField] private UnityEvent m_onGameOver;

    private Spline m_scaledPath;
    private SplineContainer m_container;

    private List<HealthBehaviour> m_activeOpponent = new List<HealthBehaviour>();
    [SerializeField, ReadOnly] private int m_remaningSequence = 0;
    [SerializeField, ReadOnly] private int m_remainingOpponent = 0;
    private bool m_isInitialized = false;

    [SerializeField] private GameModeStateMachine m_gameModeStateMachine;
    [SerializeField] private GameModeStateDefinition m_gameOverState;

    [Header("Player")]
    [SerializeField]
    private Character m_playerCharacter;
    private PlayerController m_playerController;
    private HealthBehaviour m_playerHealth;


    private int m_currentWaveIndex = 0;

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
        BattleInit();
    }

    public void SetWorldPerspective(WorldBoundariesDefinition worldPerspectiveDefinition)
    {
        m_boundariesDefinition = worldPerspectiveDefinition;
    }

    private void BattleInit()
    {
        // Debug.Assert(m_gameModeStateMachine);

        m_playerController = m_playerCharacter.Controller as PlayerController;
        Debug.Assert(m_playerController != null);
        Debug.Assert(m_playerCharacter.TryGetAbilityModule<HealthBehaviour>(out m_playerHealth));

        // m_playerController.DisableInput();
        // ResetPlayerPosition();?

        m_playerHealth.OnBurial.AddListener(OnPlayerBurial);
        m_playerHealth.Reset();
        BattleWaveInit(0);
    }


    private void BattleWaveInit(int waveIndex)
    {
        if (m_isInitialized)
        {
            return;
        }

        m_isInitialized = true;
        m_currentWaveIndex = waveIndex;
        m_remainingOpponent = 0;
        m_activeOpponent.Clear();
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void BattleWaveStart()
    {
        if (!m_isInitialized)
        {
            BattleWaveInit(m_currentWaveIndex);
        }

        m_activeWavesDefinition = m_battlesCollection.Definitions[m_currentWaveIndex];
        SpawnSequence();
        // m_battlesCollection
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    // Can be use manually to skip a battle or used by the system when all opponent are dead.
    public void BattleWaveStop()
    {
        foreach (var aliveOp in m_activeOpponent)
        {
            if (aliveOp == null)
            {
                continue;
            }

            aliveOp.OnBurial.RemoveListener(OpponentDespawn);
            aliveOp.Kill();
        }

        m_onCurrentWaveCompleted?.Invoke();
        m_isInitialized = false;

        if (++m_currentWaveIndex == m_battlesCollection.Definitions.Count)
        {
            m_onAllWaveCompleted?.Invoke();
            return;
        }

        if (m_startNextBattleAutomatically)
        {
            StartCoroutine(BattleWaveStartDelayedRoutine(m_delayBetweenWaves));
        }
        else
        {
            // m_playerController.DisableInput();
            //m_battleWave[m_currentBattleIndex].SetGameModeState();
        }
    }

    private IEnumerator BattleWaveStartDelayedRoutine(float time)
    {
        Debug.Log($"{this.name}: BattleWaveStartDelayedRoutine timer start");
        yield return new WaitForSeconds(time);
        Debug.Log($"{this.name}: BattleWaveStartDelayedRoutine timer end");
        BattleWaveInit(m_currentWaveIndex);
        BattleWaveStart();
    }

    private void SpawnSequence()
    {
        m_onCurrentWaveCompleted?.Invoke();
        m_remaningSequence = m_activeWavesDefinition.WavesSequence.Count;
        foreach (var sequence in m_activeWavesDefinition.WavesSequence)
        {
            StartCoroutine(WaitAndSpawnSequence(sequence));
        }
    }

    private void PreparePathAndSpawnEntities(BattleWaveDefinition.Sequence sequence)
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

    private void SpawnEntity(BattleWaveDefinition.Sequence sequence)
    {
        Vector3 scaledOrigin = m_boundariesDefinition.RemapPositionToBoundariesUnclamped(sequence.Tween.Origin);
        GameObject spawnedEntity = Instantiate(sequence.PROTO_PrefabToSpawn, scaledOrigin, Quaternion.identity);
        m_remainingOpponent++;

        HealthBehaviour hp = spawnedEntity.GetComponent<HealthBehaviour>();
        if (hp != null)
        {
            hp.OnBurial.AddListener(OpponentDespawn);
            m_activeOpponent.Add(hp);
        }

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

    private void OpponentDespawn()
    {
        if (--m_remainingOpponent == 0)
        {
            Debug.Log($"{this.name}: 0 remaining opponent, Spawner done");
            BattleWaveStop();
        }
        else
        {
            Debug.Log($"{this.name}: {m_remainingOpponent} remaining opponent(s)");
        }
    }



    private void OnPlayerBurial()
    {
        foreach (var healthBehaviour in m_activeOpponent)
        {
            // op.DisableAI();
            // var healthBehaviour = op.GetComponent<HealthBehaviour>();
            if (healthBehaviour != null)
            {
                healthBehaviour.OnBurial.RemoveListener(OpponentDespawn);
            }
        }
        m_playerHealth.OnBurial.RemoveListener(OnPlayerBurial);
        m_onGameOver?.Invoke();

        m_isInitialized = false;

        if (m_gameModeStateMachine)
        {
            m_gameModeStateMachine.SetState(m_gameOverState);
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

    private IEnumerator WaitAndSpawnSequence(BattleWaveDefinition.Sequence sequence)
    {
        yield return new WaitForSecondsRealtime(sequence.Delay);
        PreparePathAndSpawnEntities(sequence);
        --m_remaningSequence;
    }

    private IEnumerator DespawnAfterTimeout(GameObject entity, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        // Replace by pooling logic
        Destroy(entity);
        OpponentDespawn();
    }

    private IEnumerator WaitAndSpawnEntity(BattleWaveDefinition.Sequence sequence)
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