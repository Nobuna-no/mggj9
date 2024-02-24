using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunnerTile : PoolableBehaviour
{
    [SerializeField] private Transform m_startPoint;
    [SerializeField] private Transform m_endPoint;
    [SerializeField] private GameObject[] m_obstacles;
    public Transform StartPoint => m_startPoint;
    public Transform EndPoint => m_endPoint;


    public void ActivateRandomObstacle()
    {
        int obstacleIndex = Random.Range(0, m_obstacles.Length);
        m_obstacles[obstacleIndex].SetActive(true);
    }

    public void DeactivateObstacles()
    {
        for (int i = 0; i < m_obstacles.Length; i++)
        {
            m_obstacles[i].SetActive(false);
        }
    }

    protected override void OnDeactivation()
    {
        base.OnDeactivation();

        DeactivateObstacles();
    }
}
