using Godot;

namespace FFF.Simple;
[GlobalClass]
public partial class TimeToLive : Node
{
    [Export] private bool killParent = false;
    [Export] private bool always = false;
    [Export] public float ttl = 5.0f;
    private double timeLeft;
    public override void _Ready()
    {
        timeLeft = ttl;
    }
    public override void _Process(double delta)
    {
        timeLeft -= delta;
        if (timeLeft < 0.0) { if (killParent) { GetParent().QueueFree(); } else { QueueFree(); } }
    }
}// EOF CLASS
