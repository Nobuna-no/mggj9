using NobunAtelier;
using System.Collections.Generic;
using UnityEngine;

public class BattleWaveDefinition : DataDefinition
{
    [SerializeField] private Sequence[] m_newSequences;
    public IReadOnlyList<Sequence> WavesSequence => m_newSequences;

    [System.Serializable]
    public class Sequence
    {
        [Tooltip("Delay before the sequence start. In seconds.")]
        [SerializeField] private float m_delay = 0;

        [SerializeField] private BattleMotionDefinition m_motion;
        [SerializeField] private BattlerDefinition m_battler;
        [SerializeField, Min(1)] private int m_spawnCount = 1;

        [Tooltip("In seconds.")]
        [SerializeField, Min(0)] private float m_spawnOffset = 1;

        public float Delay => m_delay;
        public BattleMotionDefinition Motion => m_motion;
        public BattlerDefinition BattlerDefintion => m_battler;
        public int SpawnCount => m_spawnCount;
        public float SpawnOffset => m_spawnOffset;
    }
}
