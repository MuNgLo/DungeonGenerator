using Godot;
using Godot.Collections;
using Munglo.DungeonGenerator;
using System;
using System.Reflection;
namespace Munglo.DungeonGenerator.UI
{
	/// <summary>
    /// This class keeps the placer bar updated. the updates are driven by the PlacerNavigationBar
    /// </summary>
	[Tool]
	public partial class PlacerBar : Control
	{
        [Export] SectionSelector sectionSelector;

		private BottomScreen BS;
		private int index = 0;
		// Called when the node enters the scene tree for the first time.
		//public int Index { set { index = value; IndexChanged(); } }
		public override void _Ready()
		{
			BS = GetParent<BottomScreen>();
			//GetNode<TextureButton>("PlacerNameBtn").Pressed += WhenNamePressed;
            //GetNode<Button>("CheckButton").Toggled += WhenCheckButtonToggled;
        }
/*
		private void WhenCheckButtonToggled(bool state)
		{
			if (BS.SelectedSectionResource == null) { return; }
			SectionResource section = BS.SelectedSectionResource;
			if (section.placers.Count >= index)
			{
                section.placers[index].active = state;
			}
		}

        private void WhenNamePressed()
		{
            if (BS.SelectedSectionResource == null) { return; }
            SectionResource section = BS.SelectedSectionResource;
            if (section.placers.Count >= index)
			{
				EditorInterface.Singleton.InspectObject(section.placers[index]);
			}
		}
		private void IndexChanged()
		{
            GD.Print($"PlacerBar::IndexChanged({index})");
            if (index < 0) { Hide(); return; }
            SectionResource section = sectionSelector.GetSelectedResource();
			if (section.placers.Count > 0)
			{
				PlacerEntryResource entry = section.placers[index];
				
				//IPlacer placer = ResourceLoader.Load(entry.ResourcePath) as IPlacer;

				GetNode<RichTextLabel>("Index").Text = $"[center]{index}#[/center]";
				GetNode<Button>("CheckButton").SetPressedNoSignal(entry.active);
				GetNode<RichTextLabel>("PlacerName").Text = $"{entry.Name}";
				GetNode<RichTextLabel>("min").Text = $"[right]{entry.count}[/right]";
				
				GetNode<RichTextLabel>("max").Text = $"[left][/left]";
				GetNode<RichTextLabel>("Chance").Text = $"[center][/center]";
				Show();
			}
			else
			{
				Hide();
			}
		}*/

	}// EOF CLASS
}