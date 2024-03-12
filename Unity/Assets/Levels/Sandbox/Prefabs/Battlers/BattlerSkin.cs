using NobunAtelier.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthBehaviour))]
public class BattlerSkin : MonoBehaviour
{
    [SerializeField] private MeshRenderer[] m_targetMeshes;
    [SerializeField] private Material m_skinMaterial;
    [SerializeField] private Material m_hitMaterial;
    private HealthBehaviour m_healthBehaviour;

    // Start is called before the first frame update
    void Awake()
    {
        m_healthBehaviour = GetComponent<HealthBehaviour>();
        OnInvulnerabilityEnd();
    }

    private void OnEnable()
    {
        m_healthBehaviour.OnReset.AddListener(OnInvulnerabilityEnd);
        m_healthBehaviour.OnInvulnerabilityBegin.AddListener(OnInvulnerabilityStart);
        m_healthBehaviour.OnInvulnerabilityEnd.AddListener(OnInvulnerabilityEnd);
    }

    private void OnDisable()
    {
        m_healthBehaviour.OnReset.RemoveListener(OnInvulnerabilityEnd);
        m_healthBehaviour.OnInvulnerabilityBegin.RemoveListener(OnInvulnerabilityStart);
        m_healthBehaviour.OnInvulnerabilityEnd.RemoveListener(OnInvulnerabilityEnd);
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
