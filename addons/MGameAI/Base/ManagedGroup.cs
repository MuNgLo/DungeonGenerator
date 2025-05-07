using System.Collections;
using System.Collections.Generic;
using Godot;
namespace Munglo.AI.Base
{
    /// <summary>
    /// This represents an AIGroup for the AIManager. This way it can track iteration and time spent on updates.
    /// </summary>
    [System.Serializable]
    internal class ManagedGroup
    {
        //[SerializeField]
        internal AIGROUPSPEC spec;
        //[SerializeField]
        internal int groupID;
        //[SerializeField]
        internal AIGroup aiGroup;

        internal AIGROUPSPEC Spec { get { return spec; } }
        internal int Count { get { return aiGroup.Count; } }
        internal int GroupdID { get { return groupID; } }
        internal string Stats { get { return $"ID:{groupID} SPEC:{spec} Members:{aiGroup.Count}"; } }

        internal ManagedGroup(AIGROUPSPEC spec, int groupID)
        {
            this.spec = spec;
            this.groupID = groupID;
            aiGroup = new AIGroup();
        }

        internal void UpdateGroup(float delta, float minUpdateStep, ref int iterationCount)
        {
            aiGroup.UpdateGroup(delta, minUpdateStep, ref iterationCount);
        }

        internal void Add(AIUnit unit)
        {
            aiGroup.Add(unit);
        }
        internal void Remove(AIUnit unit)
        {
            aiGroup.Remove(unit);
        }
        internal void Remove(int unitAIObjectID)
        {
            aiGroup.Remove(unitAIObjectID, groupID);
        }

    }// EOF CLASS
}