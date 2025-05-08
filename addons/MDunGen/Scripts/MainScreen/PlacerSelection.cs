using Godot;
using System;

namespace Munglo.DungeonGenerator.UI
{
    [Tool]
    internal partial class PlacerSelection : EditorResourcePicker
    {
        public override void _Ready()
        {
            ResourceChanged += WhenResourceChanged;
            ResourceSelected += WhenResouceSelected;
        }
        private void FocusInspector()
        {
            if(EditedResource is null) { return; }
            EditorInterface.Singleton.InspectObject(EditedResource);
            ReleaseFocus();
        }

        private void WhenResouceSelected(Resource resource, bool inspect)
        {
            FocusInspector();
        }

        private void WhenResourceChanged(Resource resource)
        {
            FocusInspector();
        }
    }// EOF CLASS
}
