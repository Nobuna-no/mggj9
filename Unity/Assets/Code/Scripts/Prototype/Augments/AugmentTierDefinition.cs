using UnityEngine;
using NobunAtelier;

public class AugmentTierDefinition : DataDefinition
{
    [Range(0, 1)]
    [SerializeField] private float m_probability;
    [SerializeField] private PoolObjectDefinition m_crystalPrefab; // shiny crystal :D
    [SerializeField] private Sprite m_icon;

    public float Probability => m_probability;
    public PoolObjectDefinition CrystalPrefab => m_crystalPrefab;
    public Sprite Icon => m_icon;
}