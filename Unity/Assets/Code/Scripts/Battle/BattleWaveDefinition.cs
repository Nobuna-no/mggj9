using NaughtyAttributes;
using NobunAtelier;
using System.Collections.Generic;
using UnityEngine;

public class BattleWaveDefinition : DataDefinition
{
    [SerializeField, TextArea] private string m_description;

    [Tooltip("Number of time to repeat the whole sequence. Minimum is 1.")]
    [SerializeField, Min(1)] private int m_repeatCycle = 1;
    [SerializeField, Min(1)] private float m_additionalDelayBetweenCycle = 1f;

    [SerializeField] private Sequence[] m_newSequences;
    public IReadOnlyList<Sequence> WavesSequence => m_newSequences;
    public int RepeatCount => m_repeatCycle;
    public float AdditionalDelayBetweenCycle => m_additionalDelayBetweenCycle;

    [System.Serializable]
    public class Sequence
    {
        [Tooltip("Delay before the sequence start. In seconds.")]
        [SerializeField] private float m_delay = 0;

        // [SerializeField] private float m_delayBeforeBattlerAttack = 0;

        [SerializeField] private BattleMotionDefinition m_motion;
        [SerializeField] private BattlerDefinition m_battler;
        [SerializeField] private BattlerSettings m_battlerSettings;
        [SerializeField, Min(1)] private int m_spawnCount = 1;

        [Tooltip("In seconds.")]
        [SerializeField, Min(0)] private float m_spawnOffset = 1;

        public float Delay => m_delay;
        public BattlerSettings BattlerSettings => m_battlerSettings;

        // public float DelayBeforeBattlerAttack => m_delayBeforeBattlerAttack;
        public BattleMotionDefinition Motion => m_motion;

        public BattlerDefinition BattlerDefintion => m_battler;
        public int SpawnCount => m_spawnCount;
        public float SpawnOffset => m_spawnOffset;
    }

    [System.Serializable]
    public class BattlerSettings
    {
        [SerializeField]
        private bool m_canShoot = true;

        [SerializeField, AllowNesting, ShowIf("m_canShoot"), Tooltip("In seconds.")]
        private float m_delayBeforeStartShooting = 1f;

        [SerializeField, AllowNesting, ShowIf("m_canShoot"), Tooltip("Delay between each shooting phase. Scale original value on Battler weapon.")]
        private float m_intermittentShootOffset = 1f;

        [SerializeField]
        private float m_invulnerabilityAtSpawnDuration = 0.3f;

        public bool CanShoot => m_canShoot;
        public float DelayBeforeStartShooting => m_delayBeforeStartShooting;
        public float IntermittenShootOffset => m_intermittentShootOffset;
        public float InvulnerabilityAtSpawnDuration => m_invulnerabilityAtSpawnDuration;
    }
}
