using NobunAtelier;
using NobunAtelier.Gameplay;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(HealthBehaviour))]
public class BattlerBehaviour : PoolableWithEvent
{
    //[Tooltip("1 means it will start fighting after the end of the travelling." +
    //    "\n0 means it will start to shoot as soon as it spawn." +
    //    "\n2 means it is going to wait 2 times the travel time before start shooting.")]
    //[SerializeField, Range(0, 3)] private float m_readyToFightAtTravelPercentage = 1f;
    [SerializeField] private Muzzle m_battlerWeapon;
    [SerializeField] private MeshRenderer[] m_targetMeshes;
    [SerializeField] private Material m_skinMaterial;
    [SerializeField] private Material m_hitMaterial;
    private HealthBehaviour m_healthBehaviour;

    public void SetBattlerSettings(BattleWaveDefinition.BattlerSettings settings)
    {
        StartCoroutine(InvulnerabilityDelayRoutine(settings.InvulnerabilityAtSpawnDuration));

        if (settings.CanShoot)
        {
            StartCoroutine(ReadyToFightRountine(settings));
        }
    }

    private IEnumerator InvulnerabilityDelayRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        m_healthBehaviour.IsVulnerable = true;
    }

    private IEnumerator ReadyToFightRountine(BattleWaveDefinition.BattlerSettings settings)
    {
        yield return new WaitForSeconds(settings.DelayBeforeStartShooting);
        m_battlerWeapon.IntermittentOffsetDurationMultiplier = settings.IntermittenShootOffset;
        m_battlerWeapon.enabled = true;
    }

    // Start is called before the first frame update
    protected override void OnCreation()
    {
        base.OnCreation();
        m_healthBehaviour = GetComponent<HealthBehaviour>();
        OnInvulnerabilityEnd();
        Debug.Assert(m_battlerWeapon, this);
    }

    protected override void OnActivation()
    {
        base.OnActivation();
        m_healthBehaviour.OnBehaviourDeath += M_healthBehaviour_OnBehaviourDeath;
        m_healthBehaviour.OnReset.AddListener(OnInvulnerabilityEnd);
        m_healthBehaviour.OnInvulnerabilityBegin.AddListener(OnInvulnerabilityStart);
        m_healthBehaviour.OnInvulnerabilityEnd.AddListener(OnInvulnerabilityEnd);
        m_healthBehaviour.Reset();
        m_healthBehaviour.IsVulnerable = false; // invulnerable to prevent spawn kill
        m_battlerWeapon.enabled = false;
    }

    private void M_healthBehaviour_OnBehaviourDeath(HealthBehaviour healthBehaviour)
    {
        StopAllCoroutines();
    }

    protected override void OnDeactivation()
    {
        m_battlerWeapon.IntermittentOffsetDurationMultiplier = 1;
        m_battlerWeapon.enabled = false;
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
