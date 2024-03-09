using UnityEngine;
using UnityEngine.Pool;

namespace NobunAtelier
{
    public abstract class UnityPoolBehaviour<T> : MonoBehaviour
        where T : UnityPoolableBehaviour<T>
    {
        [Header("UnityPool Behaviour")]
        [Tooltip("Prefab to shoot")]
        [SerializeField] private T m_projectilePrefab;

        [Tooltip("Throw an exception if we try to return an existing item, already in the pool")]
        [SerializeField] private bool m_collectionCheck = true;

        [Tooltip("Control the pool capacity and maximum size.")]
        [SerializeField] private int m_defaultCapacity = 20;

        [SerializeField] private int m_maxSize = 100;

        private IObjectPool<T> m_objectPool;
        public IObjectPool<T> Pool => m_objectPool;

        private void Awake()
        {
            m_objectPool = new ObjectPool<T>(CreateProjectile,
                OnGetFromPool, OnReleaseToPool, OnDestroyPooledObject,
                m_collectionCheck, m_defaultCapacity, m_maxSize);
        }

        // invoked when creating an item to populate the object pool
        protected virtual T CreateProjectile()
        {
            T projectileInstance = Instantiate(m_projectilePrefab);
            projectileInstance.ObjectPool = m_objectPool;
            return projectileInstance;
        }

        // invoked when returning an item to the object pool
        protected virtual void OnReleaseToPool(T pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
        }

        // invoked when retrieving the next item from the object pool
        protected virtual void OnGetFromPool(T pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
        }

        // invoked when we exceed the maximum number of pooled items (i.e. destroy the pooled object)
        protected virtual void OnDestroyPooledObject(T pooledObject)
        {
            Destroy(pooledObject.gameObject);
        }
    }
}
