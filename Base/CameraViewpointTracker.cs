using Godot;
namespace Munglo.Movement.Base
{
    /// <summary>
    /// This class is just translated to Godot. not tested or needed probably
    /// </summary>
    public partial class CameraViewpointTracker : Node3D
    {
        public Node3D _viewpoint;
        private void LateUpdate()
        {
            GlobalPosition = _viewpoint.GlobalPosition;
            GlobalRotation = _viewpoint.GlobalRotation;
        }
    }// EOF CLASS
}