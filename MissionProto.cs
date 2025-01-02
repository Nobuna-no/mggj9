// Personal goal: I want a data-driven system that can generate missions at runtime.
namespace MissionPrototype
{
    /// <summary>
    /// Constant data defined in the editor that represent a specific type of data.
    /// Cool thing is that you can then go wild with custom editor and even serialize these data
    /// in another scriptableObject (I like to call them DataCollection).
    /// </summary>
    public abstract class DataDefinition : ScriptableObject
    {
        public string m_name;
        public string m_description;

        public string Name => m_name;
        public string Description => m_description;
    }

    /// <summary>
    /// Encapsulates all there is to know about a battler.
    /// </summary>
    public class BattlerDefinition : DataDefinition
    {
        // AssetReference for prefab to spawn;
        // xpCurvePerLevel;
        // lootTable;
        // ...
    }

    /// <summary>
    /// We could pop some procedural generation rules in here for instance...
    /// </summary>
    public class LocationDefinition : DataDefinition { }

    /// <summary>
    /// Encapsulates all there is to know about a loot.
    /// Not sure if this should be constant as it smth that might be interesting to gen at runtime.
    /// Maybe creating a LootData that would be runtime and this class would have an array of LootData.
    /// This way, it is possible to craft mediculous loot table in the editor and then generate a simpler loot at runtime too.
    /// </summary>
    public class LootDefinition : DataDefinition
    {
        [SerializeField]
        private LootData m_lootData;

        [Serializable]
        public class LootData
        {
            // xp
            // gold
            // items
        }
    }

    public class MissionDefinition : DataDefinition
    {
        [SerializeField]
        private MissionData m_missionData;

        public MissionData Data => m_missionData;

        /// <summary>
        /// Encapsulates all there is to know about a mission.
        /// Like the LootData described above, this could be the runtime class and we could also
        /// defined a MissionDefinition that would be the constant serialized version.
        /// </summary>
        [System.Serializable]
        public class MissionData
        {
            public string name;
            public string description;

            [Min(1)]
            public int targetCount = 1;

            public MissionTaskType task;
            public DataDefinition target;
            public LootDefinition loot;

            public event Action<MissionData, LootDefinition> onCompleted;

            public void Complete()
            {
                onCompleted?.Invoke(this, loot);
            }

            /// <summary>
            /// Need to write the code so the mission is listening to dependency onCompleted event.
            /// This is to be able to automatically register a new mission when other specified missions are completed.
            /// Or should this be handle by the MissionManager?
            /// - i.e. MissionManager.RegisterMissionWithDependency(Mission mission, Mission[] dependencies)
            /// - this way, the MissionManager would be able to handle the dependency system.
            /// </summary>
            // public Mission[] dependencies;
        }
    }

    /// <summary>
    /// Each mission task will implement it's own MissionTaskChecker.
    /// </summary>
    public enum MissionTaskType
    {
        KillBattler,
        CollectItem,
        ReachDestination,
        CraftItem,
        // TalkToNPC,
        // CompleteMission
    }

    /// <summary>
    /// Implements the mission task logic.
    /// </summary>
    public abstract class MissionTaskChecker
    {
        private MissionDefinition.MissionData m_mission;
        public MissionDefinition.MissionData Mission { get; }

        public MissionTaskChecker(MissionDefinition.MissionData mission)
        {
            m_mission = mission;
        }

        public abstract bool CheckCondition(MissionConditionData data);
    }

    /// <summary>
    /// Super generic way of handling target based mission.
    /// </summary>
    public class MissionTargetCountTaskChecker : MissionTaskChecker
    {
        public DataDefinition Target { get; private set; }
        public int ConditionCount { get; private set; }

        public MissionTargetCountTaskChecker(MissionDefinition.MissionData mission)
            : base(mission)
        {
            Target = mission.target;
            if (Target == null)
            {
                Debug.LogError($"MissionTargetCountTaskChecker: target of mission '{mission.name}' is null!");
            }
            ConditionCount = mission.targetCount;
        }

        public override bool CheckCondition(MissionConditionData data)
        {
            if (data == null)
            {
                return false;
            }

            MissionTargetCountConditionData conditionData = data as MissionTargetCountConditionData;
            if (conditionData == null || conditionData.Target != Target)
            {
                return false;
            }

            if (conditionData.BeforeCount >= 0)
            {
                return conditionData.BeforeCount < ConditionCount && ConditionCount <= conditionData.CurrentCount;
            }
            else
            {
                return ConditionCount == conditionData.CurrentCount;
            }
        }
    }

    /// <summary>
    /// Inspired by how collision are sending collision data,
    /// this lightweight class could be generated everytime an event is triggered.
    /// By doing so, the MissionManager would be able to check the condition on all mission
    /// with a common MissionTaskType registered. This allows to have one event raise to potentially
    /// update multiple mission! =2=)/
    /// </summary>
    public abstract class MissionConditionData
    {
        public abstract MissionTaskType TaskType { get; }
    }

    public class MissionTargetCountConditionData : MissionConditionData
    {
        public override MissionTaskType TaskType => MissionTaskType.KillBattler;
        public DataDefinition Target { get; private set; } // This way we could handle any mission that require a target...
        public int BeforeCount { get; private set; }
        public int CurrentCount { get; private set; }

