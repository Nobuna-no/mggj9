using NobunAtelier;
using NobunAtelier.Gameplay;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
public class SimulacraUIManager : Singleton<SimulacraUIManager>
{
    [SerializeField] private Camera m_gameplayCamera;

    [Header("Stats")]
    [SerializeField] private ShardsAttractor m_shardsAttractor;

    [SerializeField] private HealthBehaviour m_playerHealth;
    [SerializeField] private Image m_healthBar;
    [SerializeField] private Image m_shardBar;

    [Header("Augment")]
    [SerializeField] private RectTransform m_augmentsParent;

    [SerializeField] private PoolUIDefinition m_augmentUI;
    [SerializeField] private PoolUIDefinition m_augmentTextUI;

    // Start is called before the first frame update
    private void OnEnable()
    {
        Debug.Assert(m_augmentUI != null);
        Debug.Assert(m_augmentTextUI != null);

        m_playerHealth.OnHealthChanged += M_playerHealth_OnHealthChanged;
        m_shardsAttractor.OnShardValueChanged += M_shardsAttractor_OnShardValueChanged;
        m_healthBar.fillAmount = 1;
        m_shardBar.fillAmount = 0;
    }

    private void OnDisable()
    {
        m_playerHealth.OnHealthChanged -= M_playerHealth_OnHealthChanged;
        m_shardsAttractor.OnShardValueChanged -= M_shardsAttractor_OnShardValueChanged;
    }

    private void M_shardsAttractor_OnShardValueChanged(float currentValue, float maxValue)
    {
        m_shardBar.fillAmount = currentValue / maxValue;
    }

    private void M_playerHealth_OnHealthChanged(float currentHealth, float maxHealth)
    {
        m_healthBar.fillAmount = currentHealth / maxHealth;
    }

    public void SpawnAugmentUI(AugmentDefinition augment, AugmentTierDefinition tier)
    {
        var icon = PoolManager.Instance.SpawnObject(m_augmentUI, Vector3.zero) as AugmentUI;
        icon.GetComponent<RectTransform>().SetParent(m_augmentsParent, false);
        icon.SetTargetAugment(augment, tier);
    }

    public void SpawnAugmentTextUI(AugmentDefinition augment, AugmentTierDefinition tier)
    {
        var text = PoolManager.Instance.SpawnObject(m_augmentTextUI, Vector3.zero) as AugmentTextUI;
        var screenPos = m_gameplayCamera.WorldToScreenPoint(m_playerHealth.ModuleOwner.Position);
        text.RectTransform.position = screenPos;

        text.SetTargetAugment(augment, tier);
    }
}
