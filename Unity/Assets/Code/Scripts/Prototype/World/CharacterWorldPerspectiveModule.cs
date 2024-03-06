using NaughtyAttributes;
using System;
using UnityEngine;
using NobunAtelier;

[AddComponentMenu("SIMULACRA/Character/Velocity/VelocityModule: World Perspective")]
public class CharacterWorldPerspectiveModule : CharacterVelocityModuleBase
{
    [SerializeField] private WorldBoundariesDefinition m_perspectiveDefinition;
    [SerializeField, Range(0, 100)] private float m_lerpSpeed = 1f;
    [SerializeField] private AnimationCurve m_slowdownCurve;
    [SerializeField] private float m_distanceToBorderTrehsold = 1f;

    private Vector3 m_internalVelocity = Vector3.zero;

    private float ComputeAxisVelocity(float currentVel, Vector2 axisRange, ref float internalVel, float location, float deltaTime)
    {
        if (location > axisRange.y)
        {
            float distanceRatio = location / axisRange.y;
            internalVel += -distanceRatio * m_lerpSpeed * deltaTime;
            currentVel = m_lerpSpeed == 0 ? (axisRange.y - location) / deltaTime : currentVel + internalVel;
        }
        else if (location < axisRange.x)
        {
            float distanceRatio = location / axisRange.x;
            internalVel += distanceRatio * m_lerpSpeed * deltaTime;
            currentVel = m_lerpSpeed == 0 ? (axisRange.x - location) / deltaTime : currentVel + internalVel;
        }
        else
        {
            internalVel = 0;// Mathf.Lerp(m_internalVelocity.x, 0, deltaTime * m_decelerationSpeed);
        }

        return currentVel;
    }


    // Attemps to make a slowdown on border approach...
    private float ComputeAxisVelocityV2(float currentVel, Vector2 axisRange, ref float internalVel, float location, float deltaTime)
    {
        if (location > axisRange.y)
        {
            float distanceRatio = location / axisRange.y;
            internalVel += -distanceRatio * m_lerpSpeed * deltaTime;
            currentVel = m_lerpSpeed == 0 ? (axisRange.y - location) / deltaTime : currentVel + internalVel;
        }
        else if (Mathf.Abs(axisRange.y - location) < m_distanceToBorderTrehsold && (Mathf.Sign(currentVel) == 1))
        {
            float distanceRatio = Mathf.Abs(axisRange.y - location) / m_distanceToBorderTrehsold;
            currentVel = Mathf.Lerp(currentVel, 0, m_slowdownCurve.Evaluate(1 - distanceRatio));
        }
        else if (location < axisRange.x)
        {
            float distanceRatio = location / axisRange.x;
            internalVel += distanceRatio * m_lerpSpeed * deltaTime;
            currentVel = m_lerpSpeed == 0 ? (axisRange.x - location) / deltaTime : currentVel + internalVel;
        }
        else if (Mathf.Abs(axisRange.x - location) < m_distanceToBorderTrehsold && (Mathf.Sign(currentVel) == -1))
        {
            float distanceRatio = Mathf.Abs(axisRange.x - location) / m_distanceToBorderTrehsold;
            currentVel = Mathf.Lerp(currentVel, 0, m_slowdownCurve.Evaluate(1 - distanceRatio));
        }
        else
        {
            internalVel = 0;
        }

        return currentVel;
    }

    public override Vector3 VelocityUpdate(Vector3 currentVel, float deltaTime)
    {
        Vector3 location = ModuleOwner.Position;

        if ((m_perspectiveDefinition.ConstrainedAxesFlags & WorldBoundariesDefinition.ConstrainedAxis.X) != 0)
        {
            currentVel.x = ComputeAxisVelocityV2(currentVel.x, m_perspectiveDefinition.AxisRangeX, ref m_internalVelocity.x, location.x, deltaTime);
        }
        if ((m_perspectiveDefinition.ConstrainedAxesFlags & WorldBoundariesDefinition.ConstrainedAxis.Y) != 0)
        {
            currentVel.y = ComputeAxisVelocityV2(currentVel.y, m_perspectiveDefinition.AxisRangeY, ref m_internalVelocity.y, location.y, deltaTime);
        }
        if ((m_perspectiveDefinition.ConstrainedAxesFlags & WorldBoundariesDefinition.ConstrainedAxis.Z) != 0)
        {
            currentVel.z = ComputeAxisVelocityV2(currentVel.z, m_perspectiveDefinition.AxisRangeZ, ref m_internalVelocity.z, location.z, deltaTime);
        }

        return currentVel;
    }

    private void OnDrawGizmosSelected()
    {
        var xxx = new Vector3(m_perspectiveDefinition.AxisRangeX.x, m_perspectiveDefinition.AxisRangeY.x, m_perspectiveDefinition.AxisRangeZ.x); // left(x)-bottom(x)-back(x)
        var yxx = new Vector3(m_perspectiveDefinition.AxisRangeX.y, m_perspectiveDefinition.AxisRangeY.x, m_perspectiveDefinition.AxisRangeZ.x); // right(y)-bottom(x)-back(x)
        var xyx = new Vector3(m_perspectiveDefinition.AxisRangeX.x, m_perspectiveDefinition.AxisRangeY.y, m_perspectiveDefinition.AxisRangeZ.x); // left(x)-top(y)-back(x)
        var yyx = new Vector3(m_perspectiveDefinition.AxisRangeX.y, m_perspectiveDefinition.AxisRangeY.y, m_perspectiveDefinition.AxisRangeZ.x); // right(y)-top(y)-back(x)
        var xxy = new Vector3(m_perspectiveDefinition.AxisRangeX.x, m_perspectiveDefinition.AxisRangeY.x, m_perspectiveDefinition.AxisRangeZ.y);
        var yxy = new Vector3(m_perspectiveDefinition.AxisRangeX.y, m_perspectiveDefinition.AxisRangeY.x, m_perspectiveDefinition.AxisRangeZ.y);
        var xyy = new Vector3(m_perspectiveDefinition.AxisRangeX.x, m_perspectiveDefinition.AxisRangeY.y, m_perspectiveDefinition.AxisRangeZ.y);
        var yyy = new Vector3(m_perspectiveDefinition.AxisRangeX.y, m_perspectiveDefinition.AxisRangeY.y, m_perspectiveDefinition.AxisRangeZ.y);

        Gizmos.color = (Color.red + Color.yellow) / 2;
        Gizmos.DrawCube(xxx, Vector3.one * 0.3f);
        Gizmos.DrawCube(yxx, Vector3.one * 0.3f);
        Gizmos.DrawCube(xyx, Vector3.one * 0.3f);
        Gizmos.DrawCube(yyx, Vector3.one * 0.3f);
        Gizmos.DrawCube(xxy, Vector3.one * 0.3f);
        Gizmos.DrawCube(yxy, Vector3.one * 0.3f);
        Gizmos.DrawCube(xyy, Vector3.one * 0.3f);
        Gizmos.DrawCube(yyy, Vector3.one * 0.3f);

        Gizmos.DrawLineStrip(new Vector3[] { xxx, yxx, yyx, xyx }, true); // bottom square
        Gizmos.DrawLineStrip(new Vector3[] { xxy, yxy, yyy, xyy }, true); // top square
        Gizmos.DrawLineStrip(new Vector3[] { xxx, xxy, xyy, xyx }, false); // left square
        Gizmos.DrawLineStrip(new Vector3[] { yxx, yxy, yyy, yyx }, false); // right square
    }
}
