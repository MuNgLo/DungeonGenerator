using Godot;
using System;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace Munglo.DungeonGenerator
{
    [Tool, GlobalClass]
    public partial class DropTableResource : DungeonAddonResource
    {
        [Export] private DropTableEntryResource[] entries;
        public DropTableEntryResource[] forcedEntries => entries.Where(p=>p.forcedRoll == true).ToArray();
        public DropTableEntryResource[] rngEntries => entries.Where(p=>p.forcedRoll == false).ToArray();

        public void CalculateDropChances()
        {
            int total = 0;
            foreach ( var entry in rngEntries) { total += entry.weight; }
            foreach ( var entry in rngEntries) { entry.Chance = Mathf.FloorToInt(entry.weight / total); }
        }

        internal bool GetRandomAsset(out PackedScene asset, out int count)
        {
            asset = null;
            count = 1;
            int total = 0;
            foreach (var entry in rngEntries) { total += entry.weight; }
            foreach (var entry in rngEntries) { total -= entry.weight; if (total < 1) { asset = entry.asset; count = entry.assetCount; return true; } }
            return false;
        }
    }// EOF CLASS
}
