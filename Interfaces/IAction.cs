using Godot;

namespace Munglo.Commons
{
    /// <summary>
    /// This interface is by which the AIMind controls, executes and interact with actions
    /// </summary>
    public interface IAction
    {
        #region Properties
        /// <summary>
        /// If not overriden the ActionBaseClass will make this return False. Resulting in the Action never being picked.
        /// </summary>
        public bool CheckIfPossible { get; }

        /// <summary>
        /// Returns the accurate priority as clamped by min and max values.
        /// Also will put debug info on debug stack if unit is selected.
        /// </summary>
        public int Priority { get; }
        /// <summary>
        /// Returns the accurate priority as clamped by min and max values.
        /// Does not put debug info on debug stack.
        /// </summary>
        public int PriorityNoStack { get; }
        /// <summary>
        /// Every action needs this to be implemented. Any time it runs an evaluation it starts with checking if it is even possible to 
        /// perform the action. If not this returns false and a full evaluation can be sidestepped.
        /// Set the baseclass field "message" string to keep track of what step failed.
        /// </summary>
        public bool Possible { get; }
        #endregion
        #region Registors 
        /// <summary>
        /// Allows outside sources to tweak the priority through the AIMind. Don't call these directly.
        /// Use the AIMind's events (OnCallModifiersToRegister, OnCallTypesToRegister) to trigger registration through the AIMind's 
        /// (RegisterActionModifierMultiplier, RegisterActionModifierValue, RegisterActionReleventTypes)
        /// Example: mind.RegisterActionModifierValue<AvoidAction>(AvoidValue, this.GetType().Name);
        /// </summary>
        /// <param name="source">AssemblyQualifiedName of the class that registers the Delegate</param>
        /// <param name="del">Delegate to call to get the current multiplier</param>
        public void RegisterMultiplier(string source, ModifierMultiplier del);
        /// <summary>
        /// Allows outside sources to tweak the priority through the AIMind. Don't call these directly.
        /// Use the AIMind's events (OnCallModifiersToRegister, OnCallTypesToRegister) to trigger registration through the AIMind's 
        /// (RegisterActionModifierMultiplier, RegisterActionModifierValue, RegisterActionReleventTypes)
        /// Example: mind.RegisterActionModifierValue<AvoidAction>(AvoidValue, this.GetType().Name);
        /// </summary>
        /// <param name="source">AssemblyQualifiedName of the class that registers the Delegate</param>
        /// <param name="del">Delegate to call to get the current multiplier</param>
        public void RegisterExtraValue(string source, ModifierValue del);
        /// <summary>
        /// Allows outside sources to tweak the priority through the AIMind. Don't call these directly.
        /// Use the AIMind's events (OnCallModifiersToRegister, OnCallTypesToRegister) to trigger registration through the AIMind's 
        /// (RegisterActionModifierMultiplier, RegisterActionModifierValue, RegisterActionReleventTypes)
        /// Example: mind.RegisterActionReleventTypes<EatFood>(typeof(Apple).AssemblyQualifiedName, this.GetType().Name);
        /// </summary>
        /// <param name="source">AssemblyQualifiedName of the class that registers the Delegate</param>
        /// <param name="del">Delegate to call to get the current multiplier</param>
        public void RegisterType(string typeName, string source);
        #endregion

        /// <summary>
        /// Returns the basePriority modified from registered modifiers
        /// </summary>
        /// <returns></returns>
        public int PriorityWithModifiers();
        /// <summary>
        /// The priority value returned should never be higher then max priority or lower then min.
        /// BuildStack true will put the debug info into the action debug stack.
        /// Implement this in your Action. Then use the BaseInitilize() to pass it as a delegate to the baseClass inside the Action's Initilize().
        /// </summary>
        /// <param name="buildStack">BuildStack true will put the debug info into the action debug stack</param>
        /// <returns>
        /// -1 when action isn't possible.
        /// </returns>
        public int Evaluate(bool buildStack = false);

        /// <summary>
        /// This is called right after the action is picked. Make sure it is setting everything up for the action.
        /// It should only be called once when the action is initiated.
        /// </summary>
        /// <param name="aiState">Current state of the AIMind</param>
        /// <param name="navAgent">NavAgent for the AIMind</param>
        /// <returns>What state the mind should be put in after the Action begun. Should almost always be PerformingAction</returns>
        public AISTATE Begin(AISTATE aiState, NavigationAgent3D navAgent);
        /// <summary>
        /// Each time the Mind gets updated while performing the action this will get called.
        /// Usually it will return same state but when something goes wrong it will return a reset.
        /// </summary>
        /// <param name="aiState">Current state of the AIMind</param>
        /// <param name="navAgent">NavAgent for AIMind</param>
        /// <returns>What state the mind should be put in after the Action update</returns>
        public AISTATE Continue(AISTATE aiState, NavigationAgent3D navAgent);
        /// <summary>
        /// Whenever a higher priority or outside source causes an interuption, this should be called.
        /// TODO remember it was interupted so we can continue it if we want to.
        /// </summary>
        /// <param name="aiState">Current state of the AIMind</param>
        /// <param name="navAgent">NavAgent for the AIMind</param>
        public void Interupt(AISTATE aiState, NavigationAgent3D navAgent);
        /// <summary>
        /// After an Action has been interupted and if it can be resumed, this would be called instead of a Begin().
        /// </summary>
        /// <returns>false when resume failed</returns>
        public bool Resume();
        /// <summary>
        /// Force stop of action without effecting anything else. Essentially stop anything started in this action, coroutines, timers and such.
        /// After calling this it should be safe to delete or pool the AIMind.
        /// </summary>
        public void ForceStop();
        /// <summary>
        /// Allows the action to set itself up properly.
        /// Remember to pass the AIMind to the base class with ActionInitialize(mind)
        /// </summary>
        /// <param name="mind"></param>
        public void Initialize(AIObject unit);
        /// <summary>
        /// Allows the Action to set itself up properly.
        /// Dont override or mess with. Instead use the Initialize in IAction
        /// </summary>
        /// <param name="mind"></param>
        public void ActionInitialize(AIObject unit, EvaluateDelegate eDel);
        /// <summary>
        /// Implemented in the ActionBaseClass. Outputs to the AI Log.
        /// </summary>
        /// <param name="msg"></param>
        public void Log(string msg);
    }// EOF INTERFACE
}