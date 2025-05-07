using System;
using System.Collections.Generic;
using Munglo.AI.Base;
using Munglo.Commons;
using Godot;
using Munglo.AI.Debug;

namespace Munglo.AI
{
    /// <summary>
    /// Top manager class of all AI. This controls and balances updates of the AIGroups active in the game
    /// Tracking and balancing through the ManagedGroup objects
    /// </summary>
    public partial class AIManager : Node
    {
        public static AIManager Instance;
        private SelectedAIObject selection;
        private AIDebugSignals debugSignals;

        [Export] private bool debug = false;

        [Export] private int maxTimeIterationBreakTime = 200;
        //[Tooltip("Minimum time before an AISystem can be updated again")]
        [Export] private float minUPS = 0.1f;
        //[Tooltip("minimum time a Mind have to wait for next update")]
        [Export] private float minUpdateGap = 0.05f;
        [Export] private int maxGroupSize = 10;
        [Export(PropertyHint.Layers3DPhysics)]
        public uint blocksVision;
        //[Header("Managed Groups"), SerializeField]
        private List<ManagedGroup> groups;
        private List<QueuedUnitToRemove> removeUnitsQueue = new List<QueuedUnitToRemove>();
        private List<QueuedUnitToRemove> removeAndDestroyUnitsQueue = new List<QueuedUnitToRemove>();
        private List<int> removeAIObjectsQueue = new List<int>();
        private List<int> removeAndDestroyAIObjectsQueue = new List<int>();
        private int iterationIndex = 0;
        private int iterationCount = 0;

        private Factions factions;

        public static EventHandler<int> OnAIObjectRemoved;

        public int nbOfGroups { get { return groups.Count; } }
        private List<ManagedGroup> Groups { get { return groups; } }

        private float statTimer = 0.0f; // internal timestamp to reset values for stat counters


        private int lastIDUsed = -1;
        private static AIMatrix matrix;
        public static AIMatrix Matrix { get => matrix; }

        public static SelectedAIObject Selection { get => Instance != null ? Instance.selection : null; }
        public static AIObject Selected => Instance.selection != null ? Instance.selection.Selected : null;
        public static int MatrixSize { get { if (matrix == null) { return -1; } else { return matrix.Size; } } }

        public static int nbOfUnits { get => matrix.UnitsinMatrix(); }
        public static int nbOfObjects { get => matrix.ObjectsinMatrix(); }
        public static int IteratedMinds { get => Instance.iterationCount; }
        public static string IDsToRemove { get => string.Join(' ', Instance.removeAIObjectsQueue.ToArray()); }
        public static string IDsToRemoveAndDestroy { get => string.Join(' ', Instance.removeAndDestroyAIObjectsQueue.ToArray()); }

        public static bool Initiated { get => Instance != null; }

        #region debug stuff
        private System.Diagnostics.Stopwatch stopWatch;
        private long updateTime;
        public long UpdateTime { get => updateTime; }
        #endregion

        public static Factions Factions => Instance.factions;

        public override void _EnterTree()
        {
            if (Instance == null) { Instance = this; }
            selection = new SelectedAIObject();
            matrix = new AIMatrix();
            debugSignals = new AIDebugSignals();
            groups = new List<ManagedGroup>();
            stopWatch = new System.Diagnostics.Stopwatch();
        }
        public override void _Ready()
        {
            factions = GetNode<Factions>("Factions");
        }

        public override void _Process(double delta)
        {
            if (GetTree().GetMultiplayer().MultiplayerPeer == null) { return; }

            //if (GetTree().GetMultiplayer().MultiplayerPeer.GetConnectionStatus() != MultiplayerPeer.ConnectionStatus.Connected) { return; }


            if (!GetTree().GetMultiplayer().IsServer()) { return; }
            // Time the update
            stopWatch.Reset();
            stopWatch.Start();
            // StatTimer things
            statTimer += (float)delta;
            if (statTimer >= 1.0f)
            {
                statTimer = 0.0f;
                matrix.StatReset();
            }
            // Restarts the iteration from begining and clean out queued objects and minds
            if (iterationIndex >= groups.Count) { ResetIteration(); return; }
            // Iterate over the groups until we done or we run out of time
            // Also remember where we where if we stopped mid iteration last update
            for (int i = 0 + iterationIndex; i < groups.Count; i++)
            {
                // remember the for loop index to next Update
                iterationIndex = i + 1; // SHOULDNT we update the group first??
                // Udpate the group
                groups[i].UpdateGroup((float)delta, minUpdateGap, ref iterationCount);
                //iterationCount += groups[i].Count;
                // Check if Update has taken to long and break iteration if it has
                if (stopWatch.ElapsedMilliseconds > maxTimeIterationBreakTime)
                {
                    if (debug) { GD.PrintErr($"AIManager iteration went over time limit ({stopWatch.ElapsedMilliseconds}ms)"); }
                    break;
                }
            }
            // Time Update
            stopWatch.Stop();
            updateTime = stopWatch.ElapsedMilliseconds;
        }
        private void ResetIteration()
        {
            iterationIndex = 0;
            iterationCount = 0;
            RemoveQueuedAIObjects();
            RemoveQueuedUnits();
        }

