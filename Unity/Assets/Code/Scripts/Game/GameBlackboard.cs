using NobunAtelier;
using System;
using UnityEngine;

public class GameBlackboard : SingletonMonoBehaviour<GameBlackboard>
{
    public enum DebugType
    {
        None,
        Battle,
        Augment,

        Count
    }

    [Header("Game Blackboard")]
    [SerializeField] private float m_defaultFireRateMultiplier = 1;
    [SerializeField] private float m_defaultDamageMultiplier = 1;
    [SerializeField] private float m_defaultMovementSpeedMultiplier = 1;
    [SerializeField] private ShardsAttractionDefinition m_defaultAttractionDefinition;

    public DebugType ActiveDebugMenu = DebugType.None;
    public static BlackboardValue<float> FireRateMultiplier { get; } = new BlackboardValue<float>();
    public static BlackboardValue<float> DamageMultiplier { get; } = new BlackboardValue<float>();
    public static BlackboardValue<float> MovementSpeedMultiplier { get; } = new BlackboardValue<float>();
    public static BlackboardValue<ShardsAttractionDefinition> AttractionSettings { get; } = new BlackboardValue<ShardsAttractionDefinition>();

    public void SetMovementSpeedMultiplier(float value)
    {
        MovementSpeedMultiplier.Value = value;
    }

    public void ResetMovementSpeedMultiplier()
    {
        MovementSpeedMultiplier.Value = m_defaultMovementSpeedMultiplier;
    }

    public void SetFireRateMultiplier(float value)
    {
        FireRateMultiplier.Value = value;
    }

    public void ResetFireRateMultiplier()
    {
        FireRateMultiplier.Value = m_defaultFireRateMultiplier;
    }

    public void SetDamageMultiplier(float value)
    {
        DamageMultiplier.Value = value;
    }

    public void ResetDamageMultiplier()
    {
        DamageMultiplier.Value = m_defaultDamageMultiplier;
    }

    public void SetAttractionSettings(ShardsAttractionDefinition value)
    {
        AttractionSettings.Value = value;
    }

    public void ResetAttractionSettings()
    {
        AttractionSettings.Value = m_defaultAttractionDefinition;
    }

    protected override void OnSingletonAwake()
    {
        ResetAttractionSettings();
        ResetFireRateMultiplier();
        ResetDamageMultiplier();
        ResetMovementSpeedMultiplier();
    }

    public class BlackboardValue<T>
    {
        private T m_value;

        public T Value
        {
            get { return m_value; }
            set
            {
                OnValueChanged?.Invoke(value);
                m_value = value;
            }
        }

        public event Action<T> OnValueChanged;
    }

    public void SetDebugMode(int value)
    {
        var newValue = (DebugType)(int)Mathf.Repeat(value, (int)DebugType.Count);

        if (ActiveDebugMenu == newValue)
        {
            ActiveDebugMenu = DebugType.None;
        }
        else
        {
            ActiveDebugMenu = newValue;
        }
    }

    private void OnGUI()
    {
        switch (ActiveDebugMenu)
        {
            case DebugType.Augment:
                GUILayout.Label("Debug:(1) Battle, (2) Close");
                var ac = FindAnyObjectByType<AugmentController>();
                if (ac)
                {
                    ac.DrawManagerDebugIMGUI();
                }
                break;
            case DebugType.Battle:
                GUILayout.Label("Debug:(1) Close, (2)Augment");
                var bwm = FindAnyObjectByType<BattleWaveManager>();
                if (bwm)
                {
                    bwm.DrawManagerDebugIMGUI();
                }
                break;
            default:
                GUILayout.Label("Debug:(1) Battle, (2)Augment");
                break;
        }
    }
}