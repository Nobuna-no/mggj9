using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using System.Collections;



[RequireComponent(typeof(Rigidbody))]
public class PoolableShard : UnityPoolableBehaviour<PoolableShard>
{
    [SerializeField] private float m_defaultSpeed = 10f;
    [SerializeField] private float m_shardEnergyValue = 10f;

    public Rigidbody TargetRigidbody { get; private set; }
    public float Value => m_shardEnergyValue;

    public void Attracted()
    {
        StopAllCoroutines();
    }

    private void Awake()
    {
        TargetRigidbody = GetComponentInChildren<Rigidbody>();
    }

    protected override void OnSpawn()
    {
        float moveSpeed = GroundGenerator.Instance == null ? 1 : GroundGenerator.Instance.movingSpeed;
        TargetRigidbody.AddForce(Vector3.back * m_defaultSpeed, ForceMode.Acceleration);
    }

    protected override void OnDespawn()
    {
        TargetRigidbody.velocity = new Vector3(0f, 0f, 0f);
        TargetRigidbody.angularVelocity = new Vector3(0f, 0f, 0f);
    }
}
