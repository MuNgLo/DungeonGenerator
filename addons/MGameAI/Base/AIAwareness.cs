using System;
using System.Collections.Generic;
using Godot;
using Munglo.AI.Debug;
using Munglo.Commons;
using Munglo.GameEvents;
using static System.Net.Mime.MediaTypeNames;

namespace Munglo.AI.Base
{
    /// <summary>
    /// Handles the AIMinds knowledge. Determins what is visible or not.
    /// </summary>
    [Serializable]
    public class AIAwareness
    {
        public EventHandler<int> OnVisibleChangedVisible;
        public EventHandler<int> OnVisibleChangedHidden;
        public EventHandler<int> OnForgotten;
        
        private AIUnit unit;
        private List<ViewObject> knownObjects;
         
        public AIAwareness(AIUnit aiUnit)
        {
            unit = aiUnit;
            AIManager.OnAIObjectRemoved += OnAIObjectRemoved;
            knownObjects = new List<ViewObject>();
        }

        private void OnAIObjectRemoved(object sender, int aiObjectID)
        {
            if(knownObjects.Exists(p=>p.aIObjectID == aiObjectID)){
                ForgetAbout(aiObjectID);
            }
        }

        /// <summary>
        /// Updates list of knownobjects. Updates visiblity, keepfor. Also updates location for visible objects.
        /// </summary>
        public void UpdateAwareness()
        {
            //GD.Print("AIAwareness::UpdateAwareness()");
            List<ObjectOfInterest> objectsNearby = AIManager.Matrix.GetObjectsOfInterest(unit.EyeLocation, unit.ZoneConfig.Range);
            // Proccess info about objects around us
            foreach (ObjectOfInterest item in objectsNearby)
            {
                // Skip ourself
                if (item.aIObjectID == unit.aiObjectID) { continue; }
                // Unknown new object
                if (!knownObjects.Exists(p => p.aIObjectID == item.aIObjectID))
                {
                    if (CheckIfVisible(item))
                    {
                        knownObjects.Add(new ViewObject(item, unit.KeepFor, true, item.aiObject.isPermanantKnowledge));
                        OnVisibleChangedVisible?.Invoke(this, item.aIObjectID);
                    }
                }
                else
                {
                    // Update info on known object
                    ViewObject knownObject = knownObjects.Find(p => p.aIObjectID == item.aIObjectID);
                    // Update currently known object locations visible flag and keepfor *** LOCATION ***
                    if (CheckIfVisible(item))
                    {
                        if (!knownObject.isVisible) { 
                            knownObject.isVisible = true;
                            OnVisibleChangedVisible?.Invoke(this, item.aIObjectID); 
                        }
                        knownObject.isVisible = true;
                        knownObject.keepfor = unit.KeepFor;
                    }
                    else
                    {
                        // Vision is blocked
                        if (knownObject.isVisible) 
                        {
                            // object just turned not visible so change flag and raise event
                            knownObject.isVisible = false;
                            OnVisibleChangedHidden?.Invoke(this, item.aIObjectID); 
                        }
                    }
                }
            }
            // Update knownobjects
            foreach (ViewObject viewObject in knownObjects)
            {
                // lower all keepfor
                if (!viewObject.IsPermanent) { viewObject.keepfor -= unit.TimeSinceLastUpdate; }
                // process those outside view range
                if (!objectsNearby.Exists(p => p.aIObjectID == viewObject.aIObjectID))
                {
                    if (viewObject.isVisible) { 
                        viewObject.isVisible = false; 
                        OnVisibleChangedHidden?.Invoke(this, viewObject.aIObjectID); 
                    }
                    viewObject.isVisible = false;
                }
                // Forget about all things we haven't seen for a while
                if (viewObject.keepfor < 0.0f && !viewObject.IsPermanent)
                {
                    OnForgotten?.Invoke(this, viewObject.aIObjectID);
                }
            }
            // Remove things we forget
            int removed = knownObjects.RemoveAll(p => p.keepfor < 0.0f);
            if(removed > 0 && unit.IsSelected)
            {
                GD.Print($"AIAwareness::UpdateAwareness() Forgetting about [{removed}] things");
                Debug.AIDebugSignals.Singleton.RaiseCustomDataEvent(
                    new AICustomDataStruct()
                    {
                        message = $"Forgot [{removed}] things.",
                        sourceClass = this.GetType().Name,
                        value = 0,
                        normalizedValue = 0.0f
                    }
                );
            }
        }
        /// <summary>
        /// Returns True and distance value if the things list could be processed
        /// TODO WTF is this really?
        /// </summary>
        /// <param name="things"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public bool GetDistanceToClosest(List<int> things, out float distance)
        {
            distance = float.PositiveInfinity;
            bool result = false;
            foreach (int item in things)
            {
                if(distance > GetDistanceToKnownObject(item))
                {
                    distance = GetDistanceToKnownObject(item);
                    result = true;
                }
            }
            return result;
        }

