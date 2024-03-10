using NaughtyAttributes;
using NobunAtelier;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

// Which programming pattern best fit my need? (while keeping velocity & flexibility in mind)
//  I feel like DataDefinition is working well when designing a data driven system.
//  Otherwise, if we need a sytem that interact with a lots of other systems,
//  the observer pattern (Unity Event) should be the way to go.
//
//  For SIMULACRA, the 'Crystal Augment' system is a bonus system that can affect
//  a wide variety of parameters (move speed, firerate, gao activation, ...)
//       - aka a lot of different system.
//  So yeah, going for a Component that have a list of Tier and Gao sound more reasonable here.
//  This way I don't have to create one script per bonus, but simply handling event in the editor.
//
// I also think I should avoid one component per bonus, as we might have to pay the cost of several update...

/* The goal of the implementation is to leverage the data-driven nature of Scriptable Objects for
 * defining bonuses and their common properties, while also allowing to define tier-specific
 * functionalities using MonoBehaviour. A kind of Hybrid Data-Driven System...
 */
public class AugmentController : Singleton<AugmentController>
{
    [SerializeField] private AugmentCollection m_augmentsCollection;
    [SerializeField] private AugmentTierCollection m_augmentTierCollection;
    [SerializeField] private Augment[] m_augment;

    [SerializeField] private AugmentDefinition m_debugAugmentDefinition;
    [SerializeField] private AugmentTierDefinition m_debugAugmentTierDefinition;

    private Dictionary<AugmentTierDefinition, List<AugmentDefinition>> m_augmentTiersMap;
    private Dictionary<AugmentDefinition, Augment> m_augmentsMap;

    public void Awake()
    {
        Debug.Assert(m_augmentsCollection != null, "m_augmentsCollection != null");
        Debug.Assert(m_augmentTierCollection != null, "m_augmentTierCollection != null");

        m_augmentTiersMap = new Dictionary<AugmentTierDefinition, List<AugmentDefinition>>(m_augmentTierCollection.Definitions.Count);
        foreach (var tier in m_augmentTierCollection.Definitions)
        {
            m_augmentTiersMap.Add(tier, new List<AugmentDefinition>());
            foreach (var augment in m_augmentsCollection.Definitions)
            {
                foreach (var availableTier in augment.Tiers)
                {
                    if (availableTier != tier)
                    {
                        continue;
                    }

                    m_augmentTiersMap[tier].Add(augment);
                    break;
                }
            }
        }

        m_augmentsMap = new Dictionary<AugmentDefinition, Augment>(m_augmentsCollection.Definitions.Count);
        foreach (var augment in m_augmentsCollection.Definitions)
        {
            foreach (var behaviour in m_augment)
            {
                if (behaviour.AugmentDefinition != augment)
                {
                    continue;
                }

                m_augmentsMap.Add(augment, behaviour);
                behaviour.Initialize(this);
                break;
            }
        }
    }

    public AugmentDefinition GetRandomAugment(AugmentTierDefinition tier)
    {
        if (m_augmentTiersMap.TryGetValue(tier, out var augments))
        {
            return augments[Random.Range(0, augments.Count)];
        }

        return null;
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void ActivateRandomAugment()
    {
        var tier = m_augmentTierCollection.GetRandomBonusTier();
        var augment = GetRandomAugment(tier);

        if (m_augmentsMap.TryGetValue(augment, out var logic))
        {
            logic.Activate(tier);
        }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void ActivateDebugAugmentAndTier()
    {
        if (m_debugAugmentDefinition == null || m_debugAugmentTierDefinition == null)
        {
            return;
        }

        if (m_augmentsMap.TryGetValue(m_debugAugmentDefinition, out var logic))
        {
            logic.Activate(m_debugAugmentTierDefinition);
        }
    }

    public bool TryGetAugment(AugmentDefinition definition, out Augment out_augment)
    {
        if (m_augmentsMap.TryGetValue(definition, out out_augment))
        {
            return true;
        }

        return false;
    }

    [System.Serializable]
    public class Augment
    {
        [SerializeField] private string m_name;
        public AugmentDefinition AugmentDefinition;
        public AugmentBehaviour[] TierLogic;
        public UnityEvent OnAnyTierDeactivated;
        public event Action<float> OnAugmentUpdate;
        public event Action OnAugmentDeactivated;
        public event Action<AugmentTierDefinition> OnAugmentTierChanged;

        private MonoBehaviour m_owner;

        [Header("EXPERIMENTAL")]
        private bool m_tierLevelDownToDeactivate = true;

        private float m_remainingProgress = 0;
        public bool IsActive { get; protected set; }
        public AugmentTierDefinition ActiveTier { get; protected set; }

        private Dictionary<AugmentTierDefinition, AugmentBehaviour> m_augmentBehavioursMap;

        public void Initialize(MonoBehaviour owner)
        {
            m_owner = owner;

            m_augmentBehavioursMap = new Dictionary<AugmentTierDefinition, AugmentBehaviour>(AugmentDefinition.Tiers.Length);
            foreach (var tier in AugmentDefinition.Tiers)
            {
                foreach (var logic in TierLogic)
                {
                    if (logic.TierDefinition == tier)
                    {
                        m_augmentBehavioursMap.Add(tier, logic);
                        break;
                    }
                }
            }
        }

        public void Activate(AugmentTierDefinition tier)
        {
            Debug.Assert(m_augmentBehavioursMap.ContainsKey(tier), "m_augmentBehavioursMap.ContainsKey(tier)");
            if (IsActive/*&& !m_augmentBehavioursMap[tier].InheritsPreviousTiersEffect*/)
            {
                Deactivate();
            }
            else
            {
                // Was not actived yet, spawn UI
                SimulacraUIManager.Instance.SpawnAugmentUI(AugmentDefinition);
            }

            if (m_remainingProgress <= 0)
            {
                m_owner.StartCoroutine(CountDown());
            }
            else
            {
                m_remainingProgress = AugmentDefinition.Duration;
            }

            ActiveTier = tier;
            m_augmentBehavioursMap[tier]?.OnTierActivated?.Invoke();
            IsActive = true;
        }

        public IEnumerator CountDown()
        {
            m_remainingProgress = 1;
            while (m_remainingProgress > 0)
            {
                m_remainingProgress -= Time.deltaTime / AugmentDefinition.Duration;
                OnAugmentUpdate?.Invoke(m_remainingProgress);
                // Trigger the event with the remaining time as parameter
                yield return null;
            }

            Deactivate();
        }

        public void Deactivate()
        {
            if (!IsActive)
            {
                return;
            }

            OnAnyTierDeactivated?.Invoke();
            Debug.Assert(m_augmentBehavioursMap.ContainsKey(ActiveTier), $"Augment's m_augmentBehavioursMap doesn't contain ActiveTier.");
            m_augmentBehavioursMap[ActiveTier].OnTierDeactivated?.Invoke();
            IsActive = false;
            ActiveTier = null;
        }
    }

    [System.Serializable]
    public class AugmentBehaviour
    {
        [SerializeField] private AugmentTierDefinition m_tierDefinition;
        [SerializeField] private UnityEvent m_onTierActivated;
        [SerializeField] private UnityEvent m_onTierDeactivated;

        public AugmentTierDefinition TierDefinition => m_tierDefinition;
        public UnityEvent OnTierActivated => m_onTierActivated;
        public UnityEvent OnTierDeactivated => m_onTierDeactivated;
    }
}