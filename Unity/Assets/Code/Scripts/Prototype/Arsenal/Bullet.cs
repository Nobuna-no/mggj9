using NaughtyAttributes;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using NobunAtelier;

/* Some ideas to make shooting more interesting"
 * 1. Power-ups: Add power-ups that modify the behavior of projectiles, such as increasing their damage, size, speed, or adding special effects.
 * 2. Weapon Varieties: Introduce different types of projectiles with unique characteristics, such as homing missiles, area-of-effect bombs, or rapid-fire bullets.
 * 3. Enemy Patterns: Design enemies with varying movement patterns and behaviors, requiring the player to adjust their shooting strategy accordingly.
 * 4. Obstacles and Cover: Incorporate obstacles and cover elements that affect the trajectory of projectiles, adding a layer of tactical depth to the gameplay.
 * 5. Combo Systems: Implement a combo system where consecutive successful shots or kills reward the player with bonuses or special abilities, encouraging skillful play.
 * 6. Environmental Hazards: Introduce environmental hazards that interact with projectiles, such as explosive barrels or destructible elements that can be used strategically against enemies.
*/

[RequireComponent(typeof(Rigidbody))]
public class Bullet : UnityPoolableBehaviour<Bullet>
{
    [SerializeField] private ParticleSystem m_particleSystem;
    // public property to give the projectile a reference to its ObjectPool
    public Rigidbody TargetRigidbody { get; private set; }
    private Vector3 m_originalScale;
    private MeshRenderer m_mesh;
    private Material m_defaultMaterial;

    private void Awake()
    {
        TargetRigidbody = GetComponentInChildren<Rigidbody>();
        m_originalScale = transform.localScale;
        m_mesh = GetComponent<MeshRenderer>();
        m_defaultMaterial = m_mesh.material;
    }

    protected override void OnSpawn()
    {

    }

    protected override void OnDespawn()
    {
        // reset the moving Rigidbody
        TargetRigidbody.linearVelocity = new Vector3(0f, 0f, 0f);
        TargetRigidbody.angularVelocity = new Vector3(0f, 0f, 0f);
        transform.localScale = m_originalScale;
        m_mesh.material = m_defaultMaterial;
    }

    public void SetData(Material material, float scale)
    {
        transform.localScale = m_originalScale * scale;
        m_mesh.material = material;
    }

    public void SpawnParticle()
    {
        var fx = Instantiate(m_particleSystem, transform.position, transform.rotation);
        fx.Play();
    }
}
