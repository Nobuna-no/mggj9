using System;
using UnityEngine;

public class GameBlackboard : Singleton<GameBlackboard>
{
    [Header("Game Blackboard")]
    [SerializeField] private float m_defaultMovementSpeedMultiplier = 1;
    [SerializeField] private float m_defaultFireRateMultiplier = 1;

    [SerializeField] private ShardsAttractionDefinition m_defaultAttractionDefinition;
    [SerializeField] private Transform m_playerTransform;

    public static Transform PlayerTransform => Instance.m_playerTransform;
    public static BlackboardValue<float> MovementSpeedMultiplier { get; } = new BlackboardValue<float>();
    public static BlackboardValue<float> FireRateMultiplier { get; } = new BlackboardValue<float>();
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
                OnValueChanged?.Invoke(m_value);
                m_value = value;
            }
        }

        public event Action<T> OnValueChanged;
    }
}