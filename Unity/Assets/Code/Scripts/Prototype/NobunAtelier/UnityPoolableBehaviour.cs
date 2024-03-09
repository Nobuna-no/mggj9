using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;

namespace NobunAtelier
{
    public abstract class UnityPoolableBehaviour<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        [Header("UnityPoolable Behaviour")]
        [SerializeField] private UnityEvent m_onSpawn;
        [SerializeField] private UnityEvent m_onDespawn;
        [SerializeField, Min(0)] private float m_timeoutDelay = 3f;
        public float TimeoutDelay => m_timeoutDelay;
        public IObjectPool<T> ObjectPool { get; set; }

        public void Spawn()
        {
            OnSpawn();
            m_onSpawn?.Invoke();
            if (m_timeoutDelay > 0)
            {
                StartCoroutine(DespawnRoutine());
            }
            else
            {
                Despawn();
            }
        }

        public void Despawn()
        {
            StopCoroutine(DespawnRoutine());

            OnDespawn();

            m_onDespawn?.Invoke();
            T target = this.GetComponent<T>();
            ObjectPool.Release(target);
        }

        protected abstract void OnSpawn();

        protected abstract void OnDespawn();

        private IEnumerator DespawnRoutine()
        {
            yield return new WaitForSeconds(m_timeoutDelay);
            Despawn();
        }
    }
}