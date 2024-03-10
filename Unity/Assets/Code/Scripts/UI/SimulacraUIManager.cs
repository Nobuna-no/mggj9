using NobunAtelier;
using UnityEngine;

public class SimulacraUIManager : Singleton<SimulacraUIManager>
{
    [Header("Augment")]
    [SerializeField] private RectTransform m_augmentsParent;

    [SerializeField] private PoolUIDefinition m_augmentUI;

    public void SpawnAugmentUI(AugmentDefinition augment)
    {
        var ui = PoolManager.Instance.SpawnObject(m_augmentUI, Vector3.zero) as AugmentUI;
        ui.GetComponent<RectTransform>().parent = m_augmentsParent;
        ui.SetTargetAugment(augment);
    }
}
