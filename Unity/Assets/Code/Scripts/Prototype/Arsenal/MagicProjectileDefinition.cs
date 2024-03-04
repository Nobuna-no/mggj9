using UnityEngine;
using NobunAtelier;

public class MagicProjectileDefinition : DataDefinition
{
    [Tooltip("Projectile force")]
    [SerializeField] private float m_muzzleVelocity = 700f;
    [SerializeField] private float m_timeoutDelay = 3f;
}