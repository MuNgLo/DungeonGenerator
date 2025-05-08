using Godot;
namespace Munglo.DungeonGenerator
{
    /// <summary>
    ///  Addon's internal settings for remembering picks and things
    ///  Should never be instanced again
    /// </summary>
    [Tool, GlobalClass]
    public partial class AddonSettingsResource : DungeonAddonResource
    {
        [Export] public string lastUsedProfile = "res://addons/MDunGen/Config/def_profile.tres";
        [Export] public string ProjectResourcePath = string.Empty;

        public string SectionResourcePathDefault = "res://addons/MDunGen/Config/Sections/";
        public string SectionResourcePath => ProjectResourcePath +"Sections/";



        public string defaultBiome = "res://addons/MDunGen/Config/def_biome.tres";
        public string defaultSettings = "res://addons/MDunGen/Config/def_settings.tres";

        public string defaultStartRoom = "res://addons/MDunGen/Config/Rooms/DefaultStartRoom.tres";
        public string defaultStandardRoom = "res://addons/MDunGen/Config/Rooms/DefaultStandardRoom.tres";

        [ExportCategory("Visual Floors")]
        [Export] public int visibleFloorStart = 0;
        [Export] public int maxVisibleFloors = 5;
        public int visibleFloorEnd => visibleFloorStart + maxVisibleFloors - 1;


        [ExportGroup("Show")]
        [Export] public bool showFloors = true;
        [Export] public bool showWalls = true;
        [Export] public bool showCeilings = true;
        [Export] public bool showExtras = true;

        [ExportGroup("Passes")]
        [Export] public bool pathingPass = true;
    }
}