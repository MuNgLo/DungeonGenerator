using Godot;
namespace Munglo.Commons
{
    public partial class SimpleLocalRotation : Node3D
    {
        public enum ROTATIONAXIS { X, Y, Z }
        public bool nonScaledTime = false;
        public ROTATIONAXIS aroundAxis = ROTATIONAXIS.Y;
        public float rotationSpeed = 90.0f;

        public override void _Process(double delta)
        {
            double frameRotation = nonScaledTime ? rotationSpeed * delta : rotationSpeed * (delta / Engine.TimeScale);
            Rotate(ResolveAxis(), (float)frameRotation);
        }

        private Vector3 ResolveAxis()
        {
            switch (aroundAxis)
            {
                case ROTATIONAXIS.X:
                    return Vector3.Right;
                case ROTATIONAXIS.Y:
                    return Vector3.Up;
                case ROTATIONAXIS.Z:
                default:
                    return Vector3.Forward;
            }
        }
       
    }// EOF CLASS
}