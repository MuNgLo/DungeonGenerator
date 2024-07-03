using Godot;
namespace Munglo.DungeonGenerator
{
    [GlobalClass, Tool]
    public partial class AddonSettings : DungeonAddonResource
    {
        [Export] public string lastUsedProfile = "res://addons/MuNgLosDungeon/Config/def_profile.tres";
        public string defaultBiome = "res://addons/MuNgLosDungeon/Config/def_biome.tres";
        public string defaultSettings = "res://addons/MuNgLosDungeon/Config/def_settings.tres";

        public string defaultStartRoom = "res://addons/MuNgLosDungeon/Config/Rooms/DefaultStartRoom.tres";
        public string defaultStandardRoom = "res://addons/MuNgLosDungeon/Config/Rooms/DefaultStandardRoom.tres";
    }
}