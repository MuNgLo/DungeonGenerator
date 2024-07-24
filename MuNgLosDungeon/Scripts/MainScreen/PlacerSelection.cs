using Godot;
using System;

namespace Munglo.DungeonGenerator
{
    [Tool]
    internal partial class PlacerSelection : EditorResourcePicker
    {
        public override void _Ready()
        {
            ResourceChanged += WhenResourceChanged;
            ResourceSelected += WhenResouceSelected;
        }
        private void FocusInpsector()
        {
            if(EditedResource is null) { return; }
            EditorInterface.Singleton.InspectObject(EditedResource);
            ReleaseFocus();
        }

        private void WhenResouceSelected(Resource resource, bool inspect)
        {
            FocusInpsector();
        }

        private void WhenResourceChanged(Resource resource)
        {
            FocusInpsector();
        }
    }// EOF CLASS
}
