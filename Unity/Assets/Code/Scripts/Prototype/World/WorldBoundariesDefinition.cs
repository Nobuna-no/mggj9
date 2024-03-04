using UnityEngine;
using NobunAtelier;
using NaughtyAttributes;

public class WorldBoundariesDefinition : DataDefinition
{
    [System.Flags]
    public enum ConstrainedAxis
    {
        X = 1,
        Y = 2,
        Z = 4
    }

    [SerializeField] private ConstrainedAxis m_constrainedAxes = ConstrainedAxis.X | ConstrainedAxis.Y | ConstrainedAxis.Z;
    [SerializeField, ShowIf("IsLockingX")] private Vector2 m_axisRangeX = new Vector2(-1f, 1f);
    [SerializeField, ShowIf("IsLockingY")] private Vector2 m_axisRangeY = new Vector2(-1f, 1f);
    [SerializeField, ShowIf("IsLockingZ")] private Vector2 m_axisRangeZ = new Vector2(-1f, 1f);

    public ConstrainedAxis ConstrainedAxesFlags => m_constrainedAxes;
    public Vector2 AxisRangeX => m_axisRangeX;
    public Vector2 AxisRangeY => m_axisRangeY;
    public Vector2 AxisRangeZ => m_axisRangeZ;

#if UNITY_EDITOR
    private bool IsLockingX => (m_constrainedAxes & ConstrainedAxis.X) != 0;
    private bool IsLockingY => (m_constrainedAxes & ConstrainedAxis.Y) != 0;
    private bool IsLockingZ => (m_constrainedAxes & ConstrainedAxis.Z) != 0;
#endif
}