using System;
using Godot;
using Munglo.AI.Base;
using Munglo.Commons;

namespace Munglo.AI
{
    public class AIMind
    {
        private AIUnit unit;

        public EventHandler<AIMind> OnCallModifiersToRegister;
        public EventHandler<AIMind> OnCallTypesToRegister;

        private NavigationAgent3D navAgent;

        public AIMind(AIUnit aiUnit, NavigationAgent3D agent)
        {
            // Setup references
            unit = aiUnit;
            navAgent = agent;
            if (navAgent is null) { GD.PrintErr("NavAgent reference : FAIL"); }
        }
       
        /// <summary>
        /// If the incomming priority is higher then the currently performing one we reset AI and return True.
        /// </summary>
        /// <param name="prio"></param>
        /// <returns></returns>
        public bool ChangeIfHigherPrio(int prio)
        {
            //Debug.Log($"AIMind::ChangeActionIfHigherPrio({action.GetType()}, {prio}) old prio = {unit.CurrentAction.Priority}", unit.gameObject);
            if(unit.CurrentAction is not null && unit.CurrentAction.PriorityNoStack < prio)
            {
                (unit.CurrentAction as IAction).ForceStop();
                // This is where we might wanna add this mind to a high priority queue on the AIManager
                //unit.TimeSinceLastUpdate += 500.0f; // make sure we dont skip next update pass DONT DO THIS
                unit.State = AISTATE.RESET;
                return true;
            }
            return false;
        }
        
        
        
        public void DebugForceMindRegistration()
        {
            //AIManager.Instance.RegisterUnit(unit);
        }

        public void UpdateMind(float delta)
        {
            unit.Awareness.UpdateAwareness();
            // Do a normal AI update
            switch (unit.State)
            {
                case AISTATE.INACTIVE:
                    // AI DISABLED !! DO NOTHING !!
                    break;
                case AISTATE.PERFORMINGACTION:
                    //if (BreakForTarget()) { break; }
                    unit.State = (unit.CurrentAction as IAction).Continue(unit.State, navAgent);
                    break;
                case AISTATE.ATTACKING:
                    //continue attack
                    unit.State = (unit.CurrentAction as IAction).Continue(unit.State, navAgent);
                    break;
                case AISTATE.RESET:
                    if (unit.CurrentAction is not null)
                    {
                        (unit.CurrentAction as IAction).ForceStop();
                        unit.CurrentAction = null;
                    }
                    //if(BreakForTarget()) { break; }
                    EvaluateAllOption();
                    break;
            }
            unit.TimeSinceLastUpdate = 0.0f;
            //GD.Print($"AIMind::UpdateMind() AIState[{unit.State}] MoveState[{unit.MovementState}]");
        }

        public void EquipWeapon(IWeapon newWeapon)
        {
            //GD.Print($"AIMind::EquipWeapon() newWeapon[{newWeapon.node3D.Name}]");
            if (unit.Weapon != null) { DropWeapon(); }
            unit.Weapon = newWeapon;
            unit.Weapon.AIControlWeapon(unit.weaponMountPoint, unit.aiObjectID);
        }
        public void DropWeapon()
        {
            unit.Weapon.DropWeapon();
            unit.Weapon = null;
        }

        public void PoolReset()
        {
            StopAI();
            unit.Awareness.PoolReset();
        }
        /// <summary>
        /// Goes through all actions and picks the one with highest priority and starts executing it
        /// </summary>
        private void EvaluateDefault()
        {
            if(unit.Actions == null) { GD.PrintErr("AI MIND WITH NO ACTIONS!!"); StopAI(); return; }
            unit.CurrentAction = unit.Actions[0];
            int pickedPrio = unit.Actions[0].Priority;
            int challenger = -1;
            for (int i = 1; i < unit.Actions.Length; i++)
            {
                challenger = unit.Actions[i].Priority;
                if (challenger > pickedPrio)
                {
                    pickedPrio = challenger;
                    challenger = -1;
                    unit.CurrentAction = unit.Actions[i];
                }
            }
            
            if (AIManager.Selection.Selected != null && AIManager.Selection.Selected.aiObjectID == unit.aiObjectID)
            {
                Debug.AIDebugSignals.Singleton.ActionStarted(unit.CurrentAction.GetType().Name);
            }
            //GD.Print($"AIMind::EvaluateDefault() currentAction[{unit.CurrentAction.Name}]");
            unit.State = (unit.CurrentAction as IAction).Begin(unit.State, navAgent);
            if(pickedPrio < 0) { GD.PrintErr("AIMind:: No possible actions can be picked!!"); }
        }
        /// <summary>
        /// Updates visible objects and checks if we have at least one target and at least one attack.
        /// If we do, get the best attack priority and feed it to ChangeIfHigherPrio().
        /// return what that returns.
        /// </summary>
        /// <returns></returns>
        /*private bool BreakForTarget()
        {
            if(unit.CurrentTarget == null) { return false; }
            if (unit.Awareness.NumberOfTargets() < 1)
            {
                if (unit.DebugConfig.MindDebug)
                {
                    //Debug.Log($"AIMind::BreakForTarget() unit.Awareness.NoTarget is True", unit.gameObject);
                }
                return false;
            }
            //return ChangeIfHigherPrio(subAttacking.BestAttackPrio());
            // TODO FIX IT FIX IT FIX IT FIX IT FIX IT FIX IT FIX IT FIX IT 
            return false;
        }*/

