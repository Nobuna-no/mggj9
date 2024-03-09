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
        if (m_battleWaveDef == null) return;

        m_dataDefinitions.Clear();
        foreach (var sequence in m_battleWaveDef.WavesSequence)
        {
            if (m_dataDefinitions.Contains(sequence.Motion))
            {
                continue;
            }

            m_dataDefinitions.Add(sequence.Motion);
        }

        base.RefreshDataset();
    }
}
