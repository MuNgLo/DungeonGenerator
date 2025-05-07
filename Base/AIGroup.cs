using Godot;
using System.Collections;
using System.Collections.Generic;

namespace Munglo.AI.Base
{
    [System.Serializable]
    internal class AIGroup
    {
        //[SerializeField]
        private List<AIUnit> units;
        //[SerializeField]
        internal int Count { get { return units.Count; } }

        public AIGroup()
        {
            units = new List<AIUnit>();
        }

        internal void UpdateGroup(float delta, float minUpdateStep, ref int iterationCount)
        {
            foreach (AIUnit unit in units)
            {
                if (AIManager.Instance.IsInstancevalidNode(unit.Body) && unit.Body.Visible)
                {
                    //Debug.Log($"Min updateStep = {minUpdateStep} :: TimeSinceLastUdpate = {mind.TimeSinceLastUpdate}");
                    unit.UpdateUnit(delta, minUpdateStep);
                    iterationCount++;
                }
                else
                {
                    //Debug.Log("Mind skipped!");
                }
            }
        }

        internal void Add(AIUnit unit)
        {
            units.Add(unit);
        }
        internal void Remove(AIUnit unit)
        {
            units.Remove(unit);
        }
        internal void Remove(int unitAIObjectID, int groupID)
        {
            if(!units.Exists(p=>p.aiObjectID == unitAIObjectID))
            {
                GD.PrintErr($"AIGroup::Remove({unitAIObjectID}) The ID doesn't exist in this group[{groupID}]");
            }
            else
            {
                AIUnit unit = units.Find(p => p.aiObjectID == unitAIObjectID);
                units.Remove(unit);
                unit.aiObjectID = -1;
            }
        }
    }// EOF CLASS
}