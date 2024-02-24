using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

/* Some ideas to make shooting more interesting"
 * 1. Power-ups: Add power-ups that modify the behavior of projectiles, such as increasing their damage, size, speed, or adding special effects.
 * 2. Weapon Varieties: Introduce different types of projectiles with unique characteristics, such as homing missiles, area-of-effect bombs, or rapid-fire bullets.
 * 3. Enemy Patterns: Design enemies with varying movement patterns and behaviors, requiring the player to adjust their shooting strategy accordingly.
 * 4. Obstacles and Cover: Incorporate obstacles and cover elements that affect the trajectory of projectiles, adding a layer of tactical depth to the gameplay.
 * 5. Combo Systems: Implement a combo system where consecutive successful shots or kills reward the player with bonuses or special abilities, encouraging skillful play.
 * 6. Environmental Hazards: Introduce environmental hazards that interact with projectiles, such as explosive barrels or destructible elements that can be used strategically against enemies.
*/

public class RevisedProjectile : MonoBehaviour
{
    // deactivate after delay
    [SerializeField] private float timeoutDelay = 3f;

    private IObjectPool<RevisedProjectile> objectPool;

    // public property to give the projectile a reference to its ObjectPool
    public IObjectPool<RevisedProjectile> ObjectPool { set => objectPool = value; }

    public void Deactivate()
    {
        StartCoroutine(DeactivateRoutine(timeoutDelay));
    }

    IEnumerator DeactivateRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        // reset the moving Rigidbody
        Rigidbody rBody = GetComponent<Rigidbody>();
        rBody.velocity = new Vector3(0f, 0f, 0f);
        rBody.angularVelocity = new Vector3(0f, 0f, 0f);

        // release the projectile back to the pool
        objectPool.Release(this);
    }
}
