using UnityEngine;
using NobunAtelier;

public class ShardsAttractionDefinition : DataDefinition
{
	[SerializeField] private float m_radius = 1f;
	[SerializeField] private float m_durationMultiplier = 1f;

	public float Radius => m_radius;
	public float DurationMultiplier => m_durationMultiplier;
}