        /// <summary>
        /// This registers an AIUnit into the smallest group.
        /// If no group exists it will create a new one.
        /// Returns the GroupdID 
        /// </summary>
        /// <param name="mind"></param>
        public int RegisterUnit(AIUnit unit)
        {
            GD.Print($"AIManager::RegisterUnit() Registered[{unit.Name}]");

            if (!Multiplayer.IsServer()) { return -1; }
            if (unit.aiObjectID != -1)
            {
                if (matrix.IsIDinMatrix(unit.aiObjectID))
                {
                    GD.PrintErr($"AIManager::RegisterMind({unit.Name}) unit have AIObjedtID of {unit.aiObjectID} and is already in the matrix! This should not happen!");
                }
                else
                {
                    GD.PrintErr($"AIManager::RegisterUnit({unit.Name}) unit have AIObjedtID of {unit.aiObjectID} and is not in the matrix! This should not happen!");
                }
            }
            removeUnitsQueue.RemoveAll(p => p.AIObjectID == unit.aiObjectID);
            RegisterAIObject(unit as AIObject, true);
            ManagedGroup group = GetSmallestUnSpecGroup();
            group.Add(unit);
            return group.GroupdID;
        }
        public void RegisterAIObject(AIObject aIObject, bool isUnit)
        {
            if (!Multiplayer.IsServer()) { return; }
            removeAIObjectsQueue.RemoveAll(p => p == aIObject.aiObjectID);
            aIObject.aiObjectID = lastIDUsed + 1;
            lastIDUsed = aIObject.aiObjectID;
            matrix.AddObject(new ObjectOfInterest() { aiObject = aIObject, aIObjectID = aIObject.aiObjectID, aIFactionID = aIObject.factionID, bounds = aIObject.bounds, aiType = aIObject.AIType });
            if (debug)
            {
                GD.Print($"AIManager::RegisterAIObject() Registered[{aIObject.Name}]");
                AIDebugSignals.Singleton.Log(
                    new AILogMessageStruct($"AIM::RegisterObject({aIObject.Name}, isUnit={isUnit})", aIObject.aiObjectID, aIObject));
            }
        }
        public void DeRegisterUnit(int groupID, AIUnit unit, bool alsoDestroy = false)
        {
            if (!Multiplayer.IsServer()) { return; }
            if (unit.aiObjectID == -1) { return; }
            if (alsoDestroy)
            {
                if (!removeAndDestroyUnitsQueue.Exists(p => p.GroupID == groupID && p.AIObjectID == unit.aiObjectID))
                {
                    removeAndDestroyUnitsQueue.Add(new QueuedUnitToRemove() { GroupID = groupID, AIObjectID = unit.aiObjectID });
                }
            }
            else
            {
                if (!removeUnitsQueue.Exists(p => p.GroupID == groupID && p.AIObjectID == unit.aiObjectID))
                {
                    removeUnitsQueue.Add(new QueuedUnitToRemove() { GroupID = groupID, AIObjectID = unit.aiObjectID });
                }
            }
            OnAIObjectRemoved?.Invoke(this, unit.aiObjectID);
        }
        public void DeRegisterAIObject(int aIObjectID, bool alsoDestroy = false)
        {
            if (!Multiplayer.IsServer()) { return; }
            if (debug)
            {
                AIDebugSignals.Singleton.Log(
                    new AILogMessageStruct($"AIM::DeRegisterAIObject({aIObjectID}, alsoDestroy={alsoDestroy})", aIObjectID, null));
            }
            if (!alsoDestroy)
            {
                if (!removeAIObjectsQueue.Exists(p => p == aIObjectID))
                {
                    removeAIObjectsQueue.Add(aIObjectID);
                }
            }
            else
            {
                if (!removeAndDestroyAIObjectsQueue.Exists(p => p == aIObjectID))
                {
                    removeAndDestroyAIObjectsQueue.Add(aIObjectID);
                }
            }
            OnAIObjectRemoved?.Invoke(this, aIObjectID);
        }
        private void RemoveQueuedAIObjects()
        {
            //Debug.Log($"AIManager::RemoveQueuedAIObjects()");
            foreach (int aID in removeAIObjectsQueue)
            {
                matrix.RemoveObject(aID);
            }
            removeAIObjectsQueue = new List<int>();

            foreach (int aID in removeAndDestroyAIObjectsQueue)
            {
                AIObject tr = matrix.GetAIObject(aID);
                if (tr is null)
                {
                    GD.PrintErr($"AIManager::RemoveQueuedAIObjects() get transform from Matrix returned NULL on AIObjectID({aID})");
                    return;
                }
                matrix.RemoveObject(aID);
                tr.QueueFree();
                tr.Body.QueueFree();
            }
            removeAndDestroyAIObjectsQueue = new List<int>();
        }
        private void RemoveQueuedUnits()
        {
            //Debug.Log($"AIManager::RemoveQueuedMinds()");
            foreach (QueuedUnitToRemove unit in removeUnitsQueue)
            {
                ManagedGroup group = Groups.Find(p => p.GroupdID == unit.GroupID);
                if (group == null) { GD.PrintErr($"AIManager::RemoveQueuedMinds({unit.GroupID}, {unit.AIObjectID}) deregister but group resolved to NULL"); }
                group.Remove(unit.AIObjectID);
                DeRegisterAIObject(unit.AIObjectID);
            }
            removeUnitsQueue = new List<QueuedUnitToRemove>();

            foreach (QueuedUnitToRemove unit in removeAndDestroyUnitsQueue)
            {
                ManagedGroup group = Groups.Find(p => p.GroupdID == unit.GroupID);
                if (group == null) { GD.PrintErr($"AIManager::RemoveQueuedMinds({unit.GroupID}, {unit.AIObjectID}) deregister but group resolved to NULL"); }
                AIObject tr = matrix.GetAIObject(unit.AIObjectID);
                group.Remove(unit.AIObjectID);
                DeRegisterAIObject(unit.AIObjectID);
                tr.Body.QueueFree();
            }
            removeAndDestroyUnitsQueue = new List<QueuedUnitToRemove>();


        }

