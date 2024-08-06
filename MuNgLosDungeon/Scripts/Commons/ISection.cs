using Godot;
using Godot.Collections;
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
        /// The name of the section. That should be unique for that resource
        /// </summary>
        public string SectionName { get; }
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
        /// Generate the connections between sections
        /// </summary>
        public void BuildConnections();

        /// <summary>
        /// Write all the section's pieces to the mapData instance
        /// </summary>
        public void Save();

        public bool AddOpening(MapCoordinate coord, MAPDIRECTION dir, bool wide, bool overrideLocked);
        /// <summary>
        /// Add inner prop to section
        /// </summary>
        /// <param name="rp"></param>
        /// <param name="pData"></param>
        public void AddProp(SectionProp pData);
        public bool AddPropOnRandomTile(KeyData keyData, out MapPiece pick);



        /// <summary>
        /// Resolve the worldPosition to closest mapCoordinate and return true if it is part of section
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool IsInside(Vector3 worldPosition);

        public List<SectionConnection> Connections { get; }

        public List<SectionProp> Props { get; }
        public MapPiece GetRandomPiece();
        public MapPiece GetRandomFloor();
        public void PunchBackDoor();

        public MapCoordinate Coord { get; }
        public ROOMCONNECTIONRESPONCE defaultConnectionResponses { get; }
        public bool BridgeAllowed { get; }
        public bool DoorAllowed { get; }
        public List<MapPiece> GetWallPieces(int floor, bool includeCorners = false);


        /// <summary>
        /// Puts wall,floor and ceiling keys against other sections
        /// </summary>
        public void SealSection(int wallVariant = 0, int floorVariant = 0, int ceilingVariant = 0);


        public void AddConnection(int otherSectionIndex, MAPDIRECTION dir, MapCoordinate coord, bool overrideLocked);
        /// <summary>
        /// Assign placers to the section. If placersOverride is valid it will override the SectionResource placers collection
        /// </summary>
        /// <param name="sectionDef"></param>
        /// <param name="placersOverride"></param>
        void AssignPlacer(SectionResource sectionDef, Array<PlacerEntryResource> placersOverride);

        public int ConnectionCount { get; }

        public Node3D SectionContainer { get; set; }
        public MapCoordinate MaxCoord { get; }
        public MapCoordinate MinCoord { get; }
        public PRNGMarsenneTwister RNG { get; }
        public Array<PlacerEntryResource> Placers { get; }
    }// EOF INTERFACE
}
