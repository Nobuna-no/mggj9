using NaughtyAttributes;
using NobunAtelier;
using UnityEngine;

public class BattleMotionDefinition : DataDefinition
{
    [SerializeField] private MotionSpace m_origin;
    [SerializeField] private MotionSpace m_destination;
    [SerializeField] private AnimationCurve m_motionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float m_motionDuration = 1f;

    public MotionSpace Origin => m_origin;
    public MotionSpace Destination => m_destination;
    public AnimationCurve Curve => m_motionCurve;
    public float Duration => m_motionDuration;

    [System.Serializable]
    public class MotionSpace
    {
        public enum SpaceType
        {
            Point,
            Spline
        }

        [SerializeField] private SpaceType m_mode = SpaceType.Point;
        [SerializeField, AllowNesting, ShowIf("PointMode")] private Vector3 m_point;
        [SerializeField, AllowNesting, HideIf("PointMode")] private SplineDefinition m_spline;
        [SerializeField, AllowNesting, HideIf("PointMode")] private Vector3 m_splineOrigin;
        [SerializeField, AllowNesting, HideIf("PointMode")] private float m_splineScale = 1f;

#if UNITY_EDITOR
        private bool PointMode => m_mode == SpaceType.Point;
#endif
        public SpaceType Type => m_mode;
        public Vector3 Point => m_point;
        public SplineDefinition SplineDefinition => m_spline;
        public Vector3 SplineOrigin => m_splineOrigin;
        public float SplineScale => m_splineScale;
    }

    // This is going to be transform in world perspective space.
    // We really care about value between -1 and 1
    // [SerializeField, Tooltip("Delay before this sequence start")] private float m_delay = 0;
    // [SerializeField] private bool m_useSplinePath = false;
    // [SerializeField, HideIf("ShowSpline")] private PointDefinition m_tween;
    // [SerializeField, ShowIf("ShowSpline")] private SplinePathSettings m_splinePath;
    // [SerializeField] private bool m_despawnWhenReachingDestinaton = false;

    // [SerializeField] private int m_entityToSpawnCount = 1;
    // [SerializeField] private float m_delayInSecondBetweenSpawn = 1f;
    //
    // [SerializeField] private GameObject m_PROTO_prefabToSpawn;

    //  public bool UseSpline => m_useSplinePath;

    // public float Delay => m_delay;
    // public PointDefinition Tween => m_tween;
    // public SplinePathSettings Spline => m_splinePath;
    // public float TweenDuration => m_movementDuration;
    // public int EntityToSpawnCount => m_entityToSpawnCount;
    // public float SpawnDelay => m_delayInSecondBetweenSpawn;
    // public bool DespawnWhenReachingDestinaton => m_despawnWhenReachingDestinaton;
    //
    // public GameObject PROTO_PrefabToSpawn => m_PROTO_prefabToSpawn;
    //
    // private bool ShowSpline => m_useSplinePath;
    // private bool ShowVectors => !m_useSplinePath;

    //[System.Serializable]
    //public class PointDefinition
    //{
    //    [SerializeField] private Vector3 m_origin;
    //    [SerializeField] private Vector3 m_destination;
    //    [SerializeField] private Vector3 m_escapeDestination;

    //    public Vector3 Origin => m_origin;
    //    public Vector3 Destination => m_destination;
    //    public Vector3 EscapeDestination => m_escapeDestination;
    //}

    //[System.Serializable]
    //public class SplinePathSettings
    //{
    //    [SerializeField] private Spline m_path;
    //    [SerializeField] private SplineAnimate.EasingMode m_easingMode;
    //    [SerializeField] private SplineAnimate.LoopMode m_loopMode;

    //    public Spline Path => m_path;
    //    public SplineAnimate.EasingMode EasingMode => m_easingMode;
    //    public SplineAnimate.LoopMode LoopMode => m_loopMode;
    //}
}
