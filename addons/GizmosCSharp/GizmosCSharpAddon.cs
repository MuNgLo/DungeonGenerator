#if TOOLS
using Godot;

namespace GizmosCSharp;
[Tool]
public partial class GizmosCSharpAddon : EditorPlugin
{
    public override void _EnterTree()
    {
        GD.Print("Loaded GizmosCSharp Plugin");
    }
    public override void _ExitTree()
    {
        GD.Print("Unloaded GizmosCSharp Plugin");
    }

    public override bool _HasMainScreen()
    {
        return false;
    }
    public override string _GetPluginName()
    {
        return "GizmosCSharp";
    }
    /*public override Texture2D _GetPluginIcon()
    {
        Texture2D icon = ResourceLoader.Load("res://addons/MuNgLosDungeon/Icons/AddonIcon.png") as Texture2D;
        return icon;
    }*/
}// EOF CLASS
#endif