using NobunAtelier;
using NobunAtelier.Gameplay;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(HealthBehaviour))]
public class BattlerBehaviour : PoolableWithEvent
{
    [SerializeField] private UnityEvent m_onBattlerReadyToFight;
    //[Tooltip("1 means it will start fighting after the end of the travelling." +
    //    "\n0 means it will start to shoot as soon as it spawn." +
    //    "\n2 means it is going to wait 2 times the travel time before start shooting.")]
    //[SerializeField, Range(0, 3)] private float m_readyToFightAtTravelPercentage = 1f;

    [SerializeField] private MeshRenderer[] m_targetMeshes;
    [SerializeField] private Material m_skinMaterial;
    [SerializeField] private Material m_hitMaterial;
    private HealthBehaviour m_healthBehaviour;
    public void BattlerReadyToFight(float travelDuration)
    {
        StartCoroutine(ReadyToFightRountine(travelDuration));
    }

    private IEnumerator ReadyToFightRountine(float travelDuration)
    {
        yield return new WaitForSeconds(travelDuration);
        m_onBattlerReadyToFight?.Invoke();
    }

    // Start is called before the first frame update
    protected override void OnCreation()
    {
        base.OnCreation();
        m_healthBehaviour = GetComponent<HealthBehaviour>();
        OnInvulnerabilityEnd();
    }

    protected override void OnActivation()
    {
        base.OnActivation();
        m_healthBehaviour.OnBehaviourDeath += M_healthBehaviour_OnBehaviourDeath;
        m_healthBehaviour.OnReset.AddListener(OnInvulnerabilityEnd);
        m_healthBehaviour.OnInvulnerabilityBegin.AddListener(OnInvulnerabilityStart);
        m_healthBehaviour.OnInvulnerabilityEnd.AddListener(OnInvulnerabilityEnd);
        m_healthBehaviour.Reset();
        m_healthBehaviour.IsVulnerable = true;
    }

    private void M_healthBehaviour_OnBehaviourDeath(HealthBehaviour healthBehaviour)
    {
        StopAllCoroutines();
    }

    protected override void OnDeactivation()
    {
        StopAllCoroutines();
        m_healthBehaviour.OnBehaviourDeath -= M_healthBehaviour_OnBehaviourDeath;
        m_healthBehaviour.OnReset.RemoveListener(OnInvulnerabilityEnd);
        m_healthBehaviour.OnInvulnerabilityBegin.RemoveListener(OnInvulnerabilityStart);
        m_healthBehaviour.OnInvulnerabilityEnd.RemoveListener(OnInvulnerabilityEnd);
        m_healthBehaviour.IsVulnerable = false;
        base.OnDeactivation();
    }

    private void OnInvulnerabilityStart()
    {
        foreach (var mesh in m_targetMeshes)
        {
            mesh.material = m_hitMaterial;
        }
    }
    private void OnInvulnerabilityEnd()
    {
        foreach (var mesh in m_targetMeshes)
        {
            mesh.material = m_skinMaterial;
        }
    }
}
