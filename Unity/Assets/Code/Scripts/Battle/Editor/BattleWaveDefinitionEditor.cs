using NobunAtelier;
using NobunAtelier.Editor;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

[CustomEditor(typeof(BattleWaveDefinition))]
public class BattleWaveDefinitionEditor : NestedDataDefinitionEditor<BattleWaveDefinition>
{
    public override IReadOnlyList<DataDefinition> TargetDefinitions => m_dataDefinitions;
    private List<DataDefinition> m_dataDefinitions;
    private BattleWaveDefinition m_battleWaveDef;

    protected override void OnEnable()
    {
        if (target == null)
        {
            return;
        }

        m_battleWaveDef = (BattleWaveDefinition)target;
        m_dataDefinitions = new List<DataDefinition>();
        base.OnEnable();
    }

    protected override bool IsDatasetDirty()
    {
        return (m_battleWaveDef.WavesSequence.Count != m_dataDefinitions.Count) || base.IsDatasetDirty();
    }

    protected override void RefreshDataset()
    {
        if (m_battleWaveDef == null || m_battleWaveDef.WavesSequence == null) return;

        m_dataDefinitions.Clear();
        foreach (var sequence in m_battleWaveDef.WavesSequence)
        {
            var spline = sequence.Motion.Origin.SplineDefinition;
            if (!m_dataDefinitions.Contains(spline))
            {
                m_dataDefinitions.Add(spline);
            }
            spline = sequence.Motion.Destination.SplineDefinition;
            if (!m_dataDefinitions.Contains(spline))
            {
                m_dataDefinitions.Add(spline);
            }
        }

        base.RefreshDataset();
    }
}
