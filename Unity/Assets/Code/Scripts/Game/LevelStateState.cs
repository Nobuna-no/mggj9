using UnityEngine;
using NobunAtelier;

[AddComponentMenu("SIMULACRA/States/LevelState")]
public class LevelStateState : StateWithTransition<LevelStateDefinition, LevelStateCollection>
{
    [Header("Level Section")]
    [SerializeField] private LevelSection m_section;

    [System.Serializable]
    private class LevelSection
    {
        public enum Rule
        {
            None = 0,
            KillXEnemy,
            SurviveXSeconds,
            SurviveXTiles
        }

        [SerializeField] private WorldBoundariesDefinition m_perspective;
        [SerializeField] private Rule m_rule = Rule.None;
        [SerializeField] private BattleWaveCollection m_enemyWaves;
        [SerializeField] private PoolObjectDefinition m_tiles;
    }
}