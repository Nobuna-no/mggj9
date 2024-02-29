using UnityEngine;
using NobunAtelier;

public class AugmentDefinition : DataDefinition
{
    [SerializeField] private string m_displayName;
    [SerializeField] private string m_description;
    // this assume every tier has the same duration, worth case, group with tiers
    [SerializeField] private float m_duration;
    [SerializeField] private AugmentTierDefinition[] m_availableTiers;
    [SerializeField] private Sprite m_icon;

    public string DisplayName => m_displayName;
    public string Description => m_description;
    public float Duration => m_duration;
    public AugmentTierDefinition[] Tiers => m_availableTiers;
    public Sprite Icon => m_icon;
}