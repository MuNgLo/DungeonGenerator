using Godot;
namespace Munglo.DungeonGenerator
{
    /// <summary>
    ///  Addon's internal settings for remembering picks and things
    ///  Should never be instanced again
    /// </summary>
    [Tool]
    public partial class AddonSettings : DungeonAddonResource
    {
        [Export] public string lastUsedProfile = "res://addons/MuNgLosDungeon/Config/def_profile.tres";
        [Export] public string ProjectResourcePath = string.Empty;
        
        public string defaultBiome = "res://addons/MuNgLosDungeon/Config/def_biome.tres";
        public string defaultSettings = "res://addons/MuNgLosDungeon/Config/def_settings.tres";

        public string defaultStartRoom = "res://addons/MuNgLosDungeon/Config/Rooms/DefaultStartRoom.tres";
        public string defaultStandardRoom = "res://addons/MuNgLosDungeon/Config/Rooms/DefaultStandardRoom.tres";

        [ExportCategory("Visual Floors")]
        [Export] public int visibleFloorStart = 0;
        [Export] public int maxVisibleFloors = 5;
        public int visibleFloorEnd => visibleFloorStart + maxVisibleFloors - 1;

    }
}