        public MissionTargetCountConditionData(DataDefinition target, int beforeCount, int currentCount)
        {
            this.Target = target;
            this.BeforeCount = beforeCount;
            this.CurrentCount = currentCount;
        }
    }

    /// <summary>
    ///  Please don't use this trash version, it's just for the code to compile...
    /// </summary>
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour
        where T : MonoBehaviour
    {
        private static T m_instance;

        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindObjectOfType<T>();
                    if (m_instance == null)
                    {
                        Debug.LogError($"SingletonMonoBehaviour: Instance - No instance of {typeof(T)} found in the scene.");
                    }
                }
                return m_instance;
            }
        }
    }

    public class MissionManager : SingletonMonoBehaviour<MissionManager>
    {
        private Dictionary<MissionTaskType, List<MissionTaskChecker>> m_taskCheckerMap
            = new Dictionary<MissionTaskType, List<MissionTaskChecker>>();

        // you can do that, but I prefer to use a bootstrapper to handle managers...
        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            m_taskCheckerMap.Add(MissionTaskType.KillBattler, new List<MissionTaskChecker>());
            m_taskCheckerMap.Add(MissionTaskType.CollectItem, new List<MissionTaskChecker>());
            m_taskCheckerMap.Add(MissionTaskType.ReachDestination, new List<MissionTaskChecker>());
            m_taskCheckerMap.Add(MissionTaskType.CraftItem, new List<MissionTaskChecker>());
            // etc..
        }

        public void RegisterMission(MissionDefinition.MissionData mission)
        {
            if (mission == null)
            {
                Debug.LogWarning("MissionManager: RegisterMission - mission is null.");
                return;
            }

            if (!m_taskCheckerMap.ContainsKey(mission.task))
            {
                Debug.LogWarning($"MissionManager: RegisterMission - mission task type '{mission.task}' is not yet implemented.");
            }

            switch (mission.task)
            {
                case MissionTaskType.KillBattler:
                case MissionTaskType.CollectItem:
                case MissionTaskType.ReachDestination:
                case MissionTaskType.CraftItem:
                    m_taskCheckerMap[mission.task].Add(new MissionTargetCountTaskChecker(mission));
                    break;
            }
        }

        public void UpdateMission(MissionConditionData data)
        {
            if (data == null)
            {
                return;
            }

            if (!m_taskCheckerMap.ContainsKey(data.TaskType))
            {
                Debug.LogWarning("MissionManager: UpdateMissionCondition - TaskType not found: " + data.TaskType);
                return;
            }

            for (int i = 0, c = m_taskCheckerMap[data.TaskType].Count - 1; i >= 0; i--)
            {
                MissionTaskChecker checker = m_taskCheckerMap[data.TaskType][i];
                if (checker.CheckCondition(data))
                {
                    m_taskCheckerMap[data.TaskType].RemoveAt(i);
                    CompleteMission(checker.Mission);
                }
            }
        }

        public void CompleteMission(MissionDefinition.MissionData mission)
        {
            Debug.Log($"MissionManager: Mission '{mission.name}' completed: ");
            mission.Complete();

            // handle dependency...
        }
    }

    public class MissionTest : MonoBehaviour
    {
        public MissionDefinition mission;
        public BattlerDefinition goblinDefinition;
        public LocationDefinition gateDefinition;

        private MissionDefinition.MissionData m_runtimeMission_KillGoblin;
        private MissionDefinition.MissionData m_runtimeMission_ReachTheGate;

        private int m_targetA = 0;
        private int m_targetB = 0;

        private void Start()
        {
            MissionManager.Instance.Initialize();
            MissionManager.Instance.RegisterMission(mission.Data);
            GenerateAndRegisterMission();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                m_targetA++;
                MissionConditionData data = new MissionTargetCountConditionData(mission.Data.target, m_targetA - 1, m_targetA);
                MissionManager.Instance.UpdateMission(data);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                // Emulate a goblin that just died.
                // A BattleStatsManager could handle the count death by battler and raise the following event...
                m_targetB++;
                MissionConditionData data = new MissionTargetCountConditionData(goblinDefinition, m_targetB - 1, m_targetB);
                MissionManager.Instance.UpdateMission(data);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                // Emulate a player running into a trigger that would raise the gate location reached...
                MissionConditionData data = new MissionTargetCountConditionData(gateDefinition, 0, 1);
                MissionManager.Instance.UpdateMission(data);
            }
        }

        private void GenerateAndRegisterMission()
        {
            m_runtimeMission_KillGoblin = new MissionDefinition.MissionData();
            m_runtimeMission_KillGoblin.name = "Kill 5 Goblin";
            m_runtimeMission_KillGoblin.description = "Lorem Ipsum";
            m_runtimeMission_KillGoblin.task = MissionTaskType.KillBattler;
            m_runtimeMission_KillGoblin.target = goblinDefinition;
            MissionManager.Instance.RegisterMission(m_runtimeMission_KillGoblin);

            m_runtimeMission_ReachTheGate = new MissionDefinition.MissionData();
            m_runtimeMission_ReachTheGate.name = "Reach the Gate";
            m_runtimeMission_ReachTheGate.description = "Lorem Ipsum";
            m_runtimeMission_ReachTheGate.task = MissionTaskType.ReachDestination;
            m_runtimeMission_ReachTheGate.target = gateDefinition;
            MissionManager.Instance.RegisterMission(m_runtimeMission_ReachTheGate);
        }
    }
}