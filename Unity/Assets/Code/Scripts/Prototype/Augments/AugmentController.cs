using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;
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
    [Header("System")]
    [SerializeField] private ShardsAttractor m_attractor;

    [SerializeField] private Augment[] m_augment;
    [SerializeField] private AugmentCollection m_augmentsCollection;
    [SerializeField] private AugmentTierCollection m_augmentTierCollection;

    [Header("Gacha Juiciness")]
    [SerializeField] private AugmentTierDefinition m_pityAugmentTier;
    [SerializeField] private UnityEvent m_onPityCrystalObtained;
    [SerializeField] private SplineContainer m_crystalSpline;
    [SerializeField] private SplineContainer m_crystalSplineOpeningPath;
    [SerializeField] private float m_crystalOpeningDuration = 1f;
    [SerializeField] private float m_offsetDurationBetweenOpening = 0.5f;
    [SerializeField, ReadOnly] private int m_remaingCrystalToOpen = 0;

    [Header("A/B Testing")]
    private bool m_tierLevelDownToDeactivate = true;

    private Dictionary<AugmentDefinition, Augment> m_augmentsMap;
    private Dictionary<string, bool> m_lazyBoolmap;
    private Dictionary<AugmentTierDefinition, List<AugmentDefinition>> m_augmentTiersMap;
    private bool m_canCreateNewCrystal = true;
    private bool m_canOpenCrystal = true;
    private List<CrystalAugment> m_crystalList = new List<CrystalAugment>();
    private Vector2 m_debugScrollview;

    public void ActivateAugment(AugmentDefinition augment, AugmentTierDefinition tier)
    {
        if (augment == null || tier == null)
        {
            Debug.LogWarning("Trying to set an invalid augment or tier", this);
            return;
        }

        if (m_augmentsMap.TryGetValue(augment, out var logic))
        {
            logic.Activate(tier);
        }
    }

    public void DrawManagerDebugIMGUI()
    {
        if (m_lazyBoolmap == null)
        {
            m_lazyBoolmap = new Dictionary<string, bool>();
        }

        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            GUILayout.Label("Augments");

            GUILayout.HorizontalSlider(-1, 0, 0, GUILayout.Height(0.1f));
            string keyShowTierOnly = "Tiers";
            if (!m_lazyBoolmap.ContainsKey(keyShowTierOnly))
            {
                m_lazyBoolmap[keyShowTierOnly] = false;
            }
            if ((m_lazyBoolmap[keyShowTierOnly] = GUILayout.Toggle(m_lazyBoolmap[keyShowTierOnly], keyShowTierOnly, GUI.skin.button)))
            {
                using (new GUILayout.VerticalScope(GUI.skin.box))
                {
                    m_tierLevelDownToDeactivate = GUILayout.Toggle(m_tierLevelDownToDeactivate, "Level Down On Augment End");

                    GUILayout.Label("Activate All Augments of tier:");
                    foreach (var tier in m_augmentTiersMap)
                    {
                        if (GUILayout.Button(tier.Key.name))
                        {
                            foreach (var item in tier.Value)
                            {
                                if (!m_augmentsMap.TryGetValue(item, out var augment))
                                {
                                    continue;
                                }

                                foreach (var tierLogic in augment.TierLogic)
                                {
                                    if (tierLogic.TierDefinition == tier.Key)
                                    {
                                        // My brain hurt...
                                        ActivateAugment(item, tier.Key);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            GUILayout.HorizontalSlider(-1, 0, 0, GUILayout.Height(0.1f));
            string keyShowCheats = "Cheats";
            if (!m_lazyBoolmap.ContainsKey(keyShowCheats))
            {
                m_lazyBoolmap[keyShowCheats] = false;
            }
            if (m_lazyBoolmap[keyShowCheats] = GUILayout.Toggle(m_lazyBoolmap[keyShowCheats], keyShowCheats, GUI.skin.button))
            {
                if (GUILayout.Button("Spawn Random Crystal"))
                {
                    SpawnRandomAugment();
                }
                if (GUILayout.Button("Spawn Pity Crystal"))
                {
                    SpawnPityAugment();
                }
            }

            string keyShowAll = "All Augments";
            if (!m_lazyBoolmap.ContainsKey(keyShowAll))
            {
                m_lazyBoolmap[keyShowAll] = false;
            }

            GUILayout.HorizontalSlider(-1, 0, 0, GUILayout.Height(0.1f));
            if (!(m_lazyBoolmap[keyShowAll] = GUILayout.Toggle(m_lazyBoolmap[keyShowAll], keyShowAll, GUI.skin.button)))
            {
                return;
            }
            m_debugScrollview = GUILayout.BeginScrollView(m_debugScrollview);
            foreach (var augment in m_augmentsMap.Values)
            {
                var name = augment.Definition.name;
                if (!m_lazyBoolmap.ContainsKey(name))
                {
                    m_lazyBoolmap.Add(name, false);
                }

                m_lazyBoolmap[name] = GUILayout.Toggle(m_lazyBoolmap[name], augment.Definition.name);
                if (!m_lazyBoolmap[name])
                {
                    continue;
                }

                using (new GUILayout.VerticalScope(GUI.skin.window))
                {
                    string status = augment.ActiveTier != null ? augment.ActiveTier.name : "Disabled";
                    GUILayout.Label($"Description: {augment.Definition.Description}");
                    GUILayout.Label($"Status: {status}");
                    GUILayout.Label($"Force Augment Tier:");
                    using (new GUILayout.HorizontalScope())
                    {
                        foreach (var tier in augment.AvailableTier)
                        {
                            if (GUILayout.Button(tier.TierDefinition.name))
                            {
                                ActivateAugment(augment.Definition, tier.TierDefinition);
                                break;
                            }
                        }
                    }
                }
            }
            GUILayout.EndScrollView();
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

    // Finaly the gacha part
    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void OpenAllCrystal()
    {
        if (!m_canOpenCrystal || m_crystalList.Count == 0)
        {
            return;
        }

        m_canOpenCrystal = false;
        m_remaingCrystalToOpen = m_crystalList.Count;
        if (m_remaingCrystalToOpen == 10)
        {
            m_onPityCrystalObtained?.Invoke();
            SpawnPityAugment();
        }

        for (int i = m_crystalList.Count - 1, c = 0; i >= 0; --i, ++c)
        {
            StartCoroutine(OpenCrystalDelayRoutine(m_crystalList[i], c));
            m_crystalList.RemoveAt(i);
        }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void SpawnPityAugment()
    {
        var augment = GetRandomAugment(m_pityAugmentTier);

        if (m_pityAugmentTier.CrystalPrefab)
        {
            var crystal = Instantiate(m_pityAugmentTier.CrystalPrefab) as CrystalAugment;
            crystal.SetData(augment, m_pityAugmentTier);
            var animator = crystal.GetComponent<SplineAnimate>();
            animator.Container = m_crystalSpline;
            animator.StartOffset = 1 / (m_crystalList.Count + 1);
            animator.Play();
            m_crystalList.Add(crystal);
        }
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    public void SpawnRandomAugment()
    {
        if (!m_canCreateNewCrystal)
        {
            return;
        }

        var tier = m_augmentTierCollection.GetRandomBonusTier();
        var augment = GetRandomAugment(tier);

        if (tier.CrystalPrefab)
        {
            var crystal = Instantiate(tier.CrystalPrefab) as CrystalAugment;
            crystal.SetData(augment, tier);
            var animator = crystal.GetComponent<SplineAnimate>();
            animator.Container = m_crystalSpline;
            animator.StartOffset = 1 / (m_crystalList.Count + 1);
            animator.Play();
            m_crystalList.Add(crystal);
        }

        if (m_crystalList.Count == 10)
        {
            m_canCreateNewCrystal = false;
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

    protected override void OnSingletonAwake()
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
                if (behaviour.Definition != augment)
                {
                    continue;
                }

                m_augmentsMap.Add(augment, behaviour);
                behaviour.Initialize(this);
                break;
            }
        }
    }

    private void OnEnable()
    {
        m_attractor.OnLevelUp += M_attractor_OnLevelUp;
    }

    private void OnDisable()
    {
        m_attractor.OnLevelUp -= M_attractor_OnLevelUp;
    }

    private void M_attractor_OnLevelUp()
    {
        SpawnRandomAugment();

        if (!m_canCreateNewCrystal)
        {
            m_attractor.DisableAbsorption();
        }
    }

    private IEnumerator OpenCrystalDelayRoutine(CrystalAugment crystal, int indexForDelay)
    {
        yield return new WaitForSeconds(m_offsetDurationBetweenOpening + m_crystalOpeningDuration * indexForDelay);
        crystal.Open(m_crystalSplineOpeningPath, m_crystalOpeningDuration);

        yield return new WaitForSeconds(m_crystalOpeningDuration);
        // All of that just for that line :D
        ActivateAugment(crystal.Augment, crystal.Tier);

        --m_remaingCrystalToOpen;
        if (m_remaingCrystalToOpen == 0)
        {
            m_canCreateNewCrystal = true;
            m_canOpenCrystal = true;
            m_attractor.EnableAttraction();
        }
    }

    [System.Serializable]
    public class Augment
    {
        [SerializeField] private string m_name;
        public AugmentDefinition Definition;
        public UnityEvent OnAnyTierActivated;
        public UnityEvent OnAnyTierDeactivated;
        public AugmentBehaviour[] TierLogic;
        private Dictionary<AugmentTierDefinition, AugmentBehaviour> m_augmentBehavioursMap;
        private MonoBehaviour m_owner;

        private float m_remainingProgress = 0;

        public event Action OnAugmentDeactivated;

        public event Action<AugmentTierDefinition> OnAugmentTierChanged;

        public event Action<float> OnAugmentUpdate;

        public AugmentTierDefinition ActiveTier { get; protected set; }
        public IReadOnlyList<AugmentBehaviour> AvailableTier => TierLogic;
        public bool IsActive { get; protected set; }

        public void Activate(AugmentTierDefinition tier, bool forceTier = false)
        {
            var previousTier = ActiveTier;
            if (IsActive)
            {
                Deactivate();

                // No need to go further if we don't have a tier for this augment.
                if (!m_augmentBehavioursMap.ContainsKey(tier))
                {
                    OnAugmentDeactivated?.Invoke();
                    return;
                }
            }
            else
            {
                // Was not actived yet, spawn UI
                SimulacraUIManager.Instance.SpawnAugmentUI(Definition, tier);
            }

            // No need to go further if we don't have a tier for this augment.
            if (!m_augmentBehavioursMap.ContainsKey(tier))
            {
                Debug.LogWarning($"Augment({Definition.name}) doesn't have tier {tier.name} available");
                return;
            }

            // Only change tier if the new tier has higher priority
            if (previousTier == null || forceTier || tier.OverridePriority >= previousTier.OverridePriority)
            {
                ActiveTier = tier;
                OnAugmentTierChanged?.Invoke(tier);
            }
            else
            {
                ActiveTier = previousTier;
            }

            OnAnyTierActivated?.Invoke();
            m_augmentBehavioursMap[tier]?.OnTierActivated?.Invoke();
            IsActive = true;

            // In case a crystal has been open, spawn the correct text.
            if (!forceTier)
            {
                SimulacraUIManager.Instance.SpawnAugmentTextUI(Definition, tier);
            }


            if (m_remainingProgress <= 0)
            {
                m_owner.StartCoroutine(CountDown());
            }
            else
            {
                m_remainingProgress = 1;
            }
        }

        public IEnumerator CountDown()
        {
            m_remainingProgress = 1;
            while (m_remainingProgress > 0)
            {
                m_remainingProgress -= Time.deltaTime / Definition.Duration;
                OnAugmentUpdate?.Invoke(m_remainingProgress);
                // Trigger the event with the remaining time as parameter
                yield return null;
            }

            if (Instance.m_tierLevelDownToDeactivate && ActiveTier.LevelDownTier != null)
            {
                Activate(ActiveTier.LevelDownTier, true);
                yield break;
            }

            OnAugmentDeactivated?.Invoke();
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

        public void Initialize(MonoBehaviour owner)
        {
            m_owner = owner;

            m_augmentBehavioursMap = new Dictionary<AugmentTierDefinition, AugmentBehaviour>(Definition.Tiers.Length);
            foreach (var tier in Definition.Tiers)
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
    }

    [System.Serializable]
    public class AugmentBehaviour
    {
        [SerializeField] private AugmentTierDefinition m_tierDefinition;
        [SerializeField] private UnityEvent m_onTierActivated;
        [SerializeField] private UnityEvent m_onTierDeactivated;
        public UnityEvent OnTierActivated => m_onTierActivated;
        public UnityEvent OnTierDeactivated => m_onTierDeactivated;
        public AugmentTierDefinition TierDefinition => m_tierDefinition;
    }
}
