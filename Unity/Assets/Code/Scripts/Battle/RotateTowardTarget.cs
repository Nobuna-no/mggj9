using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardTarget : MonoBehaviour
{
    [SerializeField] private Rigidbody m_targetRigidbody;
    public Transform m_targetToFollow;

    private void OnEnable()
    {
        m_targetToFollow = GameBlackboard.PlayerTransform;
    }

    private void OnDisable()
    {
    }

    private void Update()
    {
        if (m_targetRigidbody == null || m_targetToFollow == null)
        {
            Debug.LogError("Target Rigidbody or Target Transform not assigned.");
            return;
        }

        // Calculate direction to target
        Vector3 direction = m_targetToFollow.position - m_targetRigidbody.position;
        direction.y = 0f; // Make sure the rotation only affects the yaw

        if (direction != Vector3.zero)
        {
            // Calculate rotation to look at the target
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Apply rotation only affecting the yaw
            m_targetRigidbody.MoveRotation(Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f));
        }
    }
}
