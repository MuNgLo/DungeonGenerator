using Godot;
using Munglo.DungeonGenerator;
using System;
namespace Munglo.DungeonGenerator.UI
{

    [Tool]
    public partial class PlacerNavigationBar : Control
    {
        [Export] SectionSelector sectionSelector;
        private MainScreen MS;
        private PlacerBar placerBar;
        private int index = 0;
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            MS = GetParent<MainScreen>();
            placerBar = GetParent().GetNode<PlacerBar>("PlacerBar");
            MS.OnMainScreenUIUpdate += WhenMainScreenUIUpdate;
            GetNode<TextureButton>("StepLeft").Pressed += WhenLeftPressed;
            GetNode<TextureButton>("StepRight").Pressed += WhenRightPressed;
            GetNode<TextureButton>("AddPlacer").Pressed += WhenAddPlacerPressed;
        }

        private void WhenMainScreenUIUpdate(object sender, EventArgs e)
        {
            if(!sectionSelector.Visible)
            {
                Hide();
                placerBar.Index = -1;
                return;
            }
            Show();
            GD.Print($"PlacerNavigationBar::WhenMainScreenUIUpdate()");
            SectionResource section = sectionSelector.GetSelectedResource();
            int nbPlacers = section.placers.Count;

            if (index < 0) { index = nbPlacers - 1; }
            if (index >= nbPlacers) { index = 0; }
            placerBar.Index = index;
            GetNode<RichTextLabel>("Counter").Text = nbPlacers.ToString();
        }

        /*
        private void WhenSelectionChange(object sender, EventArgs e)
        {
            //GD.Print($"PlacerNavigationBar::WhenSelectionChange()  MS is not null[{MS is not null}]");
            if (MS.SelectedSectionResource == null)
            {
                GetNode<RichTextLabel>("Counter").Text = $"[center]-[/center]";
                return;
            }
            int nbPlacers = MS.SelectedSectionResource.placers.Count;
            GetNode<RichTextLabel>("Counter").Text = $"[center]{nbPlacers}[/center]";
            index = 0;
            if (index < 0) { index = nbPlacers - 1; }
            if (index >= nbPlacers) { index = 0; }
            placerBar.Index = index;
        }
        */

        private void WhenAddPlacerPressed()
        {
            if (MS.SelectedSectionResource == null) { return; }
            SectionResource sectionResource = MS.SelectedSectionResource;
            sectionResource.placers.Add(new PlacerEntryResource());
            ResourceSaver.Save(sectionResource);
        }

 

        private void WhenRightPressed()
        {
            if (MS.SelectedSectionResource == null) { return; }
            int nbPlacers = MS.SelectedSectionResource.placers.Count;
            index++;
            if (index < 0) { index = nbPlacers - 1; }
            if (index >= nbPlacers) { index = 0; }
            placerBar.Index = index;
        }

        private void WhenLeftPressed()
        {
            if (MS.SelectedSectionResource == null) { return; }
            int nbPlacers = MS.SelectedSectionResource.placers.Count;
            index--;
            if (index < 0) { index = nbPlacers - 1; }
            if (index >= nbPlacers) { index = 0; }
            placerBar.Index = index;
        }
    }// EOF CLASS
}