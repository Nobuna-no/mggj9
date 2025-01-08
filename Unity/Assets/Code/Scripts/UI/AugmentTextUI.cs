using NobunAtelier;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class AugmentTextUI : FactoryProduct
{
    [Header("Refs")]
    [SerializeField] private RectTransform m_rectTransform;
    [SerializeField] private Color m_cancelColor = Color.grey;
    [SerializeField] private TextMeshProUGUI m_text;
    [SerializeField] private Image m_iconImage;
    [SerializeField] private Image m_tierImage;

    [Header("Polish")]
    [SerializeField] float m_fadeOutDuration = 3;
    [SerializeField] float m_cancelFadeOutDuration = .5f;
    [SerializeField] private AnimationCurve m_fadeOutAnimation = AnimationCurve.EaseInOut(0,0,1,1);
    public RectTransform RectTransform => m_rectTransform;

    private AugmentController.Augment m_targetAugment;
    private AugmentDefinition m_targetDefinition;
    private AugmentTierDefinition m_currentTier;
    private bool m_cancelled;

    public void SetTargetAugment(AugmentDefinition target, AugmentTierDefinition tier)
    {
        Debug.Assert(target != null, $"{this.name}: AugmentDefinition is null");

        m_targetDefinition = target;
        m_iconImage.sprite = target.Icon;
        m_tierImage.sprite = tier.Icon;
        m_text.color = tier.Color;
        m_iconImage.color = tier.Color;
        m_currentTier = tier;
        m_text.text = $"{target.DisplayName} {tier.DisplayTierName}";

        if (AugmentController.Instance.TryGetAugment(m_targetDefinition, out m_targetAugment))
        {
            m_targetAugment.OnAugmentDeactivated += OnAugmentEnd;
            m_targetAugment.OnAugmentTierChanged += OnTierChange;
        }
        else
        {
            Debug.LogWarning($"{this}: was not able to find a valid Augment for {target.name}", this);
        }

        StartCoroutine(DespawnRoutine());
    }

    protected override void OnProductActivation()
    {
        m_iconImage.color = Color.white;
        m_text.color = Color.white;
        m_cancelled = false;
    }

    protected override void OnProductDeactivation()
    {
        m_targetAugment.OnAugmentDeactivated -= OnAugmentEnd;
        m_targetAugment.OnAugmentTierChanged -= OnTierChange;
        m_targetAugment = null;
    }

    private void OnTierChange(AugmentTierDefinition tier)
    {
        if (m_currentTier == tier)
        {
            return;
        }

        // m_tierImage.sprite = tier.Icon;
        m_cancelled = true;
    }

    private void OnAugmentEnd()
    {
        m_cancelled = true;
    }

    private IEnumerator DespawnRoutine()
    {
        float t = 0;
        float duration = m_fadeOutDuration;

        m_text.alpha = 1;

        Color imgOriginColor = m_iconImage.color;
        Color imgDestColor = imgOriginColor;
        imgDestColor.a = 0;

        Color tierOriginColor = m_tierImage.color;
        Color tierDestColor = tierOriginColor;
        tierDestColor.a = 0;

        while (t < 1f)
        {
            if (m_cancelled)
            {
                imgOriginColor = m_text.color = m_iconImage.color = m_cancelColor;
                imgDestColor = m_cancelColor * new Color(1, 1, 1, 0);
                t = 0;
                duration = m_cancelFadeOutDuration;
                m_cancelled = false;
            }

            yield return null;
            t += Time.deltaTime / duration;
            //m_rectTransform.position = Mathf.Lerp(originalPos, destination
            m_iconImage.color = Color.Lerp(imgOriginColor, imgDestColor, m_fadeOutAnimation.Evaluate(t));
            m_text.alpha = m_fadeOutAnimation.Evaluate(1 - t);
            m_tierImage.color = Color.Lerp(tierOriginColor, tierDestColor, m_fadeOutAnimation.Evaluate(t));
        }

        Release();
    }
}
