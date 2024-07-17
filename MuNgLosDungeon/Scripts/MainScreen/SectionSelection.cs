using Godot;
using Munglo.DungeonGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
[Tool]
public partial class SectionSelection : OptionButton
{
	// Called when the node enters the scene tree for the first time.
	public override void _EnterTree()
	{
        Clear();
        AddItem("Dungeon");
        foreach (Type type in GetList())
        {
            if (type.Name.Contains("<>")) { continue; }
            if (type.GetInterface(nameof(ISection)) == null) { continue; }
            AddItem(type.Name);
        }
        Select(0);
        GD.Print($"SectionSelection::_Ready() GetList.Count[{GetList().Count}] itemCount[{ItemCount}]");
	}
    public override void _ExitTree()
    {
        ItemSelected -= WhenItemSelected;
    }
    public override void _Ready()
	{
        ItemSelected += WhenItemSelected;
	}
    private void WhenItemSelected(long index)
    {
        if(index > 0) { GetParent<MainScreen>().addon.ChangeMode(VIEWERMODE.SECTION); return; }
        GetParent<MainScreen>().addon.ChangeMode(VIEWERMODE.DUNGEON);
    }

    private List<Type> GetList()
    {
        string nspace = "Munglo.DungeonGenerator.Sections";
        //List<string> result = new List<string>();
        var q = from t in Assembly.GetExecutingAssembly().GetTypes()
                where t.IsClass && t.Namespace == nspace
                select t;
        //q.ToList().ForEach(t => Console.WriteLine(t.Name));
        //q.ToList().ForEach(t => { if (t.Name.Contains("Section")) { result.Add t.Name; } });
        return q.ToList();
    }
}
