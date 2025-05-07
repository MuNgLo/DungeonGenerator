using System.Collections.Generic;
using Godot;

namespace GizmosCSharp;
[GlobalClass, Tool]
public partial class SimpleDebugLine : GizmoNode3D
{
    [ExportToolButton("Update")]
    private Callable runUpdate => Callable.From(RunUpdate);
    [Export] private float showFor = 1.0f;
    [Export] private Color col;
    [Export] private Node3D start;
    [Export] private Node3D end;
    [Export] private bool addEndShape = true;
    [Export] private float shapeScale = 1.0f;
    [Export] private GSHAPES shape;
    [Export] private Color shapeCol;


    public void RunUpdate()
    {
        GizmoUtils.DrawLine(start.GlobalPosition, end.GlobalPosition, showFor, col);
        if(addEndShape){
            GizmoUtils.DrawShape(end.GlobalPosition, shape, showFor, shapeScale, shapeCol);
        }
    }

}// EOF CLASS