        /// <summary>
        /// This decides what to do with all options on the table.
        /// Usually we want to fall back into Reset state after a completed action.
        /// </summary>
        private void EvaluateAllOption()
        {
            EvaluateDefault();
        }

   
        /// <summary>
        /// This is tied to the coroutine for spawning in to allow spawn animation to play out and have spawnprotection
        /// </summary>
        public void StartAI()
        {
            unit.State = AISTATE.RESET;
        }
        public void StopAI()
        {
            unit.Movement.Stop();
            unit.State = AISTATE.INACTIVE;
        }
        /// <summary>
        /// Rotates in navagent speed to face worldpoint
        /// returns true when it does
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public bool TurnToPoint(Vector3 worldPoint, float precision)
        {
            //if(!navAgent.isStopped){ navAgent.isStopped=true;}

            worldPoint = worldPoint.Project(unit.Body.Transform.Basis.Y);
            Vector3 relPos = unit.GlobalPosition.Project(unit.Body.Transform.Basis.Y);
            Vector3 relDir = worldPoint - relPos;

            if(-unit.Body.Transform.Basis.Z.AngleTo(relDir) * 180.0f/MathF.PI < precision){
                return true;
            }
            /*
            unit.transform.rotation = Quaternion.RotateTowards(
                unit.transform.rotation,

                Quaternion.LookRotation(relDir, unit.transform.rotation * unit.transform.up),

                (navAgent.angularSpeed * Time.deltaTime)
                );
            */
            unit.Body.Rotate(unit.Body.Transform.Basis.Y, 30.0f);


            return false;
        }
        /// <summary>
        /// Register delegate for action priority multiplier
        /// </summary>
        /// <typeparam name="T">action Class</typeparam>
        /// <param name="del">delegate</param>
        /// <param name="source">usually gameobject name</param>
        public void RegisterActionModifierMultiplier<T>(ModifierMultiplier del, string source)
        {
            foreach (ActionBaseClass action in unit.Actions)
            {
                if (typeof(T) == action.GetType())
                {
                    action.RegisterMultiplier(source, del);
                }
            }
        }
        /// <summary>
        /// Register delegates for action priority value change through this one
        /// </summary>
        /// <typeparam name="T">action Class</typeparam>
        /// <param name="del">delegate</param>
        /// <param name="source">usually gameobject name</param>
        public void RegisterActionModifierValue<T>(ModifierValue del, string source)
        {
            foreach (ActionBaseClass action in unit.Actions)
            {
                if (typeof(T) == action.GetType())
                {
                    action.RegisterExtraValue(source, del);
                }
            }
        }
        /// <summary>
        /// Through this you pass Types to an action that needs or at least are effected by componets of certain Types
        /// When Evaluating.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="typeName"></param>
        /// <param name="source"></param>
        public void RegisterActionReleventTypes<T>(string typeName, string source)
        {
            foreach (ActionBaseClass action in unit.Actions)
            {
                if (typeof(T) == action.GetType())
                {
                    (action as IAction).RegisterType(typeName, source);
                }
            }
        }
        /// <summary>
        /// This stops movement and interuptable action and makes a new pass of judgement
        /// </summary>
        internal void ForceStop(bool interuptAction) // TODO interuption
        {
            unit.Movement.Stop();
            unit.State = AISTATE.RESET;
        }
        internal void ForceTeleportTo(Vector3 point)
        {
            //GD.Print($"AIMind::ForceTeleportTo() point is [{point}]");
            unit.Movement.Stop();
            unit.Body.GlobalPosition = point;
            unit.State = AISTATE.RESET;
        }
    }// EOF CLASS
}