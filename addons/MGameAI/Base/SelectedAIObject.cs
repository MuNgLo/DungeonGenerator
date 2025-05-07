using Munglo.Commons;
using System.Reflection;
using Godot;
using System;

namespace Munglo.AI.Base
{
    [System.Serializable]
    public class SelectedAIObject
    {
        //[SerializeField, Header("Selected")]
        private AIObject selectedObject;

        public AIObject Selected { get => selectedObject; }
        public AIUnit SelectedUnit
        {
            get => selectedObject is AIUnit ? selectedObject as AIUnit : default(AIUnit);
        }

        public SelectedAIObject()
        {

        }

        public void Select(AIObject obj)
        {
            if (selectedObject is null || obj is null || obj.aiObjectID != selectedObject.aiObjectID)
            {
                Debug.AIDebugSignals.Singleton.ClearDebugInfo?.Invoke(this, null);
            }
            selectedObject = obj;
        }
        public void Select(AIUnit unit)
        {
            Select(unit as AIObject);
        }

        public void UnitKill()
        {
            if(SelectedUnit == null) { return; }
            if (SelectedUnit.aiObjectID > 0)
            {
                SelectedUnit.GetNode<Health>("Health").TakeDirectDamage(
                    new DamagePackage()
                    {
                        damage = 10000.0f,
                        source = DSOURCE.NONE,
                        directionStyle = DDIRECTIONAL.NO
                    }
                    );
            }
        }

        public void UnitHeal()
        {
            if (SelectedUnit == null) { return; }
            if (SelectedUnit.aiObjectID > 0)
            {
                SelectedUnit.GetNode<Health>("Health").SetFullHealth();
            }
        }
        public void UnitResetMind()
        {
            if (SelectedUnit == null) { return; }
            if (SelectedUnit.aiObjectID > 0)
            {
                SelectedUnit.Mind.ForceStop(true);
            }
        }
        public void UnitInterupt()
        {
            if (SelectedUnit == null) { return; }
            if (SelectedUnit.aiObjectID > 0)
            {
                SelectedUnit.Mind.ForceStop(true);
            }
        }

        internal void UnitMoveTo(Vector3 point)
        {
            if (SelectedUnit == null) { return; }
            if (SelectedUnit.aiObjectID > -1)
            {
                SelectedUnit.Mind.ForceTeleportTo(point);
            }
        }
    }// EOF CLASS
}