        internal List<TargetParameters> Targets()
        {
            List<TargetParameters> targets = new List<TargetParameters>();
            Faction faction = AIManager.Factions.GetFaction(unit.factionID);
            foreach (ViewObject vObject in knownObjects)
            {
                if (IsTarget(vObject, faction)) {
                    targets.Add(new TargetParameters()
                    {
                        AIObjectID = vObject.aIObjectID,
                        bounds = vObject.bounds,
                        isInView = vObject.isVisible,
                        location = vObject.Position,
                        distance = unit.GlobalPosition.DistanceTo(vObject.Position)
                    });
                }
            }
            return targets;
        }

        /// <summary>
        /// Returns True and the closest viewobject if the the list of things could be processed
        /// </summary>
        /// <param name="things"></param>
        /// <param name="vObject"></param>
        /// <returns></returns>
        public bool GetClosest(List<int> things, out ViewObject vObject)
        {
            vObject = null;
            if(things.Count < 1) { return false; }
            int pickedID = -1;
            float distance = float.PositiveInfinity;
            foreach (int oID in things)
            {
                if (distance > GetDistanceToKnownObject(oID))
                {
                    distance = GetDistanceToKnownObject(oID);
                    pickedID = oID;
                }
            }
            if (pickedID == -1) { return false; }
            vObject = knownObjects.Find(p => p.aIObjectID == pickedID);
            return vObject != null;
        }
        public bool GetClosest<T>(out ViewObject vObject)
        {
            vObject = null;
            ViewObject[] matches = knownObjects.ToArray();
            int pickedID = -1;
            float distance = float.PositiveInfinity;
            foreach (ViewObject vObj in matches)
            {
                if (AIManager.Matrix.GetAIObject(vObj.aIObjectID) is T)
                {
                    if (distance > GetDistanceToKnownObject(vObj.aIObjectID))
                    {
                        distance = GetDistanceToKnownObject(vObj.aIObjectID);
                        pickedID = vObj.aIObjectID;
                    }
                }
            }
            if (pickedID == -1) { return false; }
            vObject = knownObjects.Find(p => p.aIObjectID == pickedID);
            return true;
        }
        public bool GetClosest<T>(List<int> excludeFilter, out ViewObject vObject) where T : Node3D
        {
            vObject = null;
            ViewObject[] matches = knownObjects.FindAll(p => p.aIObjectID is T).ToArray();
            int pickedID = -1;
            float distance = float.PositiveInfinity;
            foreach (ViewObject vObj in matches)
            {
                if(excludeFilter.Exists(p=>p == vObj.aIObjectID)) { continue; }
                if (distance > GetDistanceToKnownObject(vObj.aIObjectID))
                {
                    distance = GetDistanceToKnownObject(vObj.aIObjectID);
                    pickedID = vObj.aIObjectID;
                }
            }
            if (pickedID == -1) { return false; }
            vObject = knownObjects.Find(p => p.aIObjectID == pickedID);
            return true;
        }


        public bool GetClosestByInterface<T>(out ViewObject vObject)
        {
            vObject = null;
            ViewObject[] matches = knownObjects.FindAll(p => p is T).ToArray();
            int pickedID = -1;
            float distance = float.PositiveInfinity;
            foreach (ViewObject vObj in matches)
            {
                if (distance > GetDistanceToKnownObject(vObj.aIObjectID))
                {
                    distance = GetDistanceToKnownObject(vObj.aIObjectID);
                    pickedID = vObj.aIObjectID;
                }
            }
            if (pickedID == -1) { return false; }
            vObject = knownObjects.Find(p => p.aIObjectID == pickedID);
            return true;
        }
        public bool GetClosestByInterface<T>(List<int> excludeFilter, out ViewObject vObject)
        {
            vObject = null;
            ViewObject[] matches = knownObjects.FindAll(p => p is T).ToArray();
            int pickedID = -1;
            float distance = float.PositiveInfinity;
            foreach (ViewObject vObj in matches)
            {
                if (excludeFilter.Exists(p => p == vObj.aIObjectID)) { continue; }
                if (distance > GetDistanceToKnownObject(vObj.aIObjectID))
                {
                    distance = GetDistanceToKnownObject(vObj.aIObjectID);
                    pickedID = vObj.aIObjectID;
                }
            }
            if (pickedID == -1) { return false; }
            vObject = knownObjects.Find(p => p.aIObjectID == pickedID);
            return true;
        }



