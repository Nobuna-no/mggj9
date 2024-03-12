using NaughtyAttributes;
using NobunAtelier;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Muzzle : UnityPoolBehaviour<Bullet>
{
    [Header("Muzzle")]
    [SerializeField] private bool m_shootingEnable = false;

    [Tooltip("Projectile force")]
    [SerializeField] private float m_muzzleVelocity = 700f;

    [Tooltip("End point of gun where shots appear")]
    [SerializeField] private Transform[] m_muzzlesPosition;

    [Tooltip("Time between shots / smaller = higher rate of fire")]
    [SerializeField] private float m_spreadAngle = 5f; // Adjust this value to control the spread

    [MinMaxSlider(0.1f, 5f)]
    [SerializeField] private Vector2 m_cooldownWindow = Vector2.one;

    [Tooltip("Should the shooting be disable every so often.")]
    [SerializeField] private float m_delayBeforeFirstShot = 0f;

    [SerializeField] private bool m_intermittentShooting = false;

    [Tooltip("How often should the shooting be disabled. In seconds.")]
    [SerializeField, ShowIf("m_intermittentShooting")] private float m_intermittenceOffset = 1f;

    [Tooltip("For how much time should the shooting be disabled. In seconds.")]
    [SerializeField, ShowIf("m_intermittentShooting")] private float m_intermittenceDuration = 1f;

    [Header("Augment")]
    [SerializeField] private bool m_isAffectedByAugment = false;

    [SerializeField, ShowIf("m_isAffectedByAugment")] private AugmentDefinition m_damageAugmentDefinition;
    [SerializeField, ShowIf("m_isAffectedByAugment")] private BulletTier[] m_bulletPerAugmentTier;

    private WorldBoundariesDefinition m_worldBoundaries;
    private float m_nextTimeToShoot;
    private float m_fireRateMultiplier = 1;

    private Dictionary<AugmentTierDefinition, BulletTier> m_bulletPerAugmentTierMap;
    private AugmentController.Augment m_activeAugment;

    public float SpreadAngleMultiplier { get; set; } = 1;
    public float IntermittentOffsetDurationMultiplier { get; set; } = 1;

    [System.Serializable]
    private class BulletTier
    {
        [SerializeField] private AugmentTierDefinition m_augmentTier;
        [SerializeField] private Material m_material;
        [SerializeField] private float m_scale = 1;

        public AugmentTierDefinition AugmentTier => m_augmentTier;
        public Material TierMaterial => m_material;
        public float TierScale => m_scale;
    }

    private void OnEnable()
    {
        if (m_delayBeforeFirstShot > 0)
        {
            StartCoroutine(DelayFirstShotRoutine());
        }
        else if (m_intermittentShooting)
        {
            StartCoroutine(IntermittenceRountine());
        }

        WorldPerspectiveManager.Instance.OnWorldPerspectiveChanged += OnWorldPerspectiveChanged;
        m_worldBoundaries = WorldPerspectiveManager.Instance.ActiveBoundaries;

        if (m_isAffectedByAugment && GameBlackboard.IsSingletonValid && AugmentController.IsSingletonValid)
        {
            // Listen to BB fire rate
            GameBlackboard.FireRateMultiplier.OnValueChanged += FireRateMultiplier_OnValueChanged;
            m_fireRateMultiplier = GameBlackboard.FireRateMultiplier.Value;

            // Retrieve damage augment and subscribe to event
            if (!AugmentController.Instance.TryGetAugment(m_damageAugmentDefinition, out m_activeAugment))
            {
                Debug.LogWarning($"Failed to retrieved '{m_damageAugmentDefinition.name}' from AugmentController", this);
            }

            m_bulletPerAugmentTierMap = new Dictionary<AugmentTierDefinition, BulletTier>(m_bulletPerAugmentTier.Length);
            foreach (var bulletTier in m_bulletPerAugmentTier)
            {
                m_bulletPerAugmentTierMap.Add(bulletTier.AugmentTier, bulletTier);
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (WorldPerspectiveManager.IsSingletonValid)
        {
            WorldPerspectiveManager.Instance.OnWorldPerspectiveChanged -= OnWorldPerspectiveChanged;
        }

        if (m_isAffectedByAugment && GameBlackboard.IsSingletonValid)
        {
            GameBlackboard.FireRateMultiplier.OnValueChanged -= FireRateMultiplier_OnValueChanged;
        }
    }

    private IEnumerator DelayFirstShotRoutine()
    {
        m_shootingEnable = false;
        yield return new WaitForSeconds(m_delayBeforeFirstShot);
        m_shootingEnable = true;

        if (m_intermittentShooting)
        {
            StartCoroutine(IntermittenceRountine());
        }
    }

    private IEnumerator IntermittenceRountine()
    {
        while (m_intermittentShooting)
        {
            yield return new WaitForSeconds(m_intermittenceOffset * IntermittentOffsetDurationMultiplier);
            m_shootingEnable = false;
            yield return new WaitForSeconds(m_intermittenceDuration);
            m_shootingEnable = true;
        }
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

                if (m_isAffectedByAugment && m_activeAugment.ActiveTier != null)
                {
                    if (m_bulletPerAugmentTierMap.TryGetValue(m_activeAugment.ActiveTier, out var data))
                    {
                        bulletObject.SetData(data.TierMaterial, data.TierScale);
                    }
                }

                // align to gun barrel/muzzle position
                bulletObject.transform.SetPositionAndRotation(muzzlePosition.position, muzzlePosition.rotation);

                // Generate a random rotation within the spread angle
                // need to constrain the y axis for top down and x for sidescroll
                bool constrainedXSpread = m_worldBoundaries.AxisRangeX.y - m_worldBoundaries.AxisRangeX.x == 0;
                bool constrainedYSpread = m_worldBoundaries.AxisRangeY.y - m_worldBoundaries.AxisRangeY.x == 0;

                float spread = m_spreadAngle * SpreadAngleMultiplier;
                Quaternion spreadRotation = Quaternion.Euler(0,
                    constrainedXSpread ? 0 : Random.Range(-spread, spread),
                    constrainedYSpread ? 0 : Random.Range(-spread, spread));
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
