using Godot;
namespace Munglo.DungeonGenerator
{
    [GlobalClass, Tool]
    public partial class BiomeEntry : Resource
    {
        [Export] public PIECEKEYS key { get; set; } = PIECEKEYS.NONE;
        /// <summary>
        /// DONT access this directly USe the GetResource
        /// </summary>
        [Export] public Resource[] resources { get; set; } = null;
        public Resource GetResource(int index)
        { 
            if(index < 0 || index >= resources.Length)
            {
                GD.PrintErr($"BiomeEntry::GetResouce([{index}]) Variant index out of range for [{key}]. Falling back on default index 0.");
                index = 0;
            };
            if (resources[index] == null)
            {
                GD.PrintErr($"BiomeEntry::GetResouce([{index}]) Resource entry is NULL for [{key}]." + (index != 0 ? "Falling back on default index 0." : ""));
                if (index != 0) { return GetResource(0); }
                return null;
            }
            return resources[index];
        }
    }
}
