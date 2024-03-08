using UnityEngine;
using NobunAtelier;
using NaughtyAttributes;
using UnityEngine.Splines;
using System.Collections.Generic;

public class BattleWaveDefinition : DataDefinition
{
    [SerializeField] private NewSequence[] m_newSequences;
    [SerializeField, HideInInspector] private Sequence[] m_sequences;
    // [SerializeField] private PoolObjectDefinition m_entityToSpawn;

    public IReadOnlyList<Sequence> WavesSequence => m_sequences;

    [System.Serializable]
    public class Sequence
    {
        // This is going to be transform in world perspective space.
        // We really care about value between -1 and 1
        [SerializeField, Tooltip("Delay before this sequence start")] private float m_delay = 0;
        [SerializeField] private bool m_useSplinePath = false;
        [SerializeField, HideIf("ShowSpline")] private TweenSettings m_tween;
        [SerializeField, ShowIf("ShowSpline")] private SplinePathSettings m_splinePath;
        [SerializeField] private float m_movementDuration;
        [SerializeField] private bool m_despawnWhenReachingDestinaton = false;

        [SerializeField] private int m_entityToSpawnCount = 1;
        [SerializeField] private float m_delayInSecondBetweenSpawn = 1f;

        [SerializeField] private GameObject m_PROTO_prefabToSpawn;

        public bool UseSpline => m_useSplinePath;
        public float Delay => m_delay;
        public TweenSettings Tween => m_tween;
        public SplinePathSettings Spline => m_splinePath;
        public float TweenDuration => m_movementDuration;
        public int EntityToSpawnCount => m_entityToSpawnCount;
        public float SpawnDelay => m_delayInSecondBetweenSpawn;
        public bool DespawnWhenReachingDestinaton => m_despawnWhenReachingDestinaton;

        public GameObject PROTO_PrefabToSpawn => m_PROTO_prefabToSpawn;

        private bool ShowSpline => m_useSplinePath;
        private bool ShowVectors => !m_useSplinePath;

        [System.Serializable]
        public class TweenSettings
        {
            [SerializeField] private Vector3 m_origin;
            [SerializeField] private Vector3 m_destination;
            [SerializeField] private Vector3 m_escapeDestination;
            [SerializeField] private AnimationCurve m_tweenCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            public Vector3 Origin => m_origin;
            public Vector3 Destination => m_destination;
            public Vector3 EscapeDestination => m_escapeDestination;
            public AnimationCurve Curve => m_tweenCurve;
        }

        [System.Serializable]
        public class SplinePathSettings
        {
            [SerializeField] private Spline m_path;
            [SerializeField] private SplineAnimate.EasingMode m_easingMode;
            [SerializeField] private SplineAnimate.LoopMode m_loopMode;

            public Spline Path => m_path;
            public SplineAnimate.EasingMode EasingMode => m_easingMode;
            public SplineAnimate.LoopMode LoopMode => m_loopMode;
        }
    }

    [System.Serializable]
    public class NewSequence
    {
        [Tooltip("Delay before the sequence start. In seconds.")]
        [SerializeField] private float m_delay = 0;
        [SerializeField] private BattleMotionDefinition m_motion;
        [SerializeField] private PoolableBattler m_battler;
        [SerializeField, Min(1)] private int m_spawnCount = 1;
        [Tooltip("In seconds.")]
        [SerializeField] private int m_spawnOffset = 1;
    }
}