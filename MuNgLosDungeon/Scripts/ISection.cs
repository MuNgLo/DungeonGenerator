using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Munglo.DungeonGenerator
{
    public interface ISection
    {
        /// <summary>
        /// Assigned section index on creation
        /// </summary>
        public int SectionIndex { get; }
        /// <summary>
        /// What type of section is this. Room, Corridor, Void, Pantry?
        /// </summary>
        public string SectionStyle { get; }
        /// <summary>
        /// Total count of all map pieces in the section (include empty?)
        /// </summary>
        public int TileCount { get; }
        
        /// <summary>
        /// TODO decide if this is all props or just the section inner props
        /// </summary>
        public int PropCount { get; }
        /// <summary>
        /// All the pieces in the section
        /// Should only be accessed when we create visuals
        /// </summary>
        public List<MapPiece> Pieces { get; }
        /// <summary>
        /// Grows the section into the current MapData
        /// </summary>
        public void Build();

        /// <summary>
        /// Write all the section's pieces to the mapData instance
        /// </summary>
        public void Save();

        /// <summary>
        /// Generate the inner props of the section
        /// </summary>
        public void BuildProps();

        public bool AddOpening(MapCoordinate coord, MAPDIRECTION dir, bool wide, bool overrideLocked);
        /// <summary>
        /// Add inner prop to section
        /// </summary>
        /// <param name="rp"></param>
        /// <param name="pData"></param>
        public void AddProp(MapCoordinate coord, RoomProp pData);
        public bool AddPropOnRandomTile(KeyData keyData, out MapPiece pick);
        public MapPiece GetRandomPiece();
        public MapPiece GetRandomFloor();
        public void PunchBackDoor();

        public int TotalPropCount();


        public MapCoordinate Coord { get; }
        public ROOMCONNECTIONRESPONCE defaultConnectionResponses { get; }
        public bool BridgeAllowed { get; }
        public bool DoorAllowed { get; }
        public List<MapPiece> GetWallPieces(int floor, bool includeCorners = false);

    }// EOF INTERFACE
}
