using UnityEngine;
using UnityEngine.UI;

public class AugmentUI : PoolableBehaviour
{
    [SerializeField] private Image m_backgroundImage;
    [SerializeField] private Color m_backgroundColor = Color.gray;
    [SerializeField] private Image m_iconImage;
    [SerializeField] private Color m_iconColor = Color.white;
    [SerializeField] private Image m_tierImage;

    private AugmentController.Augment m_targetAugment;
    private AugmentDefinition m_targetDefinition;

    public void SetTargetAugment(AugmentDefinition target,AugmentTierDefinition tier)
    {
        Debug.Assert(target != null, $"{this.name}: AugmentDefinition is null");

        m_targetDefinition = target;
        m_iconImage.sprite = m_backgroundImage.sprite = target.Icon;
        if (AugmentController.Instance.TryGetAugment(m_targetDefinition, out m_targetAugment))
        {
            m_targetAugment.OnAugmentUpdate += OnProgressUpdate;
            m_targetAugment.OnAugmentDeactivated += OnAugmentEnd;
            m_targetAugment.OnAugmentTierChanged += OnTierChange;
            m_tierImage.sprite = tier.Icon;
        }
        else
        {
            Debug.LogWarning($"{this}: was not able to find a valid Augment for {target.name}", this);
        }
    }

    protected override void OnActivation()
    {
        m_iconImage.color = m_iconColor;
        m_backgroundImage.color = m_backgroundColor;
    }

    protected override void OnDeactivation()
    {
        m_targetAugment.OnAugmentUpdate -= OnProgressUpdate;
        m_targetAugment.OnAugmentDeactivated -= OnAugmentEnd;
        m_targetAugment.OnAugmentTierChanged -= OnTierChange;
        m_targetAugment = null;
    }

    private void OnProgressUpdate(float remainingTime)
    {
        m_iconImage.fillAmount = remainingTime;
    }

    private void OnTierChange(AugmentTierDefinition tier)
    {
        m_tierImage.sprite = tier.Icon;
    }

    private void OnAugmentEnd()
    {
        IsActive = false;
    }

    // AugmentUIManager is spawning AugmentIcon
    // 1. OnActivation, base color are setup
    // 2. AugmentUIManager is setting the target definition, The AugmentUI is then:
    //      1. Setting UI images
    //      2. Calling AugmentController to get and listen to AugmentUpdate
    // 3. Once the time reached zero, the icon unsubcribe itself and disable itself.
}
