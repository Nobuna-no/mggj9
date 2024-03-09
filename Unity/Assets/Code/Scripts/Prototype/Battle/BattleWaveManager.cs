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

    // private Spline m_scaledPath;
    // private SplineContainer m_container;

    private Dictionary<HealthBehaviour, BattlerData> m_activeBattlers = new Dictionary<HealthBehaviour, BattlerData>();
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

    private class BattlerData
    {
        public PoolableBehaviour PoolableBehaviour;
        public HealthBehaviour HealthBehaviour;
        public Rigidbody Rigidbody;
    }

    private void Awake()
    {
        GameObject spawnedEntity = new GameObject();
        spawnedEntity.name = "Spawner Gen";
        spawnedEntity.transform.position = Vector3.zero;

        // m_container = spawnedEntity.AddComponent<SplineContainer>();
    }

    private void Start()
    {
        Debug.Assert(WorldPerspectiveManager.IsSingletonValid);
        WorldPerspectiveManager.Instance.OnWorldPerspectiveChanged += SetWorldPerspective;
        SetWorldPerspective(WorldPerspectiveManager.Instance.ActiveBoundaries);
        BattleInit();
    }

    private void Update()
    {
        var targetPosition = m_playerCharacter.Position;

        foreach (var battler in m_activeBattlers.Values)
        {
            Vector3 direction = targetPosition - battler.Rigidbody.position;
            direction.y = 0f; // Make sure the rotation only affects the yaw

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                battler.Rigidbody.MoveRotation(Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f));
            }
        }
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
        m_activeBattlers.Clear();
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void BattleWaveStart()
    {
        if (!m_isInitialized)
        {
            BattleWaveInit(m_currentWaveIndex);
        }

        m_activeWavesDefinition = m_battlesCollection.Definitions[m_currentWaveIndex];
        m_remaningSequence = m_activeWavesDefinition.WavesSequence.Count;
        foreach (var sequence in m_activeWavesDefinition.WavesSequence)
        {
            StartCoroutine(SpawnSequenceRoutine(sequence));
        }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void BattleWaveStop()
    {
        foreach (var battler in m_activeBattlers.Values)
        {
            battler.HealthBehaviour.OnBehaviourBurial -= BattlerDespawn;
            battler.HealthBehaviour.Kill();
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

    //private void SpawnSequence()
    //{

    //}

    //private void PreparePathAndSpawnEntities(BattleWaveDefinition.Sequence sequence)
    //{
    //    if (sequence == null)
    //    {
    //        Debug.LogWarning("Sequence is not set.");
    //        return;
    //    }

    //    //m_scaledPath = new Spline(sequence.Spline.Path);
    //    //for (int j = 0; j < m_scaledPath.Count; j++)
    //    //{
    //    //    BezierKnot knot = m_scaledPath[j];
    //    //    knot.Position = m_boundariesDefinition.RemapPositionToBoundariesUnclamped(knot.Position);//Vector3.Scale(knot.Position, m_worldSize);
    //    //    m_scaledPath[j] = knot;
    //    //    m_scaledPath.SetTangentMode(TangentMode.AutoSmooth);
    //    //}
    //    //m_container.Spline = m_scaledPath;

    //    StartCoroutine(SpawnSequenceRoutine(sequence));
    //}

    private void SpawnBattler(BattleWaveDefinition.Sequence sequence, int index)
    {
        BattleMotionDefinition motion = sequence.Motion;
        float splineEval = (float)index / (sequence.SpawnCount - 1);
        Vector3 remappedOrigin = ComputeRemappedMotionPosition(motion.Origin, splineEval);
        Vector3 remappedDestination = ComputeRemappedMotionPosition(motion.Destination, splineEval);

        m_remainingOpponent++;
        PoolableBehaviour battler = PoolManager.Instance.SpawnObject(sequence.BattlerDefintion, remappedOrigin);
        HealthBehaviour hp = battler.GetComponent<HealthBehaviour>();
        Rigidbody rb = battler.GetComponent<Rigidbody>();
        Debug.Assert(hp && rb, this);
        m_activeBattlers.Add(hp, new BattlerData()
            {
                PoolableBehaviour = battler,
                HealthBehaviour = hp,
                Rigidbody = rb
            });
        hp.OnBehaviourBurial += BattlerDespawn;

        StartCoroutine(BattlerMotionRoutine(m_activeBattlers[hp], remappedDestination, motion.Duration, motion.Curve,
            !m_boundariesDefinition.IsPositionInBoundary(remappedDestination)));
    }

    private Vector3 ComputeRemappedMotionPosition(BattleMotionDefinition.MotionSpace motion, float indexRatio)
    {
        Vector3 remappedPos;
        if (motion.Type == BattleMotionDefinition.MotionSpace.SpaceType.Point)
        {
            remappedPos = m_boundariesDefinition.RemapPositionToBoundariesUnclamped(motion.Point);
        }
        else
        {
            Vector3 splinePosition = motion.SplineDefinition.Spline.EvaluatePosition(indexRatio) * motion.SplineScale;
            remappedPos = m_boundariesDefinition.RemapPositionToBoundariesUnclamped(motion.SplineOrigin + splinePosition);
        }

        return remappedPos;
    }

    //private void SpawnEntity(BattleWaveDefinition.OldSequence sequence)
    //{
    //    Vector3 scaledOrigin = m_boundariesDefinition.RemapPositionToBoundariesUnclamped(sequence.Tween.Origin);
    //    GameObject spawnedEntity = Instantiate(sequence.PROTO_PrefabToSpawn, scaledOrigin, Quaternion.identity);
    //    m_remainingOpponent++;

    //    HealthBehaviour hp = spawnedEntity.GetComponent<HealthBehaviour>();
    //    Rigidbody rb = spawnedEntity.GetComponent<Rigidbody>();
    //    Debug.Assert(hp && rb, this);

    //    m_activeBattlers.Add(hp, new BattlerData() { HealthBehaviour = hp, Rigidbody = rb });
    //    hp.OnBehaviourBurial += BattlerDespawn;

    //    //if (sequence.UseSpline)
    //    //{
    //    //    var animator = spawnedEntity.AddComponent<SplineAnimate>();
    //    //    animator.Container = m_container;
    //    //    animator.Duration = sequence.TweenDuration;
    //    //    animator.Easing = sequence.Spline.EasingMode;
    //    //    animator.Loop = sequence.Spline.LoopMode;
    //    //    animator.Play();
    //    //}
    //    //else
    //    //{
    //        Vector3 scaledDestination = m_boundariesDefinition.RemapPositionToBoundariesUnclamped(sequence.Tween.Destination);//Vector3.Scale(m_spawningSequence.Tween.Destination, m_worldSize);
    //        StartCoroutine(MoveToDestinationRoutine(spawnedEntity, scaledDestination, sequence.TweenDuration, sequence.Tween.Curve));
    //    //}

    //    if (sequence.DespawnWhenReachingDestinaton)
    //    {
    //        StartCoroutine(DespawnAfterTimeout(spawnedEntity, sequence.TweenDuration));
    //    }
    //}

    private void BattlerDespawn(HealthBehaviour behaviour)
    {
        if (!m_activeBattlers.ContainsKey(behaviour))
        {
            Debug.LogWarning($"HealthBehaviour {behaviour.gameObject.name} is calling `BattlerDespawn` but is not registered!", this);
            return;
        }

        // Returns to pool.
        m_activeBattlers[behaviour].PoolableBehaviour.IsActive = false;

        m_activeBattlers.Remove(behaviour);
        --m_remainingOpponent;
        if (m_activeBattlers.Count == 0)
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
        foreach (var battler in m_activeBattlers.Values)
        {
            battler.HealthBehaviour.OnBehaviourBurial -= BattlerDespawn;
            battler.PoolableBehaviour.IsActive = false;
        }
        m_activeBattlers.Clear();

        m_playerHealth.OnBurial.RemoveListener(OnPlayerBurial);
        m_onGameOver?.Invoke();

        m_isInitialized = false;

        if (m_gameModeStateMachine)
        {
            m_gameModeStateMachine.SetState(m_gameOverState);
        }
    }

    private IEnumerator BattlerMotionRoutine(BattlerData battler, Vector3 destination, float duration, AnimationCurve curve, bool despawnAtDest)
    {
        float t = 0f;
        Vector3 startPosition = battler.Rigidbody.position;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            battler.Rigidbody.position = Vector3.Lerp(startPosition, destination, curve.Evaluate(t));
            yield return null;

            if (battler.HealthBehaviour.IsDead)
            {
                yield break;
            }
        }

        battler.Rigidbody.position = destination;

        if (despawnAtDest)
        {
        }
    }

    private IEnumerator SpawnSequenceRoutine(BattleWaveDefinition.Sequence sequence)
    {
        yield return new WaitForSecondsRealtime(sequence.Delay);

        for (int i = 0, c = sequence.SpawnCount; i < c; ++i)
        {
            SpawnBattler(sequence, i);
            yield return new WaitForSeconds(sequence.SpawnOffset);
        }

        --m_remaningSequence;
    }

    //private IEnumerator DespawnAfterTimeout(GameObject entity, float delay)
    //{
    //    yield return new WaitForSecondsRealtime(delay);
    //    // Replace by pooling logic
    //    Destroy(entity);
    //    BattlerDespawn();
    //}

}
