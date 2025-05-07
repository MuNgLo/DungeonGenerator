using System;
using Godot;
using Munglo.Commons;

namespace Munglo.AI.Base
{
    public class ViewObject
    {
        public int aIObjectID;
        public int factionID;
        public bool isVisible;
        private bool isPermanent;
        public bool hasSeen;
        public float keepfor;
        public Aabb bounds;
        private AITYPE aiType;
        private Vector3 lastKnownPosition;
        // On access updates lastknown position if visible. Then returns the value of lastknown position.
        public Vector3 Position { get { 
                if (isVisible && AIManager.Matrix.GetAIObject(aIObjectID) != null)
                {
                    lastKnownPosition = AIManager.Matrix.GetAIObject(aIObjectID).GlobalPosition; 
                } 
                return lastKnownPosition; } 
        }
        public bool IsPermanent { get => isPermanent; }
        public Vector3 WBC { get => Position + bounds.GetCenter(); }
        public ViewObject(ObjectOfInterest item, float keepFor, bool visible=true, bool isPerm=false)
        {
            aiType = item.aiType;
            aIObjectID = item.aIObjectID;
            factionID = item.aIFactionID;
            isVisible = visible;
            hasSeen = visible;
            lastKnownPosition = item.aiObject.GlobalPosition;
            keepfor = keepFor;
            bounds = item.bounds;
            isPermanent = isPerm;
        }
    }
    [Serializable]
    public struct ObjectOfInterest{
        public AIObject aiObject;
        public int aIObjectID;
        public int aIFactionID;
        public AITYPE aiType;
        public Aabb bounds;
        public Vector3 WBC { get => aiObject.Body.GlobalPosition + bounds.GetCenter(); }
        private int unitFlag;
        public bool isUnit { get {
            if(unitFlag == 0 && aiObject != null)
                {
                    if ((aiObject as AIUnit) != null) { unitFlag = 2; } else { unitFlag = 1; }
                }
                return unitFlag == 2;
            } }
    }

    [Serializable]
    public struct UnitMovementGear
    {
        public string name;
        public float speed;
    }
    [Serializable]
    public struct UnitMovementMaster
    {
        public string name;
        public UnitMovementGear[] gears;
    }
}
