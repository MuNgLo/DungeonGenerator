using Godot;
using Godot.Collections;
using Munglo.DungeonGenerator;
using System;
using System.Reflection;
namespace Munglo.DungeonGenerator.UI
{
	[Tool]
	public partial class PlacerBar : Control
	{
		private MainScreen MS;
		private int index = 0;
		// Called when the node enters the scene tree for the first time.
		public int Index { set { index = value; UpdateBar(this,null); } }
		public override void _Ready()
		{
			MS = GetParent<MainScreen>();
			MS.OnSelectionChanged += WhenSelectionChanged;
			MS.OnMainScreenUIUpdate += UpdateBar;
			GetNode<TextureButton>("PlacerNameBtn").Pressed += WhenNamePressed;
            GetNode<Button>("CheckButton").Toggled += WhenCheckButtonToggled;
        }

		private void WhenCheckButtonToggled(bool state)
		{
			if (MS.SelectedSectionResource == null) { return; }
			SectionResource section = MS.SelectedSectionResource;
			if (section.placers.Count >= index)
			{
                section.placers[index].active = state;
			}
		}

        private void WhenNamePressed()
		{
            if (MS.SelectedSectionResource == null) { return; }
            SectionResource section = MS.SelectedSectionResource;
            if (section.placers.Count >= index)
			{
				EditorInterface.Singleton.InspectObject(section.placers[index]);
			}
		}

		private void WhenSelectionChanged(object sender, EventArgs e)
		{
			//GD.Print("PlacerBar::WhenSelectionChanged");
			index = 0;
			UpdateBar(this,null);
		}

		private void UpdateBar(object sender, EventArgs e)
		{
            GD.Print($"PlacerBar::UpdateBar({index})");
            if (MS.SelectedSectionResource == null) { return; }
            SectionResource section = MS.SelectedSectionResource;
			if (section.placers.Count > 0)
			{
				PlacerEntryResource entry = section.placers[index];
				IPlacer placer = ResourceLoader.Load(entry.ResourcePath) as IPlacer;

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
		}

	}// EOF CLASS
}