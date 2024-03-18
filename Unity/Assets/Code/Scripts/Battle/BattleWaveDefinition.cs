using NaughtyAttributes;
using NobunAtelier;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

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


#if UNITY_EDITOR
    [ContextMenu("CopyAllMotionToNewMotion")]
    public void CopyAllMotionToNewMotion()
    {
        foreach (var sequence in m_newSequences)
        {
            sequence.CopyMotionData();
        }
    }
#endif

    [System.Serializable]
    public class Sequence
    {
        [Tooltip("Delay before the sequence start. In seconds.")]
        [SerializeField] private float m_delay = 0;

        // [SerializeField] private float m_delayBeforeBattlerAttack = 0;
        [SerializeField] private bool m_useMotionDefinition = false;
        [FormerlySerializedAs("m_newMotion")]
        [SerializeField, AllowNesting, HideIf("m_useMotionDefinition")] private BattleMotion m_motion;
        [FormerlySerializedAs("m_motion")]
        [SerializeField, AllowNesting, ShowIf("m_useMotionDefinition")] private BattleMotionDefinition m_motionDefinition;
        [SerializeField] private BattlerDefinition m_battler;
        [SerializeField] private BattlerSettings m_battlerSettings;
        [SerializeField, Min(1)] private int m_spawnCount = 1;

        [Tooltip("In seconds.")]
        [SerializeField, Min(0)] private float m_spawnOffset = 1;

        public float Delay => m_delay;
        public BattlerSettings BattlerSettings => m_battlerSettings;

        // public float DelayBeforeBattlerAttack => m_delayBeforeBattlerAttack;
        public BattleMotion Motion => m_motion;

        public BattlerDefinition BattlerDefintion => m_battler;
        public int SpawnCount => m_spawnCount;
        public float SpawnOffset => m_spawnOffset;

        public void CopyMotionData()
        {
            m_motion.CopyMotionData(m_motionDefinition);
        }

        [System.Serializable]
        public class BattleMotion
        {
            [SerializeField, TextArea] private string m_description;
            [SerializeField] private MotionSpace m_origin;
            [SerializeField] private MotionSpace m_destination;
            [SerializeField] private AnimationCurve m_motionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
            [SerializeField] private float m_motionDuration = 1f;

            public void CopyMotionData(BattleMotionDefinition data)
            {
                m_motionDuration = data.Duration;
                m_origin = new MotionSpace(data.Origin);
                m_destination = new MotionSpace(data.Destination);
                m_motionCurve = data.Curve;
                m_description = data.Description;
            }

            public string Description => m_description;
            public MotionSpace Origin => m_origin;
            public MotionSpace Destination => m_destination;
            public AnimationCurve Curve => m_motionCurve;
            public float Duration => m_motionDuration;


        }

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
        private bool m_overrideSpawnIFrame = false;
        [SerializeField, AllowNesting, ShowIf("m_overrideSpawnIFrame")]
        private float m_invulnerabilityAtSpawnDuration = 0.5f;

        public bool CanShoot => m_canShoot;
        public float DelayBeforeStartShooting => m_delayBeforeStartShooting;
        public float IntermittenShootOffset => m_intermittentShootOffset;
        public bool OverrideSpawnIFrame => m_overrideSpawnIFrame;
        public float InvulnerabilityAtSpawnDuration => m_invulnerabilityAtSpawnDuration;
    }
}
