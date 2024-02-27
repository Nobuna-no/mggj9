using UnityEngine;
using NobunAtelier;

public class MagicMuzzleDefinition : DataDefinition
{
    // Not sure if the projectile velocity should be here...
    [Tooltip("Time between shots / smaller = higher rate of fire")]
    [SerializeField] private float m_cooldownWindow = 0.1f;
    [SerializeField] private float m_spreadAngle = 5f;
}