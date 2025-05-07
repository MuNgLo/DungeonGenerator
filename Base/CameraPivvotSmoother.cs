using Godot;
namespace Munglo.Movement.Base
{
    /// <summary>
    /// This class is just translated to Godot. not tested or needed probably
    /// </summary>
    public partial class CameraPivvotSmoother : Node3D
    {
        private Vector3 _startPos;
        public override void _Ready()
        {
            _startPos = Position;
        }

        // Update is called once per frame
        void Update()
        {
            // Need to move the camera after the player has been moved because otherwise the camera will clip the player if going fast enough and will always be 1 frame behind.
            // Set the camera's position to the transform
            Position = _startPos;
        }
    }// EOF CLASS
}