using NaughtyAttributes;
using NobunAtelier;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerTile : FactoryProduct
{
    [SerializeField] private Transform m_startPoint;
    [SerializeField] private Transform m_endPoint;
    [SerializeField] private GameObject[] m_obstacles;
    [SerializeField, MinMaxSlider(0, 10)] private Vector2 m_obstaclesSpawnCountRange = Vector2.one * 3;
    public Transform StartPoint => m_startPoint;
    public Transform EndPoint => m_endPoint;

    private List<GameObject> m_availableObstacles = new List<GameObject>();
    private List<GameObject> m_inUseObstacles = new List<GameObject>();

    public void ActivateRandomObstacle()
    {
        if (m_obstacles.Length == 0 || m_obstaclesSpawnCountRange.y == 0)
        {
            return;
        }

        int min = (int)Mathf.Min(m_availableObstacles.Count, m_obstaclesSpawnCountRange.x);
        int max = (int)Mathf.Min(m_availableObstacles.Count, m_obstaclesSpawnCountRange.y);
        int count = Random.Range(min, max);
        for (int i = 0; i < count; ++i)
        {
            int obstacleIndex = Random.Range(0, m_availableObstacles.Count);
            var obstacle = m_availableObstacles[obstacleIndex];
            var poolable = obstacle.GetComponent<FactoryProduct>();
            if (poolable != null)
            {
                poolable.IsProductActive = true;
                // Debug.Log($"{this}: Activating obstacle{poolable}.");
            }
            else
            {
                obstacle.SetActive(true);
            }

            m_inUseObstacles.Add(obstacle);
            m_availableObstacles.RemoveAt(obstacleIndex);
        }
    }

    public void DeactivateObstacles()
    {
        for (int i = m_inUseObstacles.Count - 1; i >= 0; --i)
        {
            m_availableObstacles.Add(m_inUseObstacles[i]);
        }
        m_inUseObstacles.Clear();

        for (int i = 0; i < m_obstacles.Length; i++)
        {
            var product = m_obstacles[i].GetComponent<FactoryProduct>();
            if (product != null)
            {
                product.Release();
                // Debug.Log($"{this}: Deactivating obstacle{product}.");
            }
            else
            {
                m_obstacles[i].SetActive(false);
            }
        }
    }

    protected override void OnProductActivation()
    {
        base.OnProductActivation();
        DeactivateObstacles();
        m_availableObstacles = new List<GameObject>(m_obstacles);
        m_inUseObstacles = new List<GameObject>();
    }

    protected override void OnProductDeactivation()
    {
        base.OnProductDeactivation();

        DeactivateObstacles();

        m_availableObstacles.Clear();
        m_availableObstacles = null;
        m_inUseObstacles.Clear();
        m_inUseObstacles = null;
    }
}
