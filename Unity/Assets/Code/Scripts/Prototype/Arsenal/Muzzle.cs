using UnityEngine;
using NobunAtelier;
using NaughtyAttributes;

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

    private WorldBoundariesDefinition m_worldBoundaries;
    private float m_nextTimeToShoot;
    private float m_fireRateMultiplier = 1;

    private void Start()
    {
        WorldPerspectiveManager.Instance.OnWorldPerspectiveChanged += OnWorldPerspectiveChanged;
        m_worldBoundaries = WorldPerspectiveManager.Instance.ActiveBoundaries;

        if (m_isAffectedByAugment && GameBlackboard.IsSingletonValid)
        {
            GameBlackboard.FireRateMultiplier.OnValueChanged += FireRateMultiplier_OnValueChanged;
            m_fireRateMultiplier = GameBlackboard.FireRateMultiplier.Value;
        }
    }

    private void FireRateMultiplier_OnValueChanged(float val)
    {
        m_fireRateMultiplier = val;
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
