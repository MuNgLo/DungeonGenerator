using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace Munglo.DungeonGenerator.UI
{
    [Tool]
    internal partial class SectionSelector : OptionButton
    {
        private MainScreen screen;
        private SectionTypeListButton sectionTypeSelector;

        private Dictionary<string, string> resources;
        public override void _Ready()
        {
            screen = GetParent().GetParent() as MainScreen;
            sectionTypeSelector = GetParent().GetNode<SectionTypeListButton>("btnSectionTypeList");
            //screen.OnMainScreenUIUpdate += WhenMainScreenUIUpdate;
            sectionTypeSelector.OnSectionTypeSelected += WhenSectionTypeSelected;
            ItemSelected += WhenItemSelected;
            resources = new Dictionary<string, string>();
            Clear();
            PoulateResourceCollection();
            LoadSelected();
        }
        private void WhenSectionTypeSelected(object sender, Type T)
        {
            if(!Visible) { Clear(); return; }
            GD.Print($"SectionSelector::WhenSectionTypeSelected() Selected[{Selected}] ItemCount[{ItemCount}]");
            PoulateResourceCollection();
            LoadSelected();
        }
        private void WhenItemSelected(long index)
        {
            LoadSelected();
        }
        private void LoadSelected()
        {
            if(ItemCount < 1){return;}
            if(Selected < 0) { Selected = 0; }
            GD.Print($"SectionSelector::LoadSelected() Loading[{Selected}]");
            string itemText = GetItemText(Selected == -1 ? 0 : Selected);
        }
        private void PoulateResourceCollection()
        {
            string typeName = sectionTypeSelector.GetItemText(sectionTypeSelector.Selected);
            GD.Print($"SectionSelector::PoulateResourceCollection() typeName[{typeName}] [{sectionTypeSelector.GetSelectedType()}]");
            resources = new Dictionary<string, string>();
            List<Resource> items = new List<Resource>();

            // Add default resources
            foreach (string file in DirAccess.GetFilesAt(screen.addon.MasterConfig.SectionResourcePathDefault))
            {
                if (file.Contains("tres"))
                {
                    Resource res = ResourceLoader.Load(screen.addon.MasterConfig.SectionResourcePathDefault + file);// + file.Replace(".tres", ""));
                    if (res is SectionResource) {
                        if(typeName == (res as SectionResource).sectionType)
                        {
                            items.Add(res); 
                        }
                    }
                }
            }
            // Add project section resources
            if (screen.addon.VerifySectionsFolder())
            {
                foreach (string file in DirAccess.GetFilesAt(screen.addon.MasterConfig.SectionResourcePath))
                {
                    if (file.Contains("tres"))
                    {
                        Resource res = ResourceLoader.Load(screen.addon.MasterConfig.SectionResourcePath + file);// + file.Replace(".tres", ""));
                        if (res is SectionResource)
                        {
                            if (typeName == (res as SectionResource).sectionType)
                            {
                                items.Add(res);
                            }
                        }
                    }
                }
            }
            // Build the selectable list
            if (items.Count > 0)
            {
                Clear();
                foreach (Resource res in items)
                {

                    if (res is SectionResource)
                    {
                        SectionResource room = res as SectionResource;
                        if (room.sectionName.Length < 1)
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

        internal SectionResource GetSelectedResource()
        {
            string sectionName = GetItemText(Selected == -1 ? 0 : Selected);
            if(!resources.ContainsKey(sectionName)){ return null; }
            Resource res = ResourceLoader.Load(resources[sectionName]);//
            return res as SectionResource;
        }
    }// EOF CLASS
}
