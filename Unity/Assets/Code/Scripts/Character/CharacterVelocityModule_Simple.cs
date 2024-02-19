using NaughtyAttributes;
using NobunAtelier;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static NobunAtelier.Character2DMovementVelocity;

[AddComponentMenu("NobunAtelier/Character/Velocity/VelocityModule: 2D SIMULACRA")]
public class CharacterVelocityModule_Simple : CharacterVelocityModuleBase
{
    public enum MovementAxes
    {
        XZ,
        XY,
        YZ,
        Custom
    }

    public enum VelocityProcessing
    {
        // No acceleration, velocity calculated from raw inputDirection
        FromRawInput,

        // Prioritize acceleration control over velocity
        FromAcceleration,

        // Use the acceleration but prioritize velocity control
        DesiredVelocityFromAcceleration,
    }

    [SerializeField]
    private MovementAxes m_movementAxes = MovementAxes.XZ;
    [SerializeField]
    private VelocityProcessing m_accelerationApplication = VelocityProcessing.FromRawInput;

    private Vector3 m_movementVector;

    [ShowIf("DisplayCustomMovementAxisFields")]
    public Vector3 CustomForwardAxis = Vector3.forward;

    [ShowIf("DisplayCustomMovementAxisFields")]
    public Vector3 CustomRightAxis = Vector3.right;

    [SerializeField, Range(0f, 100f)]
    private float m_accelerationSpeed = 1.0f;
    [SerializeField, Range(0f, 100f)]
    private float m_maxAcceleration = 50.0f;

    [SerializeField, ReadOnly]
    private Vector3 m_velocity;

#if UNITY_EDITOR
    private bool DisplayCustomMovementAxisFields()
    {
        return m_movementAxes == MovementAxes.Custom;
    }
#endif

    public override void MoveInput(Vector3 direction)
    {
        switch (m_movementAxes)
        {
            case MovementAxes.XZ:
                m_movementVector = direction;
                m_movementVector.y = 0;
                break;

            case MovementAxes.XY:
                m_movementVector = new Vector3(direction.x, direction.z, 0);
                break;

            case MovementAxes.YZ:
                m_movementVector = new Vector3(0, direction.z, direction.x);
                break;

            case MovementAxes.Custom:
                m_movementVector = CustomRightAxis * direction.x + CustomForwardAxis * direction.z;
                break;
        }

        m_movementVector.Normalize();
    }
    float m_acceleration = 0;
    public float m_speed = 1;
    public float m_decelerationTime = 1;
    private float m_lastMoveAccel = 0;
    private float m_noMoveTime = 0;

    public override Vector3 VelocityUpdate(Vector3 currentVel, float deltaTime)
    {
        var vel = Vector3.zero;

        switch (m_accelerationApplication)
        {
            case VelocityProcessing.FromRawInput:
                m_velocity = m_movementVector * m_speed;
                m_movementVector = Vector3.zero;
                return m_velocity + currentVel;
            case VelocityProcessing.FromAcceleration:
                return vel;
            case VelocityProcessing.DesiredVelocityFromAcceleration:
                return ComputeDesiredVelocityFromAccel(currentVel, deltaTime);
        }

        return currentVel;
    }

    private Vector3 ComputeDesiredVelocityFromAccel(Vector3 currentVel, float deltaTime)
    {
        Vector3 desiredVelocity = m_movementVector * m_speed;
        // float maxSpeedChange = deltaTime * m_maxAcceleration;
        if (m_movementVector != Vector3.zero)
        {
            m_acceleration = Mathf.Min(m_accelerationSpeed + m_acceleration, m_maxAcceleration);
            m_lastMoveAccel = m_acceleration;
            m_noMoveTime = 0;
            m_velocity = Vector3.MoveTowards(m_velocity, desiredVelocity, m_acceleration * deltaTime);
        }
        else if (m_noMoveTime < 1)
        {
            m_noMoveTime += deltaTime / m_decelerationTime;
            m_acceleration = Mathf.Lerp(m_lastMoveAccel, 0, m_noMoveTime);
            m_velocity = Vector3.Slerp(m_velocity, Vector3.zero, m_noMoveTime);
        }
        // Vector3 desiredVelocity = m_movementVector * m_maxSpeed;
        // float speed = deltaTime * m_desiredVelocityMaxAcceleration;
        // m_velocity = m_movementVector * m_speed * m_acceleration * deltaTime;
        m_movementVector = Vector3.zero;
        return currentVel + m_velocity;
    }
}
