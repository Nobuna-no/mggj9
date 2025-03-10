using NobunAtelier;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldPerspectiveManager : SingletonMonoBehaviour<WorldPerspectiveManager>
{
    [SerializeField] private PerspectiveSettings[] m_settings;
    [SerializeField] private WorldBoundariesDefinition m_initialPerspective;

    private Dictionary<WorldBoundariesDefinition, PerspectiveSettings> m_perspectivesMap = null;
    private PerspectiveSettings m_activeSettings = null;
    private WorldBoundariesDefinition m_activeDefinition = null;

    public WorldBoundariesDefinition ActiveBoundaries => m_activeDefinition;

    public event Action<WorldBoundariesDefinition> OnWorldPerspectiveChanged;

    public void SetPerspective(WorldBoundariesDefinition perspective)
    {
        // If already in this perspective, exit.
        if (perspective == m_activeDefinition)
        {
            return;
        }

        OnWorldPerspectiveChanged?.Invoke(perspective);

        if (m_activeSettings != null)
        {
            m_activeSettings.OnPerspectiveExited?.Invoke(m_activeDefinition);
            m_activeSettings = null;
        }

        if (m_perspectivesMap.TryGetValue(perspective, out m_activeSettings))
        {
            m_activeSettings = m_perspectivesMap[perspective];
            m_activeSettings.OnPerspectiveActivated?.Invoke(perspective);
        }

        m_activeDefinition = perspective;
    }

    public bool TryGetPerspectiveSettings(WorldBoundariesDefinition key, out PerspectiveSettings out_settings)
    {
        if (m_perspectivesMap.TryGetValue(key, out out_settings))
        {
            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    [SerializeField] private WorldBoundariesDefinition m_debugPerspective;

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void DebugSetPerspective()
    {
        SetPerspective(m_debugPerspective);
    }

#endif

    protected override void OnSingletonAwake()
    {
        m_perspectivesMap = new Dictionary<WorldBoundariesDefinition, PerspectiveSettings>();
        foreach (var p in m_settings)
        {
            m_perspectivesMap.Add(p.Definition, p);
        }

        Debug.Assert(m_perspectivesMap.ContainsKey(m_initialPerspective), $"{this.name}: m_perspectivesMap doesn't contain initial definition.");
        m_activeSettings = m_perspectivesMap[m_initialPerspective];
        m_activeDefinition = m_initialPerspective;
    }

    [System.Serializable]
    public class PerspectiveSettings
    {
        [SerializeField] private WorldBoundariesDefinition m_definition;
        [SerializeField] private UnityEvent<WorldBoundariesDefinition> m_onPerspectiveEntered;
        [SerializeField] private UnityEvent<WorldBoundariesDefinition> m_onPerspectiveExited;

        public WorldBoundariesDefinition Definition => m_definition;
        public UnityEvent<WorldBoundariesDefinition> OnPerspectiveActivated => m_onPerspectiveEntered;
        public UnityEvent<WorldBoundariesDefinition> OnPerspectiveExited => m_onPerspectiveExited;
    }
}