using NobunAtelier;
using NobunAtelier.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(BattleMotionDefinition))]
public class BattleMotionDefinitionEditor : NestedDataDefinitionEditor<BattleMotionDefinition>
{
    public override IReadOnlyList<DataDefinition> TargetDefinitions => m_dataDefinitions;
    private List<DataDefinition> m_dataDefinitions;
    private BattleMotionDefinition m_battleMotionDef;
    private BattleMotionDefinition.MotionSpace.SpaceType m_activeOriginType;
    private BattleMotionDefinition.MotionSpace.SpaceType m_activeDestinationType;

    protected override void OnEnable()
    {
        m_battleMotionDef = target as BattleMotionDefinition;

        base.OnEnable();
    }

    protected override bool IsDatasetDirty()
    {
        bool isDirty = m_battleMotionDef.Origin.Type != m_activeOriginType
            || m_battleMotionDef.Destination.Type != m_activeDestinationType;
        return isDirty || base.IsDatasetDirty();
    }

    protected override void RefreshDataset()
    {
        if (m_battleMotionDef == null)
        {
            return;
        }

        m_dataDefinitions = new List<DataDefinition>();

        if (m_battleMotionDef.Origin.Type == BattleMotionDefinition.MotionSpace.SpaceType.Spline)
        {
            m_dataDefinitions.Add(m_battleMotionDef.Origin.SplineDefinition);
        }
        if (m_battleMotionDef.Destination.Type == BattleMotionDefinition.MotionSpace.SpaceType.Spline
            && !m_dataDefinitions.Contains(m_battleMotionDef.Destination.SplineDefinition))
        {
            m_dataDefinitions.Add(m_battleMotionDef.Destination.SplineDefinition);
        }

        m_activeOriginType = m_battleMotionDef.Origin.Type;
        m_activeDestinationType = m_battleMotionDef.Destination.Type;

        base.RefreshDataset();
    }
}
