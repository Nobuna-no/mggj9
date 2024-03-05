using UnityEngine;

public class MagicMuzzle : UnityPoolBehaviour<PoolableRigidbody>
{
    [Header("Magic Muzzle")]
    [Tooltip("Projectile force")]
    [SerializeField] private float m_muzzleVelocity = 700f;
    [Tooltip("End point of gun where shots appear")]
    [SerializeField] private Transform[] m_muzzlesPosition;
    [Tooltip("Time between shots / smaller = higher rate of fire")]
    [SerializeField] private float m_cooldownWindow = 0.1f;
    [SerializeField] float m_spreadAngle = 5f; // Adjust this value to control the spread
    [SerializeField] private bool m_shootingEnable = false;

    private float m_nextTimeToShoot;

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

                // get a pooled object instead of instantiating
                PoolableRigidbody bulletObject = Pool.Get();

                if (bulletObject == null)
                    continue;

                // align to gun barrel/muzzle position
                bulletObject.transform.SetPositionAndRotation(muzzlePosition.position, muzzlePosition.rotation);

                // Define a spread angle (in degrees)

                // Generate a random rotation within the spread angle
                Quaternion spreadRotation = Quaternion.Euler(Random.Range(-m_spreadAngle, m_spreadAngle), Random.Range(-m_spreadAngle, m_spreadAngle), 0f);

                // Apply the spread rotation to the bullet's forward direction
                Vector3 spreadDirection = spreadRotation * bulletObject.transform.forward;

                // Move projectile forward
                bulletObject.GetComponent<Rigidbody>().AddForce(spreadDirection * m_muzzleVelocity, ForceMode.Acceleration);
                // bulletObject.GetComponent<Rigidbody>().AddForce(bulletObject.transform.forward * muzzleVelocity, ForceMode.Acceleration);

                // turn off after a few seconds
                bulletObject.Spawn();
            }

            // set cooldown delay
            m_nextTimeToShoot = Time.time + m_cooldownWindow;
        }
    }
}
