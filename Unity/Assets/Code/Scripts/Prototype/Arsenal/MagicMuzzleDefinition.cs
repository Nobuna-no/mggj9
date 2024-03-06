using UnityEngine;
using NobunAtelier;
using NaughtyAttributes;

public class MagicMuzzleDefinition : DataDefinition
{
    // Not sure if the projectile velocity should be here...
    // [Tooltip("Time between shots / smaller = higher rate of fire")]
    // [SerializeField] private float m_cooldownWindow = 0.1f;
    // [SerializeField] private float m_spreadAngle = 5f;

    [SerializeField] private VirtualMuzzle[] m_virtualMuzzles; // "I will have 5 muzzle in my canon, and each will shoot 5 bullets per fire with a medium spread, and also with different speed and spawn time!"
    [SerializeField] private float m_fireRatePerSecond = 1f; // "5 per second please!"
    [SerializeField, MinMaxSlider(0, 5)] private Vector2 m_openFireDurationRange; // "Open fire for [1-2]s"
    [SerializeField, MinMaxSlider(0, 5)] private Vector2 m_delayRangeBetweenOpenFire; // "Wait [1-2]s between each fire"

    [System.Serializable]
    // Each muzzle can spawn several bullets
    public class VirtualMuzzle
    {
        [SerializeField] private Vector3 m_localOffset = Vector2.zero; // Local offset to the true muzzle
        [SerializeField, MinMaxSlider(1, 10)] private Vector2 m_bulletCountRange = Vector2.one; // "I want [2-5] bullets!"
        [SerializeField, MinMaxSlider(0, 5)] private Vector2 m_spreadAngleRange = Vector2.zero; // "I want a shotgun!"
        [SerializeField, MinMaxSlider(0, 5)] private Vector2 m_bulletSpawnDelay = Vector2.zero; // "Not all bullet are going to go at the same time"
        [SerializeField, MinMaxSlider(0, 5)] private Vector2 m_velocityMultiplierRange = Vector2.one; // "Small multiplier to add speed variation to the bullet"
    }
}