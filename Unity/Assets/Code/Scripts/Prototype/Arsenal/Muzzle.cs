using UnityEngine;
using NobunAtelier;
using NaughtyAttributes;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections.Generic;

public class Muzzle : UnityPoolBehaviour<Bullet>
{
    [Header("Magic Muzzle")]
    [Tooltip("Projectile force")]
    [SerializeField] private float m_muzzleVelocity = 700f;
    [Tooltip("End point of gun where shots appear")]
    [SerializeField] private Transform[] m_muzzlesPosition;
    [Tooltip("Time between shots / smaller = higher rate of fire")]
    [SerializeField, MinMaxSlider(0.1f, 5f)] private Vector2 m_cooldownWindow = Vector2.one;
    [SerializeField] float m_spreadAngle = 5f; // Adjust this value to control the spread
    [SerializeField] private bool m_shootingEnable = false;
    [SerializeField] private bool m_isAffectedByAugment = false;
    [SerializeField, ShowIf("m_isAffectedByAugment")] private AugmentDefinition m_damageAugmentDefinition;
    [SerializeField, ShowIf("m_isAffectedByAugment")] private TierPrefab[] m_bulletPerAugmentTier;

    private WorldBoundariesDefinition m_worldBoundaries;
    private float m_nextTimeToShoot;
    private float m_fireRateMultiplier = 1;

    private Dictionary<AugmentTierDefinition, TierPrefab> m_bulletPerAugmentTierMap;
    private AugmentController.Augment m_damageAugment;
    private Bullet m_defaultBullet;

    [System.Serializable]
    private class TierPrefab
    {
        [SerializeField] private AugmentTierDefinition m_augmentTier;
        [SerializeField] private Bullet m_bullet;

        public AugmentTierDefinition AugmentTier => m_augmentTier;
        public Bullet BulletPrefab => m_bullet;
    }

    private void Start()
    {
        WorldPerspectiveManager.Instance.OnWorldPerspectiveChanged += OnWorldPerspectiveChanged;
        m_worldBoundaries = WorldPerspectiveManager.Instance.ActiveBoundaries;

        if (m_isAffectedByAugment && GameBlackboard.IsSingletonValid && AugmentController.IsSingletonValid)
        {
            // Listen to BB fire rate
            GameBlackboard.FireRateMultiplier.OnValueChanged += FireRateMultiplier_OnValueChanged;
            m_fireRateMultiplier = GameBlackboard.FireRateMultiplier.Value;

            // Retrieve damage augment and subscribe to event
            m_defaultBullet = m_objectPrefab;
            if (!AugmentController.Instance.TryGetAugment(m_damageAugmentDefinition, out m_damageAugment))
            {
                Debug.LogWarning($"Failed to retrieved '{m_damageAugmentDefinition.name}' from AugmentController", this);
            }
            m_damageAugment.OnAugmentTierChanged += M_damageAugment_OnAugmentTierChanged;
            m_damageAugment.OnAugmentDeactivated += M_damageAugment_OnAugmentDeactivated;

            m_bulletPerAugmentTierMap = new Dictionary<AugmentTierDefinition, TierPrefab>(m_bulletPerAugmentTier.Length);
            foreach (var bulletTier in m_bulletPerAugmentTier)
            {
                m_bulletPerAugmentTierMap.Add(bulletTier.AugmentTier, bulletTier);
            }
        }
    }

    private void OnDestroy()
    {
        m_damageAugment.OnAugmentTierChanged -= M_damageAugment_OnAugmentTierChanged;
        m_damageAugment.OnAugmentDeactivated -= M_damageAugment_OnAugmentDeactivated;
    }

    private void M_damageAugment_OnAugmentTierChanged(AugmentTierDefinition tier)
    {
        if (!m_bulletPerAugmentTierMap.TryGetValue(tier, out var bullet))
        {
            Debug.LogWarning($"Failed to retrieved '{tier.name}' from m_bulletPerAugmentTierMap", this);
        }

        m_objectPrefab = bullet.BulletPrefab;
    }

    private void M_damageAugment_OnAugmentDeactivated()
    {
        m_objectPrefab = m_defaultBullet;
    }

    private void FireRateMultiplier_OnValueChanged(float value)
    {
        m_fireRateMultiplier = value;
    }

    private void OnWorldPerspectiveChanged(WorldBoundariesDefinition newBoundaries)
    {
        m_worldBoundaries = newBoundaries;
    }

    public void EnableShooting()
    {
        m_shootingEnable = true;
    }

    public void DisableShooting()
    {
        m_shootingEnable = false;
    }

    private void FixedUpdate()
    {
        if (!m_shootingEnable)
        {
            return;
        }

        // shoot if we have exceeded delay
        if (Time.time > m_nextTimeToShoot && Pool != null)
        {
            foreach (var muzzlePosition in m_muzzlesPosition)
            {
                if (!muzzlePosition.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Bullet bulletObject = Pool.Get();
                if (bulletObject == null)
                    continue;

                // align to gun barrel/muzzle position
                bulletObject.transform.SetPositionAndRotation(muzzlePosition.position, muzzlePosition.rotation);

                // Generate a random rotation within the spread angle
                // need to constrain the y axis for top down and x for sidescroll
                bool constrainedXSpread = m_worldBoundaries.AxisRangeX.y - m_worldBoundaries.AxisRangeX.x == 0;
                bool constrainedYSpread = m_worldBoundaries.AxisRangeY.y - m_worldBoundaries.AxisRangeY.x == 0;
                Quaternion spreadRotation = Quaternion.Euler(0,
                    constrainedXSpread ? 0 : Random.Range(-m_spreadAngle, m_spreadAngle),
                    constrainedYSpread ? 0 : Random.Range(-m_spreadAngle, m_spreadAngle));
                Vector3 spreadDirection = spreadRotation * bulletObject.transform.forward;
                // spreadDirection = m_worldBoundaries.RemapPositionToBoundaries(spreadDirection);
                bulletObject.transform.forward = spreadDirection;

                bulletObject.TargetRigidbody.AddForce(spreadDirection * m_muzzleVelocity, ForceMode.Acceleration);
                // bulletObject.GetComponent<Rigidbody>().AddForce(bulletObject.transform.forward * muzzleVelocity, ForceMode.Acceleration);

                // turn off after a few seconds
                bulletObject.Spawn();
            }

            // set cooldown delay
            m_nextTimeToShoot = Time.time + Random.Range(m_cooldownWindow.x, m_cooldownWindow.y) / m_fireRateMultiplier;
        }
    }
}
