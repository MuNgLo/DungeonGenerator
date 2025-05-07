using Munglo.AI.Base;
using Godot;

namespace Munglo.Commons
{
    /// <summary>
    /// This interface is by which the AIMind controls, executes and interact with actions
    /// </summary>
    public interface IAttack
    {
        /// <summary>
        /// Within this range the target can be attacked
        /// </summary>
        public float Range { get; }
        /// <summary>
        /// When this returns true attack/reload will be possible while moving
        /// </summary>
        public bool CanMove { get; }
        /// <summary>
        /// If not overriden the AttackBaseClass will make this return False.
        /// Resulting in the Attack never being in position to execute.
        /// </summary>
        public bool CheckIfInPosition { get; }
        /// <summary>
        /// Checks if there is a valid target and there is ammo, mana or whatever is needed.
        /// Should go before position check.
        /// Baseclass will look for visible valid and closest target. Use override to do something else.
        /// </summary>
        /// <returns></returns>
        public bool IsAttackPossible();
        /// <summary>
        /// Returns true if the attack can be executed from current position.
        /// Should go after IsAttackPossible check.
        /// BaseClass will return true if target is within range
        /// Override to do something else.
        /// </summary>
        /// <returns></returns>
        public bool IsInPosition();
        /// <summary>
        /// The current target for this attack.
        /// Can be null.
        /// </summary>
        public TargetParameters Target { get; }
        /// <summary>
        /// The code that runs on the State.General phase
        /// AttackBaseClass just moves to SelectingTaget phase.
        /// Overide this 
        /// </summary>
        /// <returns></returns>
        public ATTACKSTATE AttackGeneral();
        /// <summary>
        /// The code that runs on the State.SelectingTarget phase
        /// AttackBaseClass will select closes visible target and move to Setup phase.
        /// If it fails it falls back into Inactive and causes a AIState.Reset back to AIMind.
        /// Overide this for other functionality.
        /// </summary>
        /// <returns></returns>
        public ATTACKSTATE AttackSelectingTarget();
        /// <summary>
        /// The code that runs on the State.Setup phase
        /// AttackBaseClass will face target, ready weapon and move into range. If move is needed it goes to Move phase. Otherwise to Windup.
        /// Overide this for other functionality.
        /// </summary>
        /// <returns></returns>
        public ATTACKSTATE AttackSetup();
        /// <summary>
        /// The code that runs on the State.Windup phase
        /// AttackBaseClass wait for windup duration. then aim and fire weapon. Then go to Execusion phase.
        /// Overide this for other functionality.
        /// </summary>
        /// <returns></returns>
        public ATTACKSTATE AttackWindup();
        /// <summary>
        /// The code that runs on the State.Execusion phase
        /// AttackBaseClass wait for execusion duration. Then go to winddown phase.
        /// Overide this for other functionality.
        /// </summary>
        /// <returns></returns>
        public ATTACKSTATE AttackExecusion();
        /// <summary>
        /// The code that runs on the State.WindDown phase
        /// AttackBaseClass wait for winddown duration. Then go to Inactive phase.
        /// Overide this for other functionality.
        /// </summary>
        /// <returns></returns>
        public ATTACKSTATE AttackWindDown();
        /// <summary>
        /// Within the attacksetup phase attackmove might be called to move into range/position
        /// It will complete back to a setup phase or fall back to attackGeneral phase.
        /// </summary>
        /// <param name="navAgent"></param>
        /// <returns></returns>
        public ATTACKSTATE AttackMove(NavigationAgent3D navAgent);
        /// <summary>
        /// Intermediary init method between attack and actioninitilizor.
        /// Allows the attack to hook into awerness visibility events.
        /// Dont override or mess with. Instead use the Initialize in IAttack
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="eDel"></param>
        public void AttackInitialize(AIObject unit, EvaluateDelegate eDel);
        /// <summary>
        /// Allows the Attack to set itself up properly.
        /// Remember to pass the AIMind and Evaluate to the AttackBaseClass with AttackInitialize(mind, evaluate)
        /// </summary>
        /// <param name="mind"></param>
        public void Initialize(AIObject unit);
    }// EOF Interface
}