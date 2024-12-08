using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Munglo.DungeonGenerator
{
    /// <summary>
    /// The data representing a single location in the map
    /// </summary>
    public class MapPiece
    {
        #region fixed never change fields and properties
        private protected MapData map;
        private MAPPIECESTATE state = MAPPIECESTATE.UNUSED;
        private MAPDIRECTION orientation = MAPDIRECTION.ANY;
        internal MAPDIRECTION Orientation { get => orientation; set => SetOrientaion(value); }
        internal MAPPIECESTATE State { get => state; set => state = value; }
        public bool hasStairs = false;
        public bool isBridge = false;
        /*internal MapPiece NeighbourNorth => map.GetPiece(Coord.StepNorth);
        internal MapPiece NeighbourEast => map.GetPiece(Coord.StepEast);
        internal MapPiece NeighbourSouth => map.GetPiece(Coord.StepSouth);
        internal MapPiece NeighbourWest => map.GetPiece(Coord.StepWest);
        internal MapPiece NeighbourUp => map.GetPiece(Coord.StepUp);
        internal MapPiece NeighbourDown => map.GetPiece(Coord.StepDown);
*/
        private protected int sectionIndex = -1;
        internal int SectionIndex { get => sectionIndex; set => sectionIndex = value; }
        /// <summary>
        /// The Section instance the piece is part of
        /// </summary>
        public ISection Section
        {
            get
            {
                if (sectionIndex < 0) {
                    GD.PushError($"MapPiece[{coord}] has unset sectionIndex! Defaulting to 0 but this need fixing!");
                    SetError(true);
                    sectionIndex = 0;
                }
                if (sectionIndex >= map.Sections.Count)
                {
                    GD.PushError($"MapPiece[{coord}] sectionIndex[{sectionIndex}] to high! Count[{map.Sections.Count}] Defaulting to last section but this need fixing!");
                    SetError(true);
                    sectionIndex = map.Sections.Count - 1;
                }
                return map.Sections[sectionIndex];
            }
        }
        #region Wall Flags
        private protected WALLS walls = new WALLS();
        internal WALLS Walls => walls;
        internal bool HasNorthWall => walls.HasFlag(WALLS.N);
        internal bool HasEastWall => walls.HasFlag(WALLS.E);
        internal bool HasSouthWall => walls.HasFlag(WALLS.S);
        internal bool HasWestWall => walls.HasFlag(WALLS.W);
        #endregion

        /// <summary>
        /// Floor relative to the section's location
        /// </summary>
        internal int sectionfloor = -1;

        #region Mesh keys
        /// <summary>
        /// Floor mesh
        /// </summary>
        internal KeyData keyFloor = new() { key = PIECEKEYS.NONE, dir = MAPDIRECTION.ANY };
  
        /// <summary>
        /// Ceiling mesh
        /// </summary>
        internal KeyData keyCeiling = new() { key = PIECEKEYS.NONE, dir = MAPDIRECTION.ANY };
        /// <summary>
        /// Stored as Dict so we can walk a direction until we hit a wall
        /// </summary>
        private Dictionary<MAPDIRECTION, KeyData> wallkeys = new Dictionary<MAPDIRECTION, KeyData>() {
            { MAPDIRECTION.NORTH, new KeyData { key = PIECEKEYS.NONE, dir = MAPDIRECTION.NORTH } },
            { MAPDIRECTION.EAST, new KeyData { key = PIECEKEYS.NONE, dir = MAPDIRECTION.EAST } },
            { MAPDIRECTION.SOUTH, new KeyData { key = PIECEKEYS.NONE, dir = MAPDIRECTION.SOUTH } },
            { MAPDIRECTION.WEST, new KeyData { key = PIECEKEYS.NONE, dir = MAPDIRECTION.WEST } }
        };
        internal KeyData WallKeyNorth { get => wallkeys[MAPDIRECTION.NORTH]; private set => wallkeys[MAPDIRECTION.NORTH] = value; }
        internal KeyData WallKeyEast { get => wallkeys[MAPDIRECTION.EAST]; private set => wallkeys[MAPDIRECTION.EAST] = value; }
        internal KeyData WallKeySouth { get => wallkeys[MAPDIRECTION.SOUTH]; private set => wallkeys[MAPDIRECTION.SOUTH] = value; }
        internal KeyData WallKeyWest { get => wallkeys[MAPDIRECTION.WEST]; private set => wallkeys[MAPDIRECTION.WEST] = value; }

        /// <summary>
        /// Debug meshes
        /// </summary>
        private protected List<KeyData> debug = new();
        internal List<KeyData> Debug => debug;
        #endregion

        /// <summary>
        /// checks for any props, floor, ceiling and walls.
        /// </summary>
        public bool isEmpty => CheckIfEmpty();
        public bool hasCieling => keyCeiling.key != PIECEKEYS.NONE;
        public bool hasFloor => keyFloor.key != PIECEKEYS.NONE;
        #endregion

        private void SetOrientaion(MAPDIRECTION value)
        {
            orientation = value;
        }

        /// <summary>
        /// All Below needs goign through
        /// </summary>
        internal WATERAMOUNT water = WATERAMOUNT.NONE;



        private protected MapCoordinate coord;
        internal string CoordString => CoordStringer();
        internal MapCoordinate Coord => coord;

        #region Constructors
        public MapPiece(MapData mapData, MapCoordinate coordinates, MAPDIRECTION dir, MAPPIECESTATE s)
        {
            map = mapData;
            coord= coordinates;
            state = s;
            orientation = dir;
            walls = new WALLS();
        }
        public MapPiece(MapData mapData, MapCoordinate coordinates)
        {
            map = mapData;
            coord = coordinates;
            state = MAPPIECESTATE.UNUSED;
            walls = new WALLS();
        }
        public MapPiece(MapData mapData)
        {
            map = mapData;
            coord = MapCoordinate.Down * 10;
            state = MAPPIECESTATE.ERROR;
            walls = new WALLS();
        }
        #endregion

        internal bool HasWall(MAPDIRECTION dir)
        {
            switch (dir)
            {
                case MAPDIRECTION.EAST:
                    return HasEastWall;
                case MAPDIRECTION.SOUTH:
                    return HasSouthWall;
                case MAPDIRECTION.WEST:
                    return HasWestWall;
                case MAPDIRECTION.NORTH:
                    return HasNorthWall;
            }
            return false;
        }

        /// <summary>
        /// If the piece is part of room and marked as wall it returns the direction to outside the room
        /// </summary>
        /// <returns></returns>
        public MAPDIRECTION OutsideWallDirection()
        {
            if (HasEastWall)
            {
                return MAPDIRECTION.EAST;
            }
            else if (HasNorthWall)
            {
                return MAPDIRECTION.NORTH;
            }
            else if (HasSouthWall)
            {
                return MAPDIRECTION.SOUTH;
            }
            else if (HasWestWall)
            {
                return MAPDIRECTION.WEST;
            }
            return MAPDIRECTION.ANY;
        }

        internal bool IsCorner(MAPDIRECTION dir)
        {
            if (!HasWall(dir)) { return false; }
            if (HasWall(Dungeon.TwistLeft(dir)) || HasWall(Dungeon.TwistRight(dir))) { return true; }
            return false;
        }


        #region Fixed and completed Commented up and done
        /// <summary>
        /// Will return the format [{X}.{Y}.{Z}]
        /// </summary>
        /// <returns></returns>
        private string CoordStringer()
        {
            return $"[{Coord.x}.{Coord.y}.{Coord.z}]";
        }
  
        /// <summary>
        /// Removes all mathching keys from props. Then add one
        /// </summary>
        /// <param name="kData"></param>
        internal void AddProp(KeyData kData)
        {
            Section.AddProp(new SectionProp(kData, (Vector3I)Dungeon.GlobalPosition(this)));
        }
        /// <summary>
        /// Removes all mathching keys from props. Then add one
        /// </summary>
        /// <param name="kData"></param>
        internal void AddDebug(KeyData kData)
        {
            RemoveDebug(kData);
            debug.Add(kData);
        }
        /// <summary>
        /// set or removes the faulty key from debug keys
        /// </summary>
        /// <param name="isError"></param>
        internal void SetFaulty(bool isFaulty)
        {
            RemoveDebug(new KeyData() { key = PIECEKEYS.FAULTY, dir = MAPDIRECTION.ANY });
            if (isFaulty)
            {
                AddDebug(new KeyData() { key = PIECEKEYS.FAULTY, dir = Orientation });
            }
        }
        /// <summary>
        /// set or removes the error key from debug keys
        /// </summary>
        /// <param name="isError"></param>
        internal void SetError(bool isError)
        {
            RemoveDebug(new KeyData() { key = PIECEKEYS.ERROR, dir = MAPDIRECTION.ANY });
            if (isError)
            {
                AddDebug(new KeyData() { key = PIECEKEYS.ERROR, dir = Orientation });
            }
        }
        /// <summary>
        /// Removes keys from debug. Any direction results in all matching keys being removed. Wallflags has to match direction.
        /// </summary>
        /// <param name="kData"></param>
        private void RemoveDebug(KeyData kData)
        {
            if (kData.key == PIECEKEYS.WFGREEN || kData.key == PIECEKEYS.WFRED)
            {
                debug.RemoveAll(p => p.key == kData.key && p.dir == kData.dir);
                return;
            }
            if (kData.dir == MAPDIRECTION.ANY)
            {
                debug.RemoveAll(p => p.key == kData.key);
                return;
            }
            debug.RemoveAll(p => p.key == kData.key && p.dir == kData.dir);
        }
        /// <summary>
        /// floor, ceiling and walls. The floor check can be excluded.
        /// </summary>
        /// <param name="ignoreFloor"></param>
        /// <returns></returns>
        private bool CheckIfEmpty(bool ignoreFloor = false)
        {
            if (!ignoreFloor && keyFloor.key != PIECEKEYS.NONE) { return false; }
            if (keyCeiling.key != PIECEKEYS.NONE) { return false; }
            if (HasWestWall || HasSouthWall || HasEastWall || HasNorthWall) { return false; }
            return true;
        }
        /// <summary>
        /// Returns the neighbouring piece to the right relative to the orientation
        /// </summary>
        /// <returns></returns>
        public MapPiece NeighbourRight()
        {
            return Neighbour(Dungeon.TwistRight(orientation), true);
        }
        /// <summary>
        /// Returns the neighbouring piece to the left relative to the orientation
        /// </summary>
        /// <returns></returns>
        public MapPiece NeighbourLeft()
        {
            return Neighbour(Dungeon.TwistLeft(orientation), true);
        }
        /// <summary>
        /// Returns the neighbouring piece in direction
        /// Remember that this will never return NULL.
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        public MapPiece Neighbour(MAPDIRECTION dir, bool createIfNeeded)
        {
            if(createIfNeeded){ return map.GetPiece(Coord + dir);}
            return map.GetExistingPiece(Coord + dir);
        }
        /// <summary>
        /// Assign keydata to wall location. Also set the wall flag.
        /// If the piece is NONE it leaves the wall flag clear.
        /// </summary>
        /// <param name="wallKey"></param>
        internal void AssignWall(KeyData wallKey, bool overideLocked)// TODO MAYBE assign connection flag here to?
        {
            if (state == MAPPIECESTATE.LOCKED && !overideLocked)
            {
                GD.PrintErr($"MapPiece", "AssignWall", $"Attempting to write wall data to locked piece! [{CoordString}] override[{overideLocked}]");
                SetError(true);
                return;
            }
            wallkeys[wallKey.dir] = wallKey;
            // Update the wallflag
            WALLS flags = ResolveWallFlag(wallKey.dir);
            if (wallKey.key == PIECEKEYS.NONE)
            {
                walls &= ~flags; // remove flag from direction
            }
            else
            {
                if (!walls.HasFlag(flags) && wallKey.key != PIECEKEYS.WCI)
                {
                    walls |= flags;// Flag direction as wall
                }
            }
        }
        /// <summary>
        ///  Returns the flag associated with the direction
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        private WALLS ResolveWallFlag(MAPDIRECTION dir)
        {
            switch (dir)
            {
                case MAPDIRECTION.NORTH:
                    return WALLS.N;
                case MAPDIRECTION.EAST:
                    return WALLS.E;
                case MAPDIRECTION.SOUTH:
                    return WALLS.S;
                case MAPDIRECTION.WEST:
                    return WALLS.W;
            }
            return new WALLS();
        }
        /// <summary>
        /// Returns the wallKey in given direction
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        internal KeyData WallKey(MAPDIRECTION dir)
        {
            switch (dir)
            {
                case MAPDIRECTION.EAST:
                    return WallKeyEast;
                case MAPDIRECTION.SOUTH:
                    return WallKeySouth;
                case MAPDIRECTION.WEST:
                    return WallKeyWest;
            }
            return WallKeyNorth;
        }
        #endregion

        public bool Equals(MapPiece other)
        {
            if(other is null) { return false; }
            return coord.x == other.coord.x && coord.y == other.coord.y && coord.z == other.coord.z;
        }
        public static bool operator ==(MapPiece a, MapPiece b)
        {
            if (a is null && b is null) { return true; }
            if (a is null || b is null) { return false; }
            return a.coord.x == b.coord.x && a.coord.y == b.coord.y && a.coord.z == b.coord.z;
        }
        public static bool operator !=(MapPiece a, MapPiece b)
        {
            if (a is null && b is not null) { return true; }
            if (b is null && a is not null) { return true; }
            if (a is null && b is null) { return true; }
            if (a.coord.x != b.coord.x || a.coord.y != b.coord.y || a.coord.z != b.coord.z) { return false; }
            return true;
        }
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return base.Equals(obj);
        }
        /// <summary>
        /// Will return the format Piece[{X}.{Y}.{Z}][{state}]
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"Piece[{Coord.x}.{Coord.y}.{Coord.z}][{state}]";
        }
        public override int GetHashCode()
        {
            return coord.x * 1000 + coord.y * 100000 + coord.z * 1000000000;
        }

        internal void Save()
        {
            map.SavePiece(this);
        }
    }// EOF CLASS
}
