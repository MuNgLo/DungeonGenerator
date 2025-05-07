using Munglo.Commons;
using System.Collections.Generic;
using Godot;
using Munglo.AI.Debug;

namespace Munglo.AI.Base
{
    /// <summary>
    /// All Actions need to inherit from this and implement the IAction Interface.
    /// </summary>
    [GlobalClass]
    public partial class ActionBaseClass : Node
    {
        [Export] private protected bool debug = false;
        [Export] private protected int minPriority = 25;
        [Export] private protected int startPriority = 25;
        [Export] private protected int maxPriority = 50;
        public string message = "no custom message";
        #region Private Fields
        private AIUnit unit;
        private Dictionary<string, ModifierMultiplier> multipers;
        private Dictionary<string, ModifierValue> extraValues;
        private EvaluateDelegate evaluateDelegate;
        #endregion
        #region Properties
        public virtual bool CheckIfPossible { get=>false; }
        public bool IsSelected { get => CheckIfSelected(); }
        public bool Possible { get => CheckIfPossible; }
        public int Priority { get { if (Possible) { return evaluateDelegate(true); } else { PutNonPossibleMessageOnStack(true); return -1; } } }
        public int PriorityNoStack { get { if (Possible) { return evaluateDelegate(false); } else { return -1; } } }
        public AIUnit Unit { get => unit; private set => unit = value; }
        public Dictionary<string, ModifierMultiplier> Multipers { get => multipers; }
        public Dictionary<string, ModifierValue> ExtraValues { get => extraValues; }

        #endregion
        #region public Methods
        public void PutEvaluationOnDebugStack(bool buildStack)
        {
            if (AIManager.Selection.SelectedUnit == null || !buildStack) { return; }
            if (AIManager.Selection.SelectedUnit.aiObjectID != Unit.aiObjectID) { return; }
            ActionDebugInfoStruct info = ActionDebug(this.GetType().Name);
            info.message = message;
            // Construct Influence table
            int size = ExtraValues.Keys.Count + Multipers.Keys.Count;
            List<ActionDebugInfluenceStruct> actionDebugInfluences = new();
            foreach (string key in ExtraValues.Keys)
            {
                actionDebugInfluences.Add(new ActionDebugInfluenceStruct()
                {
                    source = key,
                    multiplier = 0,
                    value = ExtraValues[key]()
                });
            }
            foreach (string key in Multipers.Keys)
            {
                if (!actionDebugInfluences.Exists(p => p.source == key))
                {
                    actionDebugInfluences.Add(new ActionDebugInfluenceStruct()
                    {
                        source = key,
                        multiplier = 0,
                        value = ExtraValues[key]()
                    });
                }
                else
                {
                    ActionDebugInfluenceStruct entry = actionDebugInfluences.Find(p => p.source == key);
                    entry.multiplier = Multipers[key]();
                    actionDebugInfluences.RemoveAll(p => p.source == key);
                    actionDebugInfluences.Add(entry);
                }
            }
            info.influences = actionDebugInfluences.ToArray();
            AIDebugSignals.Singleton.RaiseSignal(info);
        }
        public void Log(string msg)
        {
            //AIDebugSignals.Log(new AILogMessage(msg, unit.aiObjectID, this));
            GD.Print(msg);
        }
        public virtual void RegisterType(string typeName, string source)
        {
            throw new System.NotImplementedException();
        }
        public virtual void ActionInitialize(AIObject unit, EvaluateDelegate eDel)
        {
            evaluateDelegate = eDel;
            Unit = unit as AIUnit;
            multipers = new Dictionary<string, ModifierMultiplier>();
            extraValues = new Dictionary<string, ModifierValue>();
        }
        public void RegisterMultiplier(string source, ModifierMultiplier del)
        {
            multipers[source] = del;
        }
        public void RegisterExtraValue(string source, ModifierValue del)
        {
            extraValues[source] = del;
        }
        /// <summary>
        /// Returns the basePriority modified from registered modifiers without clamping it between min and max.
        /// </summary>
        /// <returns></returns>
        public int PriorityWithModifiers()
        {
            return startPriority + AddMultipliers() + AddExtraValues();
        }
        #endregion
        #region private Methods
        private void PutNonPossibleMessageOnStack(bool putOnStack = false)
        {
            if (AIManager.Selection.SelectedUnit == null || !putOnStack) { return; }
            if (AIManager.Selection.SelectedUnit.aiObjectID != Unit.aiObjectID) { return; }
            AIDebugSignals.Singleton.RaiseSignal(
                new ActionNonPossibleDebugInfoStruct()
                { name = this.GetType().Name, message = message }
                );
        }
        private ActionDebugInfoStruct ActionDebug(string actionName, int internalModifierValue = 0)
        {
            return new ActionDebugInfoStruct()
            {
                name = actionName,
                priorityFull = PriorityWithModifiers(),
                startPriority = startPriority,
                minPriority = minPriority,
                maxPriority = maxPriority,
                modInternal = internalModifierValue,
                modMultiplierPriority = AddMultipliers(),
                modValuePriority = AddExtraValues()
            };
        }
        /// <summary>
        /// This adds up all multipliers. Based on basePriority.
        /// </summary>
        /// <returns></returns>
        private int AddMultipliers()
        {
            float result = 0;
            foreach (ModifierMultiplier multiplier in multipers.Values)
            {
                result += minPriority * multiplier();
            }
            return Mathf.FloorToInt(result);
        }
        /// <summary>
        /// This adds up all extravalues and returns the sum.
        /// </summary>
        /// <returns></returns>
        private int AddExtraValues()
        {
            float result = 0;
            foreach (ModifierValue extravalue in extraValues.Values)
            {
                result += extravalue();
            }
            return Mathf.FloorToInt(result);
        }
        private bool CheckIfSelected()
        {
            if (AIManager.Selection.SelectedUnit == null) { return false; }
            if (AIManager.Selection.SelectedUnit.aiObjectID != Unit.aiObjectID) { return false; }
            return true;
        }
        #endregion
    }//EOF CLASS
}