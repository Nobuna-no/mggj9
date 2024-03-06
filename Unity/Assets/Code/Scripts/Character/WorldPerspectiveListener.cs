using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldPerspectiveListener : MonoBehaviour
{
    [SerializeField]
    private PerspectiveEvent[] m_perspectiveEvents;
    private PerspectiveEvent m_activePerspective = null;
    private Dictionary<WorldBoundariesDefinition, PerspectiveEvent> m_perspectiveEventsMap =
        new Dictionary<WorldBoundariesDefinition, PerspectiveEvent>();

    void Start()
    {
        m_perspectiveEventsMap = new Dictionary<WorldBoundariesDefinition, PerspectiveEvent>(m_perspectiveEvents.Length);
        foreach (var perspectiveEvent in m_perspectiveEvents)
        {
            m_perspectiveEventsMap.Add(perspectiveEvent.Definition, perspectiveEvent);
        }

        Debug.Assert(WorldPerspectiveManager.IsSingletonValid);
        WorldPerspectiveManager.Instance.OnWorldPerspectiveChanged += SetWorldPerspective;
        SetWorldPerspective(WorldPerspectiveManager.Instance.ActiveBoundaries);
    }

    private void SetWorldPerspective(WorldBoundariesDefinition definition)
    {
        // Early out if:
        // - Component is disable.
        // - We don't have event for the new definition.
        // - Definition is already active.
        if (!enabled || !m_perspectiveEventsMap.ContainsKey(definition) || (m_activePerspective != null && m_activePerspective.Definition == definition))
        {
            return;
        }

        if (m_activePerspective != null)
        {
            m_activePerspective.OnPerspectiveExit?.Invoke();
        }

        m_activePerspective = m_perspectiveEventsMap[definition];
        m_activePerspective.OnPerspectiveEnter?.Invoke();
    }

    [System.Serializable]
    public class PerspectiveEvent
    {
        [SerializeField] private WorldBoundariesDefinition m_definition;
        [SerializeField] private UnityEvent m_onPerspectiveEnter;
        [SerializeField] private UnityEvent m_onPerspectiveExit;

        public WorldBoundariesDefinition Definition => m_definition;
        public UnityEvent OnPerspectiveEnter => m_onPerspectiveEnter;
        public UnityEvent OnPerspectiveExit => m_onPerspectiveExit;
    }
}
