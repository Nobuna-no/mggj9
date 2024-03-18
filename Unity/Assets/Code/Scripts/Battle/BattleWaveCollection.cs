using UnityEngine;
using NobunAtelier;
using NaughtyAttributes;

[CreateAssetMenu(fileName ="[BattleWaves]", menuName = "SIMULACRA/Battle/Waves")]
public class BattleWaveCollection : DataCollection<BattleWaveDefinition>
{
#if UNITY_EDITOR
    [ContextMenu("CopyAllMotionToNewMotion")]
    [Button]
    private void CopyAllMotionToNewMotion()
    {
        foreach (var def in Definitions)
        {
            foreach(var sequence in def.WavesSequence)
            {
                sequence.CopyMotionData();
            }
        }
    }
#endif
}