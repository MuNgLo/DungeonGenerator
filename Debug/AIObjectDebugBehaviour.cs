using Godot;
using Munglo.Commons;

namespace Munglo.AI.Debug
{
    /// <summary>
    /// This make it so we can debug an AI Object. See its bounds, force it to register in and other stuff that shouldn't be done in a real runtime
    /// </summary>
    public class AIObjectDebugBehaviour
    {
        [Export] private AIUnit unit;
        //[Header("AI Object Debug flags"), SerializeField]
        private bool showBounds = false;
        //[SerializeField]
        private bool forceRegister = false;
        //[SerializeField]
        private bool destroyAfterDisable = false;

        public AIObject ThisAIObject { get => GetAIObject(); }

        private AIObject GetAIObject()
        {
            //return GetComponent<AIObject>(); // fix this
            return unit; // fix this
        }

        void OnEnable()
        {
            if (forceRegister) { AIManager.Instance.RegisterAIObject(ThisAIObject, true); }
        }
        void OnDisable()
        {
            if (forceRegister) { AIManager.Instance.DeRegisterAIObject(ThisAIObject.aiObjectID, destroyAfterDisable); }
        }
        /*void OnDrawGizmos()
        {
            if (!showBounds) { return; }
            // Draw a semitransparent blue cube at the transforms position
            Gizmos.color = new Color(0, 0, 1, 0.5f);
            if (ThisAIObject.factionID == 1)
            {
                Gizmos.color = new Color(1, 0, 0, 0.5f);
            }
            else if (ThisAIObject.factionID == 2) { Gizmos.color = new Color(0, 1, 0, 0.5f); }
            Gizmos.DrawCube(transform.position + ThisAIObject.bounds.center, ThisAIObject.bounds.extents);
        }*/
    }//EOF CLASS
}