using NaughtyAttributes;
using NobunAtelier;
using UnityEngine;

public class BattleMotionDefinition : DataDefinition
{
    [SerializeField, TextArea] private string m_description;
    [SerializeField] private MotionSpace m_origin;
    [SerializeField] private MotionSpace m_destination;
    [SerializeField] private AnimationCurve m_motionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float m_motionDuration = 1f;
    public string Description => m_description;
    public MotionSpace Origin => m_origin;
    public MotionSpace Destination => m_destination;
    public AnimationCurve Curve => m_motionCurve;
    public float Duration => m_motionDuration;

}

[System.Serializable]
public class MotionSpace
{
    public enum SpaceType
    {
        Point,
        Spline
    }

    [SerializeField] private SpaceType m_mode = SpaceType.Point;
    [SerializeField, AllowNesting] private Vector3 m_point;
    [SerializeField, AllowNesting, HideIf("PointMode")] private SplineDefinition m_spline;
    [SerializeField, AllowNesting, HideIf("PointMode")] private float m_splineScale = 1f;

#if UNITY_EDITOR
    private bool PointMode => m_mode == SpaceType.Point;
#endif
    public SpaceType Type => m_mode;
    public Vector3 Point => m_point;
    public SplineDefinition SplineDefinition => m_spline;
    public Vector3 SplineOrigin => m_point;
    public float SplineScale => m_splineScale;

    public MotionSpace(MotionSpace copy)
    {
        m_mode = copy.m_mode;
        m_point = copy.m_point;
        m_spline = copy.m_spline;
        m_splineScale = copy.m_splineScale;
    }
}

