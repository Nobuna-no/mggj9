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

    [SerializeField] private UnityEvent m_onAllWaveCompleted;
    [SerializeField] private UnityEvent m_onGameOver;

    private Dictionary<HealthBehaviour, BattlerData> m_activeBattlers = new Dictionary<HealthBehaviour, BattlerData>();
    [SerializeField, ReadOnly] private int m_remainingSequence = 0;
    [SerializeField, ReadOnly] private int m_remainingBattlers = 0;
    private bool m_isInitialized = false;

    [SerializeField] private GameModeStateMachine m_gameModeStateMachine;
    [SerializeField] private GameModeStateDefinition m_gameOverState;

    [Header("Player")]
    [SerializeField]
    private Character m_playerCharacter;

    private PlayerController m_playerController;
    private HealthBehaviour m_playerHealth;

    private int m_currentWaveIndex = 0;
    private Vector2 m_debugScrollview;

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
        Debug.Assert(WorldPerspectiveManager.IsSingletonValid, "WorldPerspectiveManager.IsSingletonValid");
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

        // m_playerController = m_playerCharacter.Controller as PlayerController;
        // Debug.Assert(m_playerController != null, "m_playerController != null");
        if (!m_playerCharacter.TryGetAbilityModule<HealthBehaviour>(out m_playerHealth))
        {
            Debug.LogError("m_playerCharacter doesn't have a valid HealthBehaviour");
        }
        // m_playerController.DisableInput();
        // ResetPlayerPosition();?

        m_playerHealth.OnBurial.AddListener(OnPlayerBurial);
        m_playerHealth.OnResurrection.AddListener(OnPlayerResurrection);
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
        m_remainingBattlers = 0;
        m_remainingSequence = 0;
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
        m_remainingSequence = m_activeWavesDefinition.WavesSequence.Count;
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
            UnsubscribeBattler(battler.HealthBehaviour);
            battler.HealthBehaviour.Kill();
        }
        m_activeBattlers.Clear();

        StopAllCoroutines();
        m_remainingBattlers = 0;
        m_remainingSequence = 0;

        m_isInitialized = false;

        if (++m_currentWaveIndex == m_battlesCollection.Definitions.Count)
        {
            m_onAllWaveCompleted?.Invoke();
            return;
        }

        if (m_startNextBattleAutomatically)
        {
            Debug.Log($"{this.name}: Automatically starting a new wave...");
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
        yield return new WaitForSeconds(time);
        BattleWaveInit(m_currentWaveIndex);
        BattleWaveStart();
    }

    private void SpawnBattler(BattleWaveDefinition.Sequence sequence, int index)
    {
        BattleMotionDefinition motion = sequence.Motion;
        Vector3 remappedOrigin = ComputeRemappedMotionPosition(motion.Origin, index, sequence.SpawnCount);
        Vector3 remappedDestination = ComputeRemappedMotionPosition(motion.Destination, index, sequence.SpawnCount);

        m_remainingBattlers++;
        PoolableBehaviour battler = PoolManager.Instance.SpawnObject(sequence.BattlerDefintion, remappedOrigin);
        HealthBehaviour hp = battler.GetComponent<HealthBehaviour>();
        Rigidbody rb = battler.GetComponent<Rigidbody>();
        Debug.Assert(hp && rb, "hp && rb", this);
        m_activeBattlers.Add(hp, new BattlerData()
        {
            PoolableBehaviour = battler,
            HealthBehaviour = hp,
            Rigidbody = rb
        });
        // Will only be called if not death.
        hp.OnBehaviourBurial += BattlerDespawn;
        hp.OnBehaviourDeath += BattlerDeath;

        StartCoroutine(BattlerMotionRoutine(m_activeBattlers[hp], remappedDestination, motion.Duration, motion.Curve,
            !m_boundariesDefinition.IsPositionInBoundary(remappedDestination)));
    }

    private Vector3 ComputeRemappedMotionPosition(BattleMotionDefinition.MotionSpace motion, float index, float totalIndexes)
    {
        Vector3 remappedPos;
        if (motion.Type == BattleMotionDefinition.MotionSpace.SpaceType.Point)
        {
            remappedPos = m_boundariesDefinition.RemapPositionToBoundariesUnclamped(motion.Point);
        }
        else
        {
            float splineProgress = (float)index / (motion.SplineDefinition.Spline.Closed ? totalIndexes : totalIndexes - 1);
            Vector3 splinePosition = motion.SplineDefinition.Spline.EvaluatePosition(splineProgress) * motion.SplineScale;
            remappedPos = m_boundariesDefinition.RemapPositionToBoundariesUnclamped(motion.SplineOrigin + splinePosition);
        }

        return remappedPos;
    }

    private void BattlerDeath(HealthBehaviour behaviour)
    {
        if (!m_activeBattlers.ContainsKey(behaviour))
        {
            Debug.LogWarning($"HealthBehaviour {behaviour.gameObject.name} is calling `BattlerDespawn` but is not registered!", this);
            return;
        }

        UnsubscribeBattler(behaviour);
        m_activeBattlers.Remove(behaviour);

        --m_remainingBattlers;
        if (m_remainingBattlers == 0 && m_remainingSequence == 0)
        {
            Debug.Log($"{this.name}: 0 remaining opponent, Spawner done");
            BattleWaveStop();
        }
        else
        {
            Debug.Log($"{this.name}: {m_remainingBattlers} remaining opponent(s) out of remaining {m_remainingSequence} sequence(s).");
        }
    }

    private void BattlerDespawn(HealthBehaviour behaviour)
    {
        if (!m_activeBattlers.ContainsKey(behaviour))
        {
            Debug.LogWarning($"HealthBehaviour {behaviour.gameObject.name} is calling `BattlerDespawn` but is not registered!", this);
            return;
        }

        m_activeBattlers[behaviour].PoolableBehaviour.IsActive = false;
        BattlerDeath(behaviour);
    }

    private void UnsubscribeBattler(HealthBehaviour behaviour)
    {
        behaviour.OnBehaviourDeath -= BattlerDeath;
        behaviour.OnBehaviourBurial -= BattlerDespawn;
    }

    private void OnPlayerBurial()
    {
        foreach (var battler in m_activeBattlers.Values)
        {
            UnsubscribeBattler(battler.HealthBehaviour);
            battler.PoolableBehaviour.IsActive = false;
        }
        m_activeBattlers.Clear();
        m_remainingBattlers = 0;
        m_remainingSequence = 0;
        m_isInitialized = false;

        m_playerHealth.OnBurial.RemoveListener(OnPlayerBurial);
        m_onGameOver?.Invoke();

        if (m_gameModeStateMachine)
        {
            m_gameModeStateMachine.SetState(m_gameOverState);
        }
    }

    private void OnPlayerResurrection()
    {
        m_playerHealth.OnBurial.AddListener(OnPlayerBurial);
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
            BattlerDespawn(battler.HealthBehaviour);
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

        --m_remainingSequence;
    }

    public void DrawManagerDebugIMGUI()
    {
        using (new GUILayout.VerticalScope())
        {
            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                GUILayout.HorizontalSlider(-1, 0, 0, GUILayout.Height(0.1f));
                GUILayout.Label("Settings");
                m_playerHealth.IsVulnerable = !GUILayout.Toggle(!m_playerHealth.IsVulnerable, "God Mode");
            }

            using (new GUILayout.VerticalScope(GUI.skin.box))
            {
                GUILayout.HorizontalSlider(-1, 0, 0, GUILayout.Height(0.1f));
                GUILayout.Label("Battle Waves");
                if (m_activeWavesDefinition != null)
                {
                    using (new GUILayout.VerticalScope(GUI.skin.window))
                    {
                        GUILayout.Label($"Active Wave: {m_activeWavesDefinition.name}");
                        GUILayout.Label($"Sequence: {m_remainingSequence}/{m_activeWavesDefinition.WavesSequence.Count}");
                        GUILayout.Label($"Remaining Battlers: {m_remainingBattlers}");
                        m_startNextBattleAutomatically = GUILayout.Toggle(m_startNextBattleAutomatically, "Start wave auto");
                    }
                }

                int i = 0;
                m_debugScrollview = GUILayout.BeginScrollView(m_debugScrollview);
                foreach (var bw in m_battlesCollection.Definitions)
                {
                    if (GUILayout.Button(bw.name))
                    {
                        var prevVal = m_startNextBattleAutomatically;
                        m_startNextBattleAutomatically = false;
                        BattleWaveStop();
                        m_currentWaveIndex = i;
                        BattleWaveStart();
                        m_startNextBattleAutomatically = prevVal;
                    }
                    ++i;
                }
                GUILayout.EndScrollView();
            }
        }
    }
}
