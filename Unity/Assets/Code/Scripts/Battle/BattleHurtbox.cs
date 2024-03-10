using NobunAtelier.Gameplay;
using UnityEngine;

public class BattleHurtbox : HitboxBehaviour
{
    [SerializeField] private bool m_isAffectedByAugment = false;

    private void Start()
    {
        if (m_isAffectedByAugment && GameBlackboard.IsSingletonValid)
        {
            GameBlackboard.DamageMultiplier.OnValueChanged += DamageMultiplier_OnValueChanged;
            m_damageMultiplier = GameBlackboard.FireRateMultiplier.Value;
        }
    }

    private void DamageMultiplier_OnValueChanged(float value)
    {
        m_damageMultiplier = value;
    }
}
