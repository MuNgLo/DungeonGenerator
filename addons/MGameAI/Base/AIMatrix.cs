using Godot;
using Munglo.Commons;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Munglo.AI.Base{

    /// <summary>
    /// This is a matrix of ObjectOfInterest references to evaluate sound events and who sees what.
    /// </summary>
    public class AIMatrix
    {
        private List<ObjectOfInterest> matrix;

        private int lookupCount = 0;
        public int LookupCount { get => lookupCount; }
        internal int Size { get => matrix.Count; }

        public AIMatrix(){
            matrix = new List<ObjectOfInterest>();
        }
        public void StatReset()
        {
            lookupCount = 0;
        }
        internal void AddObject(ObjectOfInterest objectOfInterest)
        {
            if(!matrix.Exists(p=>p.aIObjectID == objectOfInterest.aIObjectID)){
                //Debug.Log($"Addobject() {objectOfInterest.aIFactionID}");
                matrix.Add(objectOfInterest);
            }
        }

        internal void UpdateFaction(int aiObjectID, int newFID)
        {
            if (matrix.Exists(p => p.aIObjectID == aiObjectID))
            {
                //Debug.Log($"UpdateFaction() {newFID}");
                ObjectOfInterest ob = matrix.Find(p=>p.aIObjectID == aiObjectID);
                RemoveObject(aiObjectID);
                ob.aIFactionID = newFID;
                AddObject(ob);
            }
        }

        internal void RemoveObject(int id)
        {
            if(matrix.Exists(p=>p.aIObjectID == id)){
                matrix.RemoveAll(p=>p.aIObjectID == id);
            }
        }

        internal List<ObjectOfInterest> GetObjectsOfInterest(Vector3 point, float radius){
            lookupCount++;
            return matrix.FindAll(
                p=>AIManager.Instance.IsInstancevalidNode(p.aiObject) && !p.aiObject.Body.IsQueuedForDeletion()
                &&
                Mathf.Abs(p.aiObject.GlobalPosition.X - point.X) < radius
                &&
                Mathf.Abs(p.aiObject.GlobalPosition.Y - point.Y) < radius
                &&
                Mathf.Abs(p.aiObject.GlobalPosition.Z - point.Z) < radius
                );
        }

        public bool IsIDinMatrix(int aiObjectID)
        {
            return matrix.Exists(p => p.aIObjectID == aiObjectID);
        }

        /// <summary>
        /// Make sure ID exists in matrix before calling this
        /// </summary>
        /// <param name="aiObjectID"></param>
        /// <returns></returns>
        public AIObject GetAIObject(int aiObjectID)
        {
            return matrix.Find(p => p.aIObjectID == aiObjectID).aiObject;
        }
        
        internal int UnitsinMatrix()
        {
            return matrix.FindAll(p => p.isUnit).Count;
        }
        internal int ObjectsinMatrix()
        {
            return matrix.FindAll(p => !p.isUnit).Count;
        }

        internal DSOURCE ResolveDamageSource(int ownerID)
        {
            ObjectOfInterest obj = matrix.Find(p => p.aIObjectID == ownerID);
            if (obj.isUnit) { return DSOURCE.MOB; }
            
            // TODO figure out how to make this not dependent on Movement module
            //if(obj.transform.GetComponent<Munglo.Movement.Base.Motor>()) { return DSOURCE.PLAYER; }
            return DSOURCE.WORLD;
        }
    }// EOF CLASS
}