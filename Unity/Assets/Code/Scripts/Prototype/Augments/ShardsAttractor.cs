using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Absorbs shard and generate crystal
[RequireComponent(typeof(SphereCollider))]
public class ShardsAttractor : MonoBehaviour
{
    [SerializeField, Layer] private int m_shardLayer;
    [SerializeField, Tooltip("In seconds.")] private float m_attractionDuration = 5f;
    [SerializeField] private AnimationCurve m_attractionTweenCurve;
    [SerializeField] private float m_absorbtionRange = 0.2f;
    [SerializeField] private float m_shardValueToLevelUp = 10f;
    [SerializeField] private bool m_canAttractAndAbsorb = true;

    public event Action<float, float> OnShardValueChanged;
    public event Action OnLevelUp;

    private float m_accumulatedShardValue = 0;

    private readonly List<PoolableShard> m_shardsList = new List<PoolableShard>();

    private readonly Dictionary<PoolableShard, SpaceTime> m_shardsSpaceTime =
        new Dictionary<PoolableShard, SpaceTime>();

    private SphereCollider m_collider;
    private bool m_isRoutineRunning = false;

    private float m_attractionDurationMultiplier = 1f;
    private bool m_isRegistered = false;

    public void EnableAttraction()
    {
        m_canAttractAndAbsorb = true;
    }

    public void DisableAbsorption()
    {
        m_canAttractAndAbsorb = false;
    }

    private void Awake()
    {
        m_isRoutineRunning = false;
        m_collider = GetComponent<SphereCollider>();
        m_collider.isTrigger = true;
        m_collider.gameObject.layer = m_shardLayer;
    }

    private void Start()
    {
        RefreshAttractionSettings(GameBlackboard.AttractionSettings.Value);
        OnEnable();
    }

    private void OnEnable()
    {
        // Skip the first OnEnable as singleton might not have initialized yet.
        if (m_isRegistered || !GameBlackboard.IsSingletonValid)
        {
            return;
        }

        GameBlackboard.AttractionSettings.OnValueChanged += RefreshAttractionSettings;
        m_isRegistered = true;
    }

    private void OnDisable()
    {
        if (!m_isRegistered)
        {
            return;
        }

        GameBlackboard.AttractionSettings.OnValueChanged -= RefreshAttractionSettings;
        m_isRegistered = false;
    }

    private void RefreshAttractionSettings(ShardsAttractionDefinition value)
    {
        m_collider.radius = value.Radius;
        m_attractionDurationMultiplier = value.DurationMultiplier;
    }

    private void AttractShards()
    {
        for (int i = m_shardsList.Count - 1; i >= 0; i--)
        {
            PoolableShard shard = m_shardsList[i];
            Vector3 direction = transform.position - shard.TargetRigidbody.transform.position;
            float distanceSquared = direction.sqrMagnitude;

            if (distanceSquared < m_absorbtionRange * m_absorbtionRange)
            {
                m_accumulatedShardValue += shard.Value;
                shard.Despawn();
                m_shardsList.RemoveAt(i);
                m_shardsSpaceTime.Remove(shard);
                CheckLevelUp();
                OnShardValueChanged?.Invoke(m_accumulatedShardValue, m_shardValueToLevelUp);
            }
            else
            {
                var spaceTime = m_shardsSpaceTime[shard];
                // shard.TargetRigidbody.AddForce(direction.normalized * m_attractionDuration, ForceMode.Acceleration);
                // shard.TargetRigidbody.velocity = direction.normalized * m_attractionDuration;
                shard.TargetRigidbody.position = Vector3.Lerp(spaceTime.Origin, transform.position, m_attractionTweenCurve.Evaluate(spaceTime.Time));
                spaceTime.Time += Time.fixedDeltaTime / (m_attractionDuration * m_attractionDurationMultiplier);
            }
        }
    }

    private void CheckLevelUp()
    {
        if (m_accumulatedShardValue > m_shardValueToLevelUp)
        {
            m_accumulatedShardValue -= m_shardValueToLevelUp;
            OnLevelUp?.Invoke();
            CheckLevelUp();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_canAttractAndAbsorb)
        {
            return;
        }

        var shard = other.GetComponent<PoolableShard>();
        if (shard == null)
        {
            return;
        }

        if (!m_shardsList.Contains(shard))
        {
            m_shardsSpaceTime[shard] = new SpaceTime() { Origin = shard.transform.position, Time = 0 };
            m_shardsList.Add(shard);
            shard.Attracted();
        }

        if (m_isRoutineRunning == false)
        {
            StartCoroutine(AttractionRoutine());
        }
    }

    private void OnDestroy()
    {
        if (m_shardsList.Count == 0)
        {
            StopCoroutine(AttractionRoutine());
        }
    }

    private IEnumerator AttractionRoutine()
    {
        m_isRoutineRunning = true;
        while (m_shardsList.Count > 0)
        {
            AttractShards();
            yield return new WaitForFixedUpdate();
        }
        m_isRoutineRunning = false;
    }

    private class SpaceTime
    {
        public Vector3 Origin;
        public float Time;
    }
}