        /// <summary>
        /// Returns the distance to the objects last known location. If invalid object float.PositiveInfinity is returned
        /// </summary>
        /// <param name="aiObjectID"></param>
        /// <returns></returns>
        public float GetDistanceToKnownObject(int aiObjectID)
        {
            ViewObject vObj = knownObjects.Find(p => p.aIObjectID == aiObjectID);
            if (vObj != null)
            {
                return unit.GlobalPosition.DistanceTo(vObj.Position);
            }
            return float.PositiveInfinity;
        }
        /// <summary>
        /// Gets last known location for a known object
        /// </summary>
        /// <param name="aiObjectID"></param>
        /// <returns></returns>
        public Vector3 GetObjectLocation(int aiObjectID)
        {
            if (knownObjects.Exists(p => p.aIObjectID == aiObjectID))
            {
                return knownObjects.Find(p => p.aIObjectID == aiObjectID).Position;
            }
            return unit.GlobalPosition;
        }
        public TargetParameters GetAsTarget(int aiObjectID)
        {
            if (knownObjects.Exists(p => p.aIObjectID == aiObjectID))
            {
                ViewObject vObject = knownObjects.Find(p => p.aIObjectID == aiObjectID);
                TargetParameters target = new TargetParameters()
                {
                    AIObjectID = vObject.aIObjectID,
                    bounds = vObject.bounds,
                    isInView = vObject.isVisible,
                    location = vObject.Position,
                    distance = unit.GlobalPosition.DistanceTo(vObject.Position)
                };
                return target;
            }
            return new TargetParameters();
        }
        
        /// <summary>
        /// Counts all viewObjects that are hostile in knownobjects
        /// </summary>
        /// <returns></returns>
        public int NumberOfTargets()
        {
            int count = 0;
            Faction thisUnitsfaction = AIManager.Factions.GetFaction(unit.factionID);
            foreach (ViewObject item in knownObjects)
            {
                if(item.factionID < 0)
                {
                    continue;
                }
                if (IsTarget(item, thisUnitsfaction)) { count++; }
            }
            return count;
        }
        /// <summary>
        /// Resolves faction relationship and returns true if hostile
        /// </summary>
        /// <param name="aiObjectId"></param>
        /// <returns></returns>
        public bool IsTarget(int aiObjectId)
        {
            if(knownObjects.Exists(p=>p.aIObjectID == aiObjectId))
            {
                Faction faction = AIManager.Factions.GetFaction(unit.factionID);
                ViewObject vObj = knownObjects.Find(p=>p.aIObjectID == aiObjectId);
                return IsTarget(vObj, faction);
            }
            return false;
        }

