using Godot;
using System;

public partial class GlobalInput : Node
{
    public override void _PhysicsProcess(double delta)
    {
        if(Input.IsActionJustPressed("ToggleConsole")){
            GameConsole.Toggle();
            GameConsole.AddLine("BLA BLA BAL BALB LBA");
        }
    }
}// EOF CLASS
