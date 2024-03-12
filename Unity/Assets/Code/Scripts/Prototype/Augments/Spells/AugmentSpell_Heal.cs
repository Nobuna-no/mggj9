using NaughtyAttributes;
using NobunAtelier.Gameplay;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AugmentSpell_Heal : MonoBehaviour
{
    [SerializeField] private AugmentDefinition m_healAugmentDefinition;
    [SerializeField] private HealthBehaviour m_healthBehaviour;
    [SerializeField] private HealTier[] m_healPerAugmentTier;
    [SerializeField] private UnityEvent m_onHeal;
    [SerializeField] private UnityEvent m_onSelfDamage;
    private AugmentController.Augment m_healAugment;
    private Dictionary<AugmentTierDefinition, HealTier> m_healPerAugmentTierMap;

    private void Awake()
    {
        if (!AugmentController.IsSingletonValid || !AugmentController.Instance.TryGetAugment(m_healAugmentDefinition, out m_healAugment))
        {
            Debug.LogWarning($"Failed to retrieved '{m_healAugmentDefinition.name}' from AugmentController", this);
        }

        m_healPerAugmentTierMap = new Dictionary<AugmentTierDefinition, HealTier>(m_healPerAugmentTier.Length);
        foreach (var healTier in m_healPerAugmentTier)
        {
            m_healPerAugmentTierMap.Add(healTier.AugmentTier, healTier);
        }

        m_healAugment.OnAugmentTierChanged += M_healAugment_OnAugmentTierChanged;
        this.enabled = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        m_healAugment.OnAugmentTierChanged -= M_healAugment_OnAugmentTierChanged;
        m_healAugment = null;
    }

    private void M_healAugment_OnAugmentTierChanged(AugmentTierDefinition tier)
    {
        StopAllCoroutines();

        if (m_healPerAugmentTierMap.TryGetValue(tier, out var healData))
        {
            StartCoroutine(HealRoutine(healData));
        }
        else
        {
            Debug.LogWarning($"Failed to find heal tier {tier.name}", this);
        }
    }

    private IEnumerator HealRoutine(HealTier heal)
    {
        while (true)
        {
            if (heal.HealValue > 0)
            {
                m_healthBehaviour.Heal(heal.HealValue);
                m_onHeal?.Invoke();
            }
            else
            {
                m_healthBehaviour.ApplyDamage(heal.HitDefinition, transform.position, this.gameObject, true);
                m_onSelfDamage?.Invoke();
            }
            yield return new WaitForSeconds(heal.HealOffset);
        }
    }

    [System.Serializable]
    private class HealTier
    {
        [SerializeField] private AugmentTierDefinition m_augmentTier;
        [SerializeField] private float m_healOffset = 1;
        [SerializeField] private float m_healValue = 1;
        [SerializeField, AllowNesting, ShowIf("IsDamage")] private HitDefinition m_hitDefinition;

        public AugmentTierDefinition AugmentTier => m_augmentTier;
        public float HealValue => m_healValue;
        public float HealOffset => m_healOffset;
        public HitDefinition HitDefinition => m_hitDefinition;
        public bool IsDamage => m_healValue < 0;
    }
}
