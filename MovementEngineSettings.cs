using Godot;
namespace Munglo.Commons;
[GlobalClass]
internal partial class MovementEngineSettings : Node, IMovement
{
    [Export] private float groundSpeed = 4.0f;
    [Export] private float maxRandomMoveDistance = 16.0f;
    public float MaxRandomMoveDistance => maxRandomMoveDistance;
    public float GroundSpeed => groundSpeed;
}// EOF CLASS
