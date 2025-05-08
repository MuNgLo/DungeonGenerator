using Godot;
using Munglo.DungeonGenerator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Munglo.DungeonGenerator.UI
{
    [Tool]
    public partial class SectionTypeListButton : OptionButton
    {
        private MainScreen screen;
        public EventHandler<Type> OnSectionTypeSelected;
        public override void _EnterTree()
        {
            Clear();
            foreach (Type type in GetList())
            {
                if (type.Name.Contains("<>")) { continue; }
                if (type.GetInterface(nameof(ISection)) == null) { continue; }
                AddItem(type.Name);
            }
            Select(0); // SELECT 0 DEFAULT ONE for starters
            GD.Print($"SectionTypeListButton::_Ready() GetList.Count[{GetList().Count}] itemCount[{ItemCount}]");
        }
        public override void _ExitTree()
        {
            ItemSelected -= WhenItemSelected;
        }
        public override void _Ready()
        {
            screen = GetParent().GetParent() as MainScreen;
            ItemSelected += WhenItemSelected;
        }
        private void WhenItemSelected(long index)
        {
            RaiseSectionTypeChanged();
        }
        private void RaiseSectionTypeChanged(){
            EventHandler<Type> evt = OnSectionTypeSelected;
            if(evt is not null)
            {
                Type T = GetSelectedType();
                evt (this, T);
            }
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