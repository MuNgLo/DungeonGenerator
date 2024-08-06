using Godot;
using Munglo.DungeonGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Munglo.DungeonGenerator.UI
{
    [Tool]
    public partial class ModeSelection : OptionButton
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
            if (index > 0) { GetParent<MainScreen>().addon.ChangeMode(VIEWERMODE.SECTION); return; }
            GetParent<MainScreen>().addon.ChangeMode(VIEWERMODE.DUNGEON);
            GetParent<MainScreen>().RaiseUpdateUI();
        }

        private List<Type> GetList()
        {
            string nspace = "Munglo.DungeonGenerator.Sections";
            IEnumerable<Type> q = from t in Assembly.GetExecutingAssembly().GetTypes()
                    where t.IsClass && t.Namespace == nspace
                    select t;
            return q.ToList();
        }

        public Type GetSelectedType()
        {
            string nspace = "Munglo.DungeonGenerator.Sections";
            IEnumerable<Type> q = from t in Assembly.GetExecutingAssembly().GetTypes()
                                  where t.IsClass && t.Namespace == nspace
                                  select t;

            string selectedText = GetItemText(Selected);


            return q.ToList().Find(p=>p.Name == selectedText);
        }
    }// EOF CLASS
}