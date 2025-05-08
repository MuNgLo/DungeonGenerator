using Godot;
using System;
namespace Munglo.DungeonGenerator.UI
{
    [Tool]
    public partial class DungeonViewerToolbar : HBoxContainer
    {
        private MainScreen screen;

        [Export] private Button btnModeToggle;
        [Export] private Button btnClear;
        [Export] private Button btnBuild;
        [Export] private Button btnRandomToggle;
        [Export] private MenuButton btnView;
        [Export] private SectionTypeListButton btnSectionTypeList;
        [Export] private SectionSelector btnSectionSelector;

        public ProfileResource Profile => screen.addon.Profile;

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            screen = GetParent<MainScreen>();
            screen.OnMainScreenUIUpdate += UpdateUI;
            btnModeToggle.Pressed += WhenModeTogglePressed;
            btnBuild.Pressed += WhenTBBuildPressed;
            btnClear.Pressed += WhenMSClearPressed;
            btnRandomToggle.Pressed += WhenMSRNGSeedPressed;
            btnView.GetPopup().IdPressed += WhenMSShowChanged;
        }

        private void UpdateUI(object sender, EventArgs e)
        {
            // Update the UI to current states
            switch (screen.addon.Mode)
            {
                case VIEWERMODE.SECTION:
                    btnModeToggle.Text = "SectionMode";
                    btnSectionSelector.Show();
                    btnSectionTypeList.Show();
                    break;
                case VIEWERMODE.DUNGEON:
                default:
                    btnModeToggle.Text = "DungeonMode";
                    btnSectionSelector.Hide();
                    btnSectionTypeList.Hide();
                    break;
            }

            if (!Profile.useRandomSeed)
            {
                if (Profile.useRandomSeed)
                {
                    btnRandomToggle.Text = "Random";
                }
                else
                {
                    btnRandomToggle.Text = "Seeded";
                }
            }
            PopupMenu pop = btnView.GetPopup();
            pop.HideOnCheckableItemSelection = false;
            pop.HideOnItemSelection = false;
            pop.HideOnStateItemSelection = false;
            pop.SetItemChecked(0, screen.addon.MasterConfig.showFloors);
            pop.SetItemChecked(1, screen.addon.MasterConfig.showWalls);
            pop.SetItemChecked(2, screen.addon.MasterConfig.showCeilings);
            pop.SetItemChecked(3, screen.addon.MasterConfig.pathingPass);
            pop.SetItemChecked(4, screen.addon.MasterConfig.showExtras);
        }

        private void WhenModeTogglePressed()
        {
            screen.addon.ChangeMode();
        }

        public override void _ExitTree()
        {
            btnBuild.Pressed -= WhenTBBuildPressed;
            btnClear.Pressed -= WhenMSClearPressed;
            btnRandomToggle.Pressed -= WhenMSRNGSeedPressed;
            btnView.GetPopup().IdPressed -= WhenMSShowChanged;
        }

        private void WhenTBBuildPressed()
        {
            GD.Print("DungeonViewerToolbar::WhenTBBuildPressed()");
            screen.WhenClearPressed();
            if (Profile.useRandomSeed)
            {
                Profile.settings.seed1 = GD.RandRange(1111, 9999);
                Profile.settings.seed2 = GD.RandRange(1111, 9999);
                Profile.settings.seed3 = GD.RandRange(1111, 9999);
                Profile.settings.seed4 = GD.RandRange(1111, 9999);
            }
            switch (screen.addon.Mode)
            {
                case VIEWERMODE.SECTION:

                    string sectionTypeName = btnSectionTypeList.GetItemText(btnSectionTypeList.Selected);
                    SectionResource sectionDef = btnSectionSelector.GetSelectedResource();
                    screen.GenerateSection(sectionTypeName, sectionDef, Profile.settings, Profile.biome);
                    break;
                default:
                case VIEWERMODE.DUNGEON:
                    screen.GenerateDungeon(Profile.settings, Profile.biome);
                    break;
            }
            btnBuild.ReleaseFocus();
            screen.RaiseUpdateUI();
        }
        private void WhenMSClearPressed()
        {
            GD.Print("DungeonViewerToolbar::WhenMSClearPressed()");
            screen.WhenClearPressed();
            btnClear.ReleaseFocus();
        }
        private void WhenMSRNGSeedPressed()
        {
            GD.Print("DungeonViewerToolbar::WhenMSRNGSeedPressed()");
            Profile.useRandomSeed = !Profile.useRandomSeed;
            ResourceSaver.Save(Profile);
            if (Profile.useRandomSeed)
            {
                btnRandomToggle.Text = "Random";
            }
            else
            {
                btnRandomToggle.Text = "Seeded";
            }
            screen.RaiseNotification("Generate " + (Profile.useRandomSeed ? "Random" : "Seeded"));
            btnRandomToggle.ReleaseFocus();
        }
        private void WhenMSShowChanged(long id)
        {
            GD.Print($"DungeonViewerToolbar::WhenMSShowChanged({id})");
            PopupMenu pop = btnView.GetPopup();
            int index = pop.GetItemIndex((int)id);
            pop.SetItemChecked(index, !pop.IsItemChecked(index));
            switch (id)
            {
                case 0:
                    screen.addon.MasterConfig.showFloors = pop.IsItemChecked(index);
                    ResourceSaver.Save(Profile.settings);
                    break;
                case 1:
                    screen.addon.MasterConfig.showWalls = pop.IsItemChecked(index);
                    ResourceSaver.Save(Profile.settings);
                    break;
                case 2:
                    screen.addon.MasterConfig.showCeilings = pop.IsItemChecked(index);
                    ResourceSaver.Save(Profile.settings);
                    break;
                case 4:
                    screen.addon.MasterConfig.pathingPass = pop.IsItemChecked(index);
                    ResourceSaver.Save(Profile.settings);
                    break;
                case 5:
                    screen.addon.MasterConfig.showExtras = pop.IsItemChecked(index);
                    ResourceSaver.Save(Profile.settings);
                    break;
                default:
                    break;
            }
            btnView.ReleaseFocus();
            screen.ReDrawDungeon();
        }
    }//EOF CLASS
}