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
        [Export] public string lastUsedProfile = "res://addons/MuNgLosDungeon/Config/def_profile.tres";
        [Export] public string ProjectResourcePath = string.Empty;

        public string SectionResourcePathDefault = "res://addons/MuNgLosDungeon/Config/Sections/";
        public string SectionResourcePath => ProjectResourcePath +"Sections/";



        public string defaultBiome = "res://addons/MuNgLosDungeon/Config/def_biome.tres";
        public string defaultSettings = "res://addons/MuNgLosDungeon/Config/def_settings.tres";

        public string defaultStartRoom = "res://addons/MuNgLosDungeon/Config/Rooms/DefaultStartRoom.tres";
        public string defaultStandardRoom = "res://addons/MuNgLosDungeon/Config/Rooms/DefaultStandardRoom.tres";

        [ExportCategory("Visual Floors")]
        [Export] public int visibleFloorStart = 0;
        [Export] public int maxVisibleFloors = 5;
        public int visibleFloorEnd => visibleFloorStart + maxVisibleFloors - 1;

        /// <summary>
        /// Pass flags will only be used as debugging flags
        /// </summary>
        //[ExportGroup("Passes")]
        //[Export] public bool debugPass = true;
        //[Export] public bool clearEmpties = true;
        //[Export] public bool floorPass = true;
        //[Export] public bool wallPass = true;
        //[Export] public bool ceilingPass = true;
        //[Export] public bool corridorPass = true;
        //[Export] public bool propPass = true;
        //[Export] public bool roomPass = true;
        //[Export] public bool waterPass = true;
    }
}