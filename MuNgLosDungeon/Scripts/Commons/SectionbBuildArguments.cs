using Munglo.DungeonGenerator;

namespace Munglo.DungeonGenerator
{
    public class SectionbBuildArguments
    {
        public RoomResource sectionDefinition;
        public MapData map;
        public MapPiece piece;
        public int sectionID;
        public GenerationSettingsResource cfg;
        public ulong[] sectionSeed;
        public ulong[] Seed => sectionSeed is null ? cfg.Seed : sectionSeed;
    }
}