        public bool IsInstancevalidNode(Node obj)
        {
            return IsInstanceValid(obj);
        }

        private ManagedGroup GetSmallestUnSpecGroup()
        {
            if (groups.Count < 1)
            {
                if (debug) { GD.Print($"AIManager::GetSmallestUnSpecGroup() no groups yet so making the first one."); }
                groups.Add(new ManagedGroup(AIGROUPSPEC.UNSPECIFIED, 0));
                return groups[0];
            }
            int picked = 1000;
            foreach (ManagedGroup mGroup in groups)
            {
                if (mGroup.Spec != AIGROUPSPEC.UNSPECIFIED) { continue; }
                if (mGroup.Count < maxGroupSize && picked > mGroup.Count) { picked = mGroup.GroupdID; continue; }
            }
            if (picked == 1000)
            {
                if (debug) { GD.Print($"AIManager::GetSmallestUnSpecGroup() All unspec groups full so making a new one."); }
                groups.Add(new ManagedGroup(AIGROUPSPEC.UNSPECIFIED, groups.Count));
                return groups[groups.Count - 1];
            }
            if (debug) { GD.Print($"AIManager::GetSmallestUnSpecGroup() picked={picked}"); }

            return GetMGroupByID(picked);
        }

        private ManagedGroup GetMGroupByID(int id)
        {
            if (debug) { GD.Print($"AIManager::GetMGroupByID() id={id}"); }
            return groups.Find(p => p.GroupdID == id);
        }
        [System.Serializable]
        private struct QueuedUnitToRemove
        {
            public int GroupID;
            public int AIObjectID;
        }
        public static void UpdateFaction(int aiObjectID, int newFID)
        {
            matrix.UpdateFaction(aiObjectID, newFID);
        }

        public static DSOURCE ResolveDsource(int ownerID)
        {
            if (matrix.IsIDinMatrix(ownerID))
            {
                return matrix.ResolveDamageSource(ownerID);
            }
            return DSOURCE.NONE;
        }
    }// EOF CLASS
}
