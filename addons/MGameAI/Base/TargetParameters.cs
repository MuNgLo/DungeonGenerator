using Godot;

namespace Munglo.AI.Base
{
    [System.Serializable]
    public class TargetParameters
    {
        public int AIObjectID = -1;
        public Vector3 location;
        public Aabb bounds;
        public float distance;
        public bool isInView;
        public bool IsValid { get => AIObjectID != -1; }
    }
}
