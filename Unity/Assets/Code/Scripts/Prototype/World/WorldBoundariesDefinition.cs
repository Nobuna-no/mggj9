using NaughtyAttributes;
using NobunAtelier;
using UnityEngine;

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

    [Tooltip("Unclamped remapped position being greater than 1 will be add this offset")]
    [SerializeField] private float m_unclampedPadding = 0.25f;

    public ConstrainedAxis ConstrainedAxesFlags => m_constrainedAxes;
    public Vector2 AxisRangeX => m_axisRangeX;
    public Vector2 AxisRangeY => m_axisRangeY;
    public Vector2 AxisRangeZ => m_axisRangeZ;

#if UNITY_EDITOR
    private bool IsLockingX => (m_constrainedAxes & ConstrainedAxis.X) != 0;
    private bool IsLockingY => (m_constrainedAxes & ConstrainedAxis.Y) != 0;
    private bool IsLockingZ => (m_constrainedAxes & ConstrainedAxis.Z) != 0;
#endif

    public Vector3 RemapPositionToBoundaries(Vector3 position)
    {
        // Remap the position to the un-clamped boundaries.
        return new Vector3(
            Mathf.LerpUnclamped(AxisRangeX.x, AxisRangeX.y, Mathf.InverseLerp(-1f, 1f, position.x)),
            Mathf.LerpUnclamped(AxisRangeY.x, AxisRangeY.y, Mathf.InverseLerp(-1f, 1f, position.y)),
            Mathf.LerpUnclamped(AxisRangeZ.x, AxisRangeZ.y, Mathf.InverseLerp(-1f, 1f, position.z))
            );
    }

    public Vector3 ClampPositionToBoundaries(Vector3 position)
    {
        // Remap the position to the boundaries.
        return new Vector3(
            Mathf.Clamp(position.x, AxisRangeX.x, AxisRangeX.y),
            Mathf.Clamp(position.y, AxisRangeY.x, AxisRangeY.y),
            Mathf.Clamp(position.z, AxisRangeZ.x, AxisRangeZ.y)
            );
    }

    public Vector3 RemapPositionToBoundariesUnclamped(Vector3 position)
    {
        // Remap the position to the un-clamped boundaries.
        return new Vector3(
            Mathf.LerpUnclamped(AxisRangeX.x, AxisRangeX.y,
                Mathf.InverseLerp(-1f, 1f, position.x) + Mathf.Sign(position.x) * (Mathf.Abs(position.x) > 1 ? m_unclampedPadding : 0)),
            Mathf.LerpUnclamped(AxisRangeY.x, AxisRangeY.y,
                Mathf.InverseLerp(-1f, 1f, position.y) + Mathf.Sign(position.y) * (Mathf.Abs(position.y) > 1 ? m_unclampedPadding : 0)),
            Mathf.LerpUnclamped(AxisRangeZ.x, AxisRangeZ.y,
                Mathf.InverseLerp(-1f, 1f, position.z) + Mathf.Sign(position.z) * (Mathf.Abs(position.z) > 1 ? m_unclampedPadding : 0))
            );
    }

    public bool IsPositionInBoundary(Vector3 position)
    {
        return IsInRange(position.x, AxisRangeX) &&
               IsInRange(position.y, AxisRangeY) &&
               IsInRange(position.z, AxisRangeZ);
    }

    private bool IsInRange(float value, Vector2 range)
    {
        return value >= range.x && value <= range.y;
    }
}
