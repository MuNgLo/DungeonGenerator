using Munglo.GameEvents;
using System.Collections.Generic;
using System;
namespace Munglo.AI.Debug
{
    public class AIDebugSignals
    {
        //private List<ActionDebugInfo> aStack;
        //private List<ActionNonPossibleDebugInfo> mStack;
        private List<string> lastActions;
        public static AIDebugSignals Singleton;

        public EventHandler ClearDebugInfo;
        public EventHandler<AILogMessageStruct> OnAILog;
        public EventHandler<AICustomDataStruct> OnAICustomData;
        public EventHandler<ActionDebugInfoStruct> OnActionPossible;
        public EventHandler<ActionNonPossibleDebugInfoStruct> OnActionNotPossible;


        //public static List<ActionDebugInfo> ActionStack { get => Instance.aStack; }
        //public static List<ActionNonPossibleDebugInfo> NonActionStack { get => Instance.mStack; }
        public AIDebugSignals()
        {
            Singleton = this;
            if (Events.Units is not null)
            {
                Events.Units.OnUnitDeath += OnUnitDeath;
            }
            //ClearDebugInfo = new UnityEvent();
            //OnAILog = new UnityEvent<AILogMessage>();
            //OnAICustomData = new UnityEvent<AICustomData>();
            //OnActionPossible = new UnityEvent<ActionDebugInfo>();
            //OnActionNotPossible = new UnityEvent<ActionNonPossibleDebugInfo>();

            lastActions = new List<string>();
        }

        public void RaiseCustomDataEvent(AICustomDataStruct data)
        {
            OnAICustomData?.Invoke(Singleton, data);
        }

        public void Log(AILogMessageStruct msg)
        {
            OnAILog?.Invoke(Singleton, msg);
        }

        private void OnUnitDeath(Object sender, UnitDeathEventArguments arg0)
        {
            if (arg0.AIObject == AIManager.Selection.Selected) { AIManager.Selection.Select(null); }
        }

        public void RaiseSignal(ActionDebugInfoStruct incomming)
        {
            //Debug.Log($"AIDebugSignals::RaiseSignal({incomming.name})");
            OnActionPossible?.Invoke(Singleton, incomming);
        }
        public void RaiseSignal(ActionNonPossibleDebugInfoStruct incomming)
        {
            //Debug.Log($"AIDebugSignals::RaiseSignal({incomming.name})");
            OnActionNotPossible?.Invoke(Singleton, incomming);
        }

        public void ActionStarted(string actionName)
        {
            //Debug.Log("ActionDebugStack::ActionStarted()  " + actionName);
            Singleton.lastActions.Add(actionName);
        }
        public void ActionInterupted()
        {
            Singleton.lastActions[Singleton.lastActions.Count - 1] += ".int";
        }
        public void ActionResumed()
        {
            if (Singleton.lastActions.Count > 1)
            {
                Singleton.lastActions[Singleton.lastActions.Count - 2] += ".int";
            }
        }
        public string ActionLast()
        {
            if (Singleton.lastActions.Count > 1)
            {
                return Singleton.lastActions[Singleton.lastActions.Count - 2];
            }
            return string.Empty;
        }
        public string ActionCurrent()
        {
            if (Singleton.lastActions.Count > 0)
            {
                return Singleton.lastActions[Singleton.lastActions.Count - 1];
            }
            return string.Empty;
        }
    }// EOF CLASS
}