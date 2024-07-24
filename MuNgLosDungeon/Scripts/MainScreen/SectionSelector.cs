using Godot;
using System;
using System.Collections.Generic;
using System.Resources;

namespace Munglo.DungeonGenerator
{
    [Tool]
    internal partial class SectionSelector : OptionButton
    {
        private Dictionary<string, string> resources;
        public RoomResource sectionSelected;
        public override void _Ready()
        {
            VisibilityChanged += WhenVisibilityChanged;
            ItemSelected += WhenItemSelected;
            resources = new Dictionary<string, string>();
        }

        private void WhenItemSelected(long index)
        {
            string itemText = GetItemText((int)index);
            if(resources.ContainsKey(itemText))
            {
                sectionSelected = ResourceLoader.Load(resources[itemText]) as RoomResource;
                //GD.Print($"SectionSelector::WhenItemSelected() GOAL! sectionSelected[{sectionSelected.sectionName}]");
            }
        }

        private void WhenVisibilityChanged()
        {
            if(!Visible) { Clear(); sectionSelected = null; return; }
            resources = new Dictionary<string, string>();

            List< Resource > items = new List< Resource >();

            foreach(string file in DirAccess.GetFilesAt("res://addons/MuNgLosDungeon/Config/Rooms/"))
            {
                if (file.Contains("tres"))
                {
                    Resource res = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/Rooms/" + file);// + file.Replace(".tres", ""));
                    if (res is RoomResource) { items.Add(res); }
                }
            }

            if (items.Count > 0)
            {
                Clear();
                foreach (Resource res in items)
                {

                    if(res is RoomResource)
                    {
                        RoomResource room = res as RoomResource;
                        if(room.sectionName.Length < 1)
                        {
                            GD.PushError($"SectionSelector::WhenVisibilityChanged() The RoomResource [{room.ResourcePath}] has an invalid name. Make sure to set a valid name when making SectionResources.");
                            continue;
                        }

                        AddItem(room.sectionName);
                        resources[room.sectionName] = room.ResourcePath;
                    }
                }

            }



        }
    }// EOF CLASS
}
