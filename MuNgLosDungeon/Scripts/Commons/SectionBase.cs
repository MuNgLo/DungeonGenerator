using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Munglo.DungeonGenerator
{
    public class SectionBase : ISection
    {
        #region fields
        /// <summary>
        /// The determenistic random number generator for this section
        /// </summary>
        private protected readonly PRNGMarsenneTwister rng;
        /// <summary>
        /// The parent map data this section belongs to
        /// </summary>
        private protected readonly MapData map;
        /// <summary>
        /// Assigned section index on creation
        /// </summary>
        private protected readonly int sectionIndex;
        /// <summary>
        /// What type of section is this. Room, Corridor, Void, Pantry?
        /// </summary>
        private protected readonly string sectionStyle = string.Empty; 
        /// <summary>
        /// The section name. Might not be unique in the Dungeon
        /// </summary>
        private protected readonly string sectionName = string.Empty;

        private protected List<MapPiece> pieces;
        private RoomProps props;
        public Dictionary<MapCoordinate, Dictionary<Vector3I, RoomProp>> PropGrids => props.grids;


        #endregion

        #region Needs some work
        private protected MapCoordinate coord;
        internal MAPDIRECTION orientation;
        public MapCoordinate Coord => coord;
        public ROOMCONNECTIONRESPONCE defaultConnectionResponses;

        ROOMCONNECTIONRESPONCE ISection.defaultConnectionResponses { get => defaultConnectionResponses; }

        public bool BridgeAllowed => defaultConnectionResponses.HasFlag(ROOMCONNECTIONRESPONCE.BRIDGE);
        public bool DoorAllowed => defaultConnectionResponses.HasFlag(ROOMCONNECTIONRESPONCE.DOOR);

        private protected int sizeZ;
        private protected int sizeX;
        private protected int sizeY;

        private protected int minX = 0;
        private protected int maxX = 0;
        private protected int minZ = 0;
        private protected int maxZ = 0;
        private protected int MinY => coord.y;
        private protected int MaxY => coord.y + sizeY;
        #endregion

        #region Properties
        public int SectionIndex => sectionIndex;
        public string SectionStyle => sectionStyle;
        public int TileCount => pieces.Count;
        public List<MapPiece> Pieces => pieces.Cast<MapPiece>().ToList();
        public int PropCount => TotalPropCount();

        #endregion



        public SectionBase(ulong[] seed, int index, string sectionStyle, string sectionName, MapData mapData)
        {
            sectionIndex = index;
            map = mapData;
            pieces = new List<MapPiece>();
            rng = new PRNGMarsenneTwister(seed);
            props = new RoomProps(this);
        }


        public virtual void Build()
        {
            throw new NotImplementedException();
        }

        public virtual void BuildProps()
        {
            throw new NotImplementedException();
        }

        public virtual bool AddOpening(MapCoordinate coord, MAPDIRECTION dir, bool wide, bool overrideLocked)
        {
            MapPiece piece = pieces.Find(p => p.Coord == coord);
            if (piece is null)
            {
                GD.PrintErr($"SectionBase::AddOpening() no piece on coord({coord}) part of room[{sectionIndex}]");
            }
            if (wide)
            {
                piece.AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = dir }, overrideLocked);
                MapPiece nb = piece.Neighbour(Dungeon.TwistRight(dir));
                nb.AssignWall(new KeyData() { key = PIECEKEYS.OCCUPIED, dir = dir }, overrideLocked);
                if (!piece.hasFloor)
                {
                    StairPlacer stairCase = new StairPlacer(this, piece, dir);
                    if (stairCase.isValid)
                    {
                        stairCase.Build();
                    }
                }
            }
            else
            {
                piece.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = dir }, overrideLocked);
                if (!piece.hasFloor)
                {

                    StairPlacer stairCase = new StairPlacer(this, piece, dir);
                    if (stairCase.isValid)
                    {
                        stairCase.Build();
                    }


                }
            }
            map.SavePiece(piece);
            return true;
        }

        public void Save()
        {
            foreach (MapPiece piece in pieces)
            {
                map.SavePiece(piece);
            }
        }
        /// <summary>
        /// Puts wall,floor and ceiling keys against other sections
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void SealSection()
        {
            foreach (MapPiece piece in pieces)
            {
                // Floor
                if (!Pieces.Exists(p => p.Coord == piece.Coord + MAPDIRECTION.DOWN)) { piece.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = orientation, variantID = 0 }; }
                // Ceiling
                if (!Pieces.Exists(p => p.Coord == piece.Coord + MAPDIRECTION.UP)) { piece.keyCeiling = new KeyData() { key = PIECEKEYS.C, dir = orientation, variantID = 0 }; }
                // Walls
                for (int i = 1; i < 5; i++)
                {
                    if (!Pieces.Exists(p => p.Coord == piece.Coord + (MAPDIRECTION)i)) { piece.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = (MAPDIRECTION)i, variantID = 0 }, true); }
                }
            }
        }
        public List<MapPiece> GetWallPieces(int floor, bool includeCorners = false)
        {
            // Confirmed
            List<MapPiece> candidates = pieces.FindAll(p => p.sectionfloor == floor && p.HasNorthWall);

            candidates.AddRange(pieces.FindAll(p => p.sectionfloor == floor && p.HasEastWall && !p.HasNorthWall));
            candidates.AddRange(pieces.FindAll(p => p.sectionfloor == floor && p.HasSouthWall && !p.HasNorthWall && !p.HasEastWall));
            candidates.AddRange(pieces.FindAll(p => p.sectionfloor == floor && p.HasWestWall && !p.HasNorthWall && !p.HasEastWall && !p.HasSouthWall));
            if (!includeCorners)
            {
                int count = 0;
                count += candidates.RemoveAll(p => p.HasNorthWall && p.HasEastWall);
                count += candidates.RemoveAll(p => p.HasEastWall && p.HasSouthWall);
                count += candidates.RemoveAll(p => p.HasSouthWall && p.HasWestWall);
                count += candidates.RemoveAll(p => p.HasWestWall && p.HasNorthWall);
            }
            return candidates;
        }


        private protected void SetMinMaxCoord()
        {
            int Xoffset = 0;
            int Zoffset = 0;
            if (sizeX % 2 != 0) { Xoffset = 1; }
            if (sizeZ % 2 != 0) { Zoffset = 1; }
            switch (orientation)
            {
                case MAPDIRECTION.NORTH:
                    minX = coord.x - (int)(sizeX * 0.5f) + 1 - Xoffset;
                    maxX = coord.x + (int)(sizeX * 0.5f);
                    minZ = coord.z - sizeZ + 1;
                    maxZ = coord.z;
                    break;
                case MAPDIRECTION.SOUTH:
                    minX = coord.x - (int)(sizeZ * 0.5f);
                    maxX = coord.x + (int)(sizeZ * 0.5f) - 1 + Xoffset;
                    minZ = coord.z;
                    maxZ = coord.z + sizeZ - 1;
                    break;
                case MAPDIRECTION.EAST:
                    minX = coord.x;
                    maxX = coord.x + sizeX - 1;
                    minZ = coord.z - (int)(sizeZ * 0.5f) + 1 - Zoffset;
                    maxZ = coord.z + (int)(sizeZ * 0.5f);
                    break;
                case MAPDIRECTION.WEST:
                    minX = coord.x - sizeX + 1;
                    maxX = coord.x;
                    minZ = coord.z - (int)(sizeZ * 0.5f) + 1 - Zoffset;
                    maxZ = coord.z + (int)(sizeZ * 0.5f);
                    break;
            }
        }
        /// <summary>
        /// Todo rewrite this so it finds the furthest corner pieces and calculates which ones is closest to the center of them all
        /// Also add parameter so it can be done per section floor
        /// </summary>
        /// <returns></returns>
        private protected MapPiece GetCenterPiece()
        {
            MapPiece centerOfStartLine = GetCenterOfRow(pieces[0], orientation);
            MapPiece centerOfRoom = GetCenterOfRow(centerOfStartLine, Dungeon.TwistLeft(orientation));
            return centerOfRoom;
        }
        private protected MapPiece GetCenterOfRow(MapPiece piece, MAPDIRECTION dir)
        {
            List<MapPiece> negP = new List<MapPiece>();
            List<MapPiece> posP = new List<MapPiece>() { piece };
            int breaker = 100;
            while (pieces.Exists(p => p.Coord == piece.Coord + dir))
            {
                piece = pieces.Find(p => p.Coord == piece.Coord + dir);
                posP.Add(piece);
                breaker--; if (breaker < 1)
                {
                    break;
                }
            }

            breaker = 100;
            dir = Dungeon.Flip(dir);
            piece = posP.First();
            while (pieces.Exists(p => p.Coord == piece.Coord + dir))
            {
                piece = pieces.Find(p => p.Coord == piece.Coord + dir);
                negP.Add(piece);
                breaker--; if (breaker < 1)
                {
                    GD.Print($"RoomBase::GetCenterOfRow() negP iteration exceeded allowed part of eternity");
                    break;
                }
            }


            if (negP.Count > 0) { negP.Reverse(); }

            negP.AddRange(posP);

            if (negP.Count > 1)
            {
                return negP[Mathf.FloorToInt(negP.Count * 0.5)];
            }
            return posP[0];
        }


        public virtual void AddProp(MapCoordinate coord, RoomProp pData)
        {
            props.Add(coord, pData);
        }

        public virtual bool AddPropOnRandomTile(KeyData keyData, out MapPiece pick)
        {
            throw new NotImplementedException();
        }

        public virtual MapPiece GetRandomPiece()
        {
            return pieces[rng.Next(0, pieces.Count)];
        }

        public virtual MapPiece GetRandomFloor()
        {
            if (!pieces.Exists(p => p.hasFloor))
            {
                GD.PushError($"SectionBase::GetRandomFloor() Section[{sectionIndex}] has no Floors!");
                return null;
            }
            MapPiece pick = pieces[rng.Next(0, pieces.Count)];
            while (!pick.hasFloor) { pick = pieces[rng.Next(0, pieces.Count)]; }
            return pick;
        }

        public virtual void PunchBackDoor()
        {
            throw new NotImplementedException();
        }

        public virtual int TotalPropCount()
        {
            int count = 0;
            foreach (var piece in pieces) { count += piece.Props.Count; }
            return count + PropGrids.Count;
        }
    }
}