        /// <summary>
        /// Resolves faction relationship and returns true if hostile
        /// </summary>
        /// <param name="item"></param>
        /// <param name="thisUnitsfaction"></param>
        /// <returns></returns>
        private bool IsTarget(ViewObject item, Faction thisUnitsfaction)
        {
            if (item.factionID != thisUnitsfaction.id)
            {
                Faction theirFaction = AIManager.Factions.GetFaction(item.factionID);
                for (int i = 0; i < thisUnitsfaction.relations.Length; i++)
                {
                    if (theirFaction.id == thisUnitsfaction.relations[i].id)
                    {
                        if (thisUnitsfaction.relations[i].state == FACTIONRELATIONSTATE.HOSTILE)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Call this when returning oibject to pool. Usually propegated from AIMind
        /// </summary>
        public void PoolReset()
        {
            knownObjects = new List<ViewObject>();
        }

        #region visibility stuff
        internal bool CheckIfVisible(ObjectOfInterest item)
        {
            //return CheckIfVisiblePoint(item.aIObjectID, item.WBC);
            return true; // TODFO FIX IT!!FIX IT!!FIX IT!!FIX IT!!FIX IT!!FIX IT!!FIX IT!!FIX IT!!FIX IT!!FIX IT!!FIX IT!!
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="thing"></param>
        internal bool CheckIfVisiblePoint(int aiObjectID, Vector3 point)
        {
            float distanceToPoint = unit.EyeLocation.DistanceTo(point);
            // Check within viewrange
            if (distanceToPoint > unit.ZoneConfig.Range) { return false; }
            // Check if thing is visible
            if (unit.Zone.IsPointInsideZone(point))
            {
                World3D world = unit.Body.GetWorld3D();
                PhysicsDirectSpaceState3D spaceState = PhysicsServer3D.SpaceGetDirectState(world.Space);
                Godot.Collections.Array<Rid> excluding = new Godot.Collections.Array<Rid> { };
                excluding.Add((unit.Body as CharacterBody3D).GetRid());
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(unit.EyeLocation, point, exclude: excluding);
                Godot.Collections.Dictionary results = spaceState.IntersectRay(query);
                if (results != null)
                {
                    // Check if vision is blocked
                    if (results.Keys.Count > 0)
                    {
                        Node3D asd = results["collider"].AsGodotObject() as Node3D;
                        if (asd is CharacterBody3D)
                        {
                            //if((asd as Personality).AIObjectID == aiObjectID)
                            //{
                            //    //GD.Print($"AIAwareness::CheckIfVisiblePoint({aiObjectID}) Cast hit [{(asd as Personality).AIObjectID}] TRUE");
                            //    return true;
                            //}
                            //GD.Print($"AIAwareness::CheckIfVisiblePoint({aiObjectID}) Cast hit [{(asd as Personality).AIObjectID}] FALSE");
                            return false;
                        }
                        else
                        {
                            /*
                            if (asd is AIRBObject)
                            {
                                if((asd as AIRBObject).GetAIObject.aiObjectID == aiObjectID)
                                {
                                    //GD.Print($"AIAwareness::CheckIfVisiblePoint({aiObjectID}) Cast hit [{asd.Name}] is an AIRBObject  TRUE");
                                    return true;
                                }
                            }
                            */
                        }
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
        #endregion
      
        /// <summary>
        /// Forgets about a single object and raises the event wioth the ID
        /// </summary>
        /// <param name="aIObjectID"></param>
        public void ForgetAbout(int aIObjectID)
        {
            if(knownObjects.Exists(p=>p.aIObjectID == aIObjectID))
            {
                if(knownObjects.Find(p => p.aIObjectID == aIObjectID).IsPermanent)
                {
                    return;
                }
            }
            if (knownObjects.RemoveAll(p => p.aIObjectID == aIObjectID) > 0)
            {
                OnForgotten?.Invoke(this, aIObjectID);
            }
        }
        /// <summary>
        /// Tries to select and return the closest enemy object of known objects
        /// </summary>
        /// <param name="onlyVisible"></param>
        /// <returns></returns>
        public TargetParameters GetClosestTarget(bool onlyVisible)
        {
            Faction faction = AIManager.Factions.GetFaction(unit.factionID);
            int pick = -1;
            float distance = 100000.0f;
            for (int i = 0; i < knownObjects.Count; i++)
            {
                // Skip if not visible when asking for visible
                if (onlyVisible && !knownObjects[i].isVisible) { continue; }
                // Skip if this is not an enemy
                if (!IsTarget(knownObjects[i], faction)) { continue; }
                // change pick if this one is closer
                if (distance > unit.GlobalPosition.DistanceTo(knownObjects[i].Position))
                {
                    distance = unit.GlobalPosition.DistanceTo(knownObjects[i].Position);
                    pick = i;
                }
            }
            // No target could be picked so reteurn a blank target
            if (pick < 0) { return new TargetParameters() { }; }
            // Return picked target
            return new TargetParameters()
            {
                AIObjectID = knownObjects[pick].aIObjectID,
                location = knownObjects[pick].Position,
                distance = unit.GlobalPosition.DistanceTo(knownObjects[pick].Position),
                bounds = knownObjects[pick].bounds,
                isInView = knownObjects[pick].isVisible
            };
        }
        public ViewObject GetKnownObject(int aiObjectID)
        {
            return knownObjects.Find(p=>p.aIObjectID == aiObjectID);
        }
        /// <summary>
        /// Tries to select and return the closest enemy object of known objects
        /// If filter is given the object needs to be in filter
        /// No matches returns a blank targetparameter object with AIObjectID -1
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public TargetParameters GetClosest(List<int> filter = null)
        {
            int pick = -1;
            float distance = 100000.0f;
            for (int i = 0; i < knownObjects.Count; i++)
            {
                // filter if given
                if(filter is not null)
                {
                    if(!filter.Exists(p=>p == knownObjects[i].aIObjectID)) { continue; }
                }
                // Passed filter and change pick if this one is closer
                if (distance > unit.GlobalPosition.DistanceTo(knownObjects[i].Position))
                {
                    distance = unit.GlobalPosition.DistanceTo(knownObjects[i].Position);
                    pick = i;
                }
            }
            // No target could be picked so return a blank target
            if (pick < 0) { return new TargetParameters() { }; }
            // Return picked target
            return new TargetParameters()
            {
                AIObjectID = knownObjects[pick].aIObjectID,
                location = knownObjects[pick].Position,
                distance = unit.GlobalPosition.DistanceTo(knownObjects[pick].Position),
                bounds = knownObjects[pick].bounds,
                isInView = knownObjects[pick].isVisible
            };
        }
    }// EOF CLASS
}
