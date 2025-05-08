using Godot;

namespace FFF.Simple;
[Tool]
public partial class OccilateAndRotate : Node3D
{
    [Export] private bool isRunning = false;
    [Export] private float rotationSpeed = 1.0f;
    [Export] private float occislateSpeed = 1.0f;
    [Export] private float lowerPosition = 0.0f;
    [Export] private float upperPosition = 0.0f;
    private float timingOffset = 1.0f;

    private float HeightSpan => upperPosition - lowerPosition;
    public override void _EnterTree()
    {
        timingOffset = GD.Randf() * occislateSpeed;
        Vector3 pos = Position;
        pos.Y = Mathf.Sin(Time.GetTicksMsec() * 0.001f * occislateSpeed + occislateSpeed * timingOffset) * HeightSpan + lowerPosition;
        Position = pos;
    }
    public override void _Process(double delta)
    {
        if(!isRunning){return;}
        Rotate(Vector3.Up, rotationSpeed * (float)delta);
        Vector3 pos = Position;
        pos.Y = Mathf.Sin(Time.GetTicksMsec() * 0.001f * occislateSpeed + occislateSpeed * timingOffset) * HeightSpan + lowerPosition;
        Position = pos;
    }

}
