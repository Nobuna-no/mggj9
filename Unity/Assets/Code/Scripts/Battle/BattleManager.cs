using NobunAtelier;
using NobunAtelier.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* Change to do:
 * - Need to be able to inject wave
 * - Need to be able to inject rule
 *
 * Responsability:
 * - Injects the active wave sequence to the EnemySpawnerManager
 * - Rule and GameMode State handling: Listen to the enemies and player
 *      - Is wave finished?
 *      - Has player died?
 */

public class BattleManager : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField, TextArea]
    private string m_description;
#endif

    [Header("Player")]
    [SerializeField]
    private Character m_playerCharacter;
    [SerializeField]
    private HealthBehaviour m_playerHealth;
    [SerializeField]
    private CharacterSimpleTargetingAbility m_targetingAbility;
    [SerializeField]
    private Transform m_playerStart;

    [Header("Battle")]
    [SerializeField]
    private BattleWave[] m_battleWave;

    [SerializeField]
    private GameModeStateMachine m_stateMachineReference;

    [SerializeField]
    private GameModeStateDefinition m_gameoverState;

    public UnityEvent OnCurrentBattleWaveDone;
    public UnityEvent OnAllBattleWavesDone;

    private int m_currentBattleIndex = 0;

    private PlayerController m_playerController;

    private List<LegacyAIControllerBase> m_currentOpponentsBrain = new List<LegacyAIControllerBase>();
    private int m_remainingOpponent = 0;
    private bool m_isInitialized = false;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    public void BattleInit()
    {
        gameObject.SetActive(true);
        Debug.Assert(m_stateMachineReference);

        m_playerController = m_playerCharacter.Controller as PlayerController;
        Debug.Assert(m_playerController != null);
        if (m_playerHealth == null)
        {
            m_playerHealth = m_playerCharacter.GetComponent<HealthBehaviour>();
        }
        Debug.Assert(m_playerHealth != null);

        m_playerController.DisableInput();
        ResetPlayerPosition();

        foreach (var bw in m_battleWave)
        {
            bw.WaveParent.SetActive(false);
        }

        m_playerHealth.OnBurial.AddListener(OnPlayerBurial);
        m_playerHealth.Reset();
        BattleWaveInit(0);
    }

    public void BattleStartNextBattleWave()
    {
        Debug.Assert(m_playerController != null);
        m_playerController.EnableInput();
        BattleWaveStart();
    }

    public void ResetPlayerPosition()
    {
        m_playerCharacter.ResetCharacter(m_playerStart);
    }

    public void BattleResume()
    {
        m_playerController.EnableInput();
        foreach (var op in m_currentOpponentsBrain)
        {
            op.EnableAI();
        }
    }

    public void BattlePause()
    {
        m_playerController.DisableInput();
        foreach (var op in m_currentOpponentsBrain)
        {
            op.DisableAI();
        }
    }

    // Hide the whole Battle
    public void BattleClose()
    {
        gameObject.SetActive(false);
        Debug.Assert(m_playerController != null);
        m_playerController.DisableInput();
        m_playerHealth.OnBurial.RemoveListener(OnPlayerBurial);
    }

    private void BattleWaveInit(int waveIndex)
    {
        if (m_isInitialized)
        {
            return;
        }

        m_isInitialized = true;
        m_currentBattleIndex = waveIndex;
        m_currentOpponentsBrain.Clear();
        m_battleWave[m_currentBattleIndex].WaveParent.SetActive(true);

        m_currentOpponentsBrain.AddRange(m_battleWave[m_currentBattleIndex].WaveParent.GetComponentsInChildren<LegacyAIControllerBase>());
        m_remainingOpponent = m_currentOpponentsBrain.Count;

        foreach (var op in m_currentOpponentsBrain)
        {
            op.DisableAI();
            var healthBehaviour = op.GetComponent<HealthBehaviour>();
            if (healthBehaviour != null)
            {
                healthBehaviour.Reset();
                // healthBehaviour.OnDeath.AddListener(OnOpponentDeath);
                healthBehaviour.OnBurial.AddListener(OnBurial);
            }
        }
    }

    // Enable AI
    private void BattleWaveStart()
    {
        if (!m_isInitialized)
        {
            BattleWaveInit(m_currentBattleIndex);
        }

        foreach (var op in m_currentOpponentsBrain)
        {
            op.EnableAI();
        }

        Debug.Assert(m_targetingAbility);
        m_targetingAbility.NextTarget();
        m_targetingAbility.RefreshTarget();
    }

    // Disable AI and remove listener to death
    // Can be use manually to skip a battle or used by the system when all opponent are dead.
    public void BattleWaveStop()
    {
        if (!m_battleWave[m_currentBattleIndex].WaveParent.gameObject.activeInHierarchy)
        {
            Debug.Log($"{this.name}: this battle wave (index = {m_currentBattleIndex}) has already been stopped");
            return;
        }

        m_battleWave[m_currentBattleIndex].WaveParent.SetActive(false);

        foreach (var op in m_currentOpponentsBrain)
        {
            op.DisableAI();
            var healthBehaviour = op.GetComponent<HealthBehaviour>();
            if (healthBehaviour != null)
            {
                healthBehaviour.OnBurial.RemoveListener(OnBurial);
            }
        }

        OnCurrentBattleWaveDone?.Invoke();
        m_isInitialized = false;


        if (m_battleWave[m_currentBattleIndex].StartNextWaveAutomatically)
        {
            StartCoroutine(BattleStartDelayed_Coroutine(m_battleWave[m_currentBattleIndex].DelayBeforeNextWave));
        }
        else
        {
            m_playerController.DisableInput();
            m_battleWave[m_currentBattleIndex].SetGameModeState();
            m_currentBattleIndex++;
            m_isInitialized = false;
        }

        if (m_currentBattleIndex >= m_battleWave.Length)
        {
            OnAllBattleWavesDone?.Invoke();
        }
    }

    private void OnPlayerBurial()
    {
        foreach (var op in m_currentOpponentsBrain)
        {
            op.DisableAI();
            var healthBehaviour = op.GetComponent<HealthBehaviour>();
            if (healthBehaviour != null)
            {
                healthBehaviour.OnBurial.RemoveListener(OnBurial);
            }
        }
        m_playerHealth.OnBurial.RemoveListener(OnPlayerBurial);

        m_isInitialized = false;
        m_stateMachineReference.SetState(m_gameoverState);
    }

    private void OnBurial()
    {
        if (--m_remainingOpponent == 0)
        {
            Debug.Log($"{this.name}: 0 remaining opponent, BattleWave done");
            BattleWaveStop();
        }
        else
        {
            Debug.Log($"{this.name}: {m_remainingOpponent} remaining opponent(s)");
        }
    }

    private IEnumerator BattleStartDelayed_Coroutine(float time)
    {
        Debug.Log($"{this.name}: BattleStartDelayed_Coroutine timer start");
        yield return new WaitForSeconds(time);
        Debug.Log($"{this.name}: BattleStartDelayed_Coroutine timer end");
        BattleWaveInit(++m_currentBattleIndex);
        BattleWaveStart();
    }


    [System.Serializable]
    private class BattleWave
    {
        public GameObject WaveParent;
        public bool StartNextWaveAutomatically = true;
        public float DelayBeforeNextWave = 1f;
        public bool SetStateAfterBattleWave = false;
        public GameModeStateMachine StateMachineReference;
        public GameModeStateDefinition StateAfterBattleWaveMode;

        public void SetGameModeState()
        {
            if (!SetStateAfterBattleWave)
            {
                return;
            }

            Debug.Assert(StateMachineReference);
            Debug.Assert(StateAfterBattleWaveMode);

            StateMachineReference.SetState(StateAfterBattleWaveMode);
        }
    }
}
