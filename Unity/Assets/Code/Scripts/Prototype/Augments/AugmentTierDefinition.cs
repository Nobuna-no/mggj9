using UnityEngine;
using NobunAtelier;

public class AugmentTierDefinition : DataDefinition
{
    [SerializeField] private AugmentTierDefinition m_levelDownTier;
    [SerializeField, Range(0, 1)] private float m_probability;
    [SerializeField] private CrystalAugment m_crystalPrefab; // shiny crystal :D
    [SerializeField] private Sprite m_icon;
    [SerializeField, Tooltip("The higher means it can't be override")] private int m_overridePriority = 0;

    public int OverridePriority => m_overridePriority;
    public float Probability => m_probability;
    public CrystalAugment CrystalPrefab => m_crystalPrefab;
    public Sprite Icon => m_icon;
    public AugmentTierDefinition LevelDownTier => m_levelDownTier;
}