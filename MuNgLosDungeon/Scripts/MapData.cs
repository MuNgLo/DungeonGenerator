using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Munglo.DungeonGenerator
{
    /// <summary>
    /// On instantiation this sets itself up to be used. Call GenerateMap() togenerate dungeon data. Use its callback to
    /// do what you want with it.
    /// </summary>
    public class MapData
    {
        #region TODO move this to settings
        private int chanceForWideCorridor = 70;
        private bool addArches = true;
        #endregion


        private Dictionary<int, Dictionary<int, Dictionary<int, MapPiece>>> pieces;
        private PRNGMarsenneTwister rng;
        private GenerationSettingsResource mapArgs;
        private RoomResource startRoom;
        private RoomResource standardRoom;
        private List<ISection> sections;

        public List<ISection> Sections => sections;
        internal Dictionary<int, Dictionary<int, Dictionary<int, MapPiece>>> Pieces => pieces;
        internal int nbOfPieces => pieces.Values.SelectMany(p => p.Values).Distinct().SelectMany(p => p.Values).Distinct().Count(); // Würkz
        internal MapData(GenerationSettingsResource p, RoomResource startRoom, RoomResource standardRoom)
        {
            mapArgs = p;
            this.startRoom = startRoom;
            this.standardRoom = standardRoom;
            pieces = new Dictionary<int, Dictionary<int, Dictionary<int, MapPiece>>>();
            sections = new List<ISection>();
            rng = new PRNGMarsenneTwister(p.Seed);
        }
        /// <summary>
        /// This generates the data that describes the layout of the dungeon
        /// When done it calls back
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        internal async Task GenerateMap(Action callback)
        {
            GD.Print("MapData::GenerateMap() Generation started.....");
            // Build Start StairCase Room
            ulong[] roomSeed = new ulong[4] { (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999) };
            RoomBase centerRoom = new RoomBase(this, MapCoordinate.Zero, MAPDIRECTION.NORTH, startRoom, sections.Count, roomSeed);
            centerRoom.Build();
            centerRoom.Save();
            sections.Add(centerRoom);
            await Task.Delay(100);
            GD.Print("MapData::GenerateMap() Heart Done.");

            // Do generation per floor
            for (int floor = 0; floor < mapArgs.nbOfFloors; floor++)
            {
                GD.Print($"MapData::GenerateMap() Generating floor[{floor}].");

                // Build corridors out from the Startroom
                if (mapArgs.corridorPass && mapArgs.corPerStair > 0)
                {
                    List<MapPiece> candidates = centerRoom.GetWallPieces(floor);
                    if (candidates.Count < 1) { continue; }
                    int spread = candidates.Count / Math.Min(candidates.Count, mapArgs.corPerStair);
                    GD.Print($"MapData::GenerateMap() Build corridors out from the Startroom candidates.Count[{candidates.Count}]. spread[{spread}]");

                    for (int i = 1; i < mapArgs.corPerStair + 1; i ++)
                    {
                        int index = rng.Next(spread*(i-1), spread * i);

                        GD.Print($"MapData::GenerateMap() Adding Corridor.. index[{index}]");
                        if(index >= candidates.Count) { break; }

                        MapPiece startLocation = GetPiece(candidates[index].Coord + candidates[index].OutsideWallDirection());
                        AddCorridor(startLocation, candidates[index].OutsideWallDirection(), rng.Next(100) < chanceForWideCorridor ? 2 : 1);

                        // add branches
                        if (sections.Last() is Path)
                        {
                            Path pData = sections.Last() as Path;
                            for (int b = 0; b < mapArgs.maxBranches; b++)
                            {
                                MapPiece branchPoint = pData.GetRandomAlongPathNoCorner(out MAPDIRECTION dir);
                                if (branchPoint is not null)
                                {
                                    AddCorridor(branchPoint, dir, rng.Next(100) < 70 ? 1 : 2);
                                }
                            }
                        }
                    }
                }
                GD.Print($"MapData::GenerateMap() Generating Rooms for floor[{floor}].");
                BuildRooms();
                await Task.Delay(100);
            }// EOF GenerateMap()

            FitSmallArches();
            // Start Fitting every piece and add debug
            foreach (int X in pieces.Keys)
            {
                foreach (int Y in pieces[X].Keys)
                {
                    foreach (int Z in pieces[X][Y].Keys)
                    {
                        if (mapArgs.wallPass) { FitRoundedCorners(pieces[X][Y][Z]); }
                        //FitLocation(pieces[X][Y][Z]);
                        if (mapArgs.debugPass) { AddDebugKeys(pieces[X][Y][Z]); }
                    }
                }
            }

            LatePassBridges();

            LatePassRooms();

            RemoveAllEmpty();
            await Task.Delay(100);
            //ProcGenMKIII.Log("MapData", "GenerateMap", $"NB of Piece[{nbOfPieces}] Unused[{GetPieces(MAPPIECESTATE.UNUSED).Count}] Pending[{GetPieces(MAPPIECESTATE.PENDING).Count}] Locked[{GetPieces(MAPPIECESTATE.LOCKED).Count}]");
            callback.Invoke();
        }
        /// <summary>
        /// Instances a BridgePlacer and 
        /// </summary>
        private void LatePassBridges()
        {
            BridgePlacer bridgeMaker = new BridgePlacer(this);
            bridgeMaker.Place();
        }


        private void BuildRooms()
        {
            // Build Rooms attached to paths
            if (mapArgs.roomPass)
            {
                for (int i = 0; i < sections.Count; i++)
                {
                    for (int inx = 0; inx < mapArgs.maxRoomsPerPath; inx++)
                    {
                        if(sections[i] is not Path) { continue; }
                        MapPiece piece = (sections[i] as Path).GetRandomAlongPath(out MAPDIRECTION dir);
                        if (piece != null && piece.State == MAPPIECESTATE.UNUSED)
                        {
                            piece.Orientation = dir;
                            BuildRoom(piece);
                        }
                    }
                }
            }
        }
        private void BuildRoom(MapPiece piece)
        {
            ulong[] roomSeed = new ulong[4] { (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999) };
            RoomBase room = new RoomBase(this, piece.Coord, piece.Orientation, standardRoom, sections.Count, roomSeed);
            room.Build();
            sections.Add(room);
        }
        
        private void LatePassRooms()
        {
            // Props pass of rooms
            foreach (ISection room in sections)
            {
                if (room is not RoomBase) { continue; }
                room.PunchBackDoor();
                room.BuildProps();
            }
        }

        private void AddDebugKeys(MapPiece piece)
        {
            piece.AddDebug(new KeyData() { key = PIECEKEYS.DEBUG, dir = piece.Orientation });

            if (piece.HasNorthWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.WFRED, dir = MAPDIRECTION.NORTH }); }
            if (piece.HasEastWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.WFRED, dir = MAPDIRECTION.EAST }); }
            if (piece.HasSouthWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.WFRED, dir = MAPDIRECTION.SOUTH }); }
            if (piece.HasWestWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.WFRED, dir = MAPDIRECTION.WEST }); }
        }

        private void FitRoundedCorners(MapPiece piece)
        {
            MapPiece adjacentN = GetExistingPiece(piece.Coord.StepNorth); // These need to NOT create new piecess
            MapPiece adjacentE = GetExistingPiece(piece.Coord.StepEast);
            MapPiece adjacentS = GetExistingPiece(piece.Coord.StepSouth);
            MapPiece adjacentW = GetExistingPiece(piece.Coord.StepWest);
            bool NE = false, SE = false, SW = false, NW = false;
            //check inner corners
            if (!piece.HasNorthWall && !piece.HasEastWall) // seems fine
            {
                if (adjacentN is not null && adjacentE is not null)
                {
                    if (!adjacentN.HasSouthWall && !adjacentE.HasWestWall)
                    {
                        if (adjacentN.HasEastWall && adjacentE.HasNorthWall)
                        {
                            NE =true;
                        }
                    }
                }
            }
            if (!piece.HasSouthWall && !piece.HasEastWall)
            {
                if (adjacentE is not null && adjacentS is not null)
                {
                    if (!adjacentS.HasNorthWall && !adjacentE.HasWestWall)
                    {
                        if (adjacentS.HasEastWall && adjacentE.HasSouthWall)
                        {
                            SE = true;

                        
                        }
                    }
                }
            }
            if (!piece.HasSouthWall && !piece.HasWestWall)
            {
                if (adjacentS is not null && adjacentW is not null)
                {
                    if (!adjacentS.HasNorthWall && !adjacentW.HasEastWall)
                    {
                        if (adjacentS.HasWestWall && adjacentW.HasSouthWall)
                        {
                            SW = true;
                        }
                    }
                }
            }
            if (!piece.HasNorthWall && !piece.HasWestWall)
            {
                if (adjacentN is not null && adjacentW is not null)
                {
                    if (!adjacentN.HasSouthWall && !adjacentW.HasEastWall)
                    {
                        NW = true;
                 
                    }
                }
            }

            // add the keys
            if (NE)
            {
                piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.NORTH }, true);
                if (piece.hasCieling && addArches)
                {
                    piece.AddProp(new KeyData() { key = PIECEKEYS.ASIC, dir = MAPDIRECTION.NORTH });
                }
            }
            if (SE)
            {
                piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.EAST }, true);
                if (piece.hasCieling && addArches)
                {
                    piece.AddProp(new KeyData() { key = PIECEKEYS.ASIC, dir = MAPDIRECTION.EAST });
                }
            }
            if (SW)
            {
                piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.SOUTH }, true);
                if (piece.hasCieling && addArches)
                {
                    piece.AddProp(new KeyData() { key = PIECEKEYS.ASIC, dir = MAPDIRECTION.SOUTH });
                }
            }
            if (NW)
            {
                if (adjacentN.HasWestWall && adjacentW.HasNorthWall)
                {
                    piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.WEST }, true);
                    if (piece.hasCieling && addArches)
                    {
                        piece.AddProp(new KeyData() { key = PIECEKEYS.ASIC, dir = MAPDIRECTION.WEST });
                    }
                }
            }
        }
        private void FitSmallArches()
        {
            if (!addArches) { return; }
            foreach (int X in pieces.Keys)
            {
                foreach (int Y in pieces[X].Keys)
                {
                    foreach (int Z in pieces[X][Y].Keys)
                    {
                        if (mapArgs.propPass) { FitSmallArch(pieces[X][Y][Z]); }
                    }
                }
            }
        }
        private void FitSmallArch(MapPiece piece)
        {
            if (!piece.hasCieling) { return; }
            // add small arches
            if (piece.HasNorthWall) { piece.AddProp(new KeyData() { key = PIECEKEYS.AS, dir = MAPDIRECTION.NORTH }); }
            if (piece.HasEastWall) { piece.AddProp(new KeyData() { key = PIECEKEYS.AS, dir = MAPDIRECTION.EAST }); }
            if (piece.HasSouthWall) { piece.AddProp(new KeyData() { key = PIECEKEYS.AS, dir = MAPDIRECTION.SOUTH }); }
            if (piece.HasWestWall) { piece.AddProp(new KeyData() { key = PIECEKEYS.AS, dir = MAPDIRECTION.WEST }); }
        } 
   

        private void AddCorridor(MapPiece startpoint, MAPDIRECTION dir, int size, bool canBranch = true)
        {
            // make the seed to usefor the path
            ulong[] bosse = new ulong[4] { (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999) };
            startpoint.Orientation = dir;
            Path path = new Path(this, startpoint, size, bosse, mapArgs.corMaxTotal, mapArgs.corMaxStraight, mapArgs.corMinStraight);
            if (path.IsValid) { sections.Add(path); }
        }

        internal void SavePiece(MapPiece piece)
        {
            pieces[piece.Coord.x][piece.Coord.y][piece.Coord.z] = piece;
        }

        /// <summary>
        /// TODO rework this
        /// </summary>
        /// <param name="floor"></param>
        /// <param name="piece"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        private bool GetRandomRandomEdgePiece(int floor, out MapPiece piece, out MAPDIRECTION dir)
        {

            int y = floor;
            int x = -1;
            int z = -1;

            dir = MAPDIRECTION.NORTH;

            // N or S
            if (rng.Next(100) < 50)
            {
                x = rng.Next(5);
                if (rng.Next(100) < 50)
                {
                    z = 0;
                    dir = MAPDIRECTION.SOUTH;
                }
                else
                {
                    z = 5 - 1;
                    dir = MAPDIRECTION.NORTH;
                }
            }
            else // W or E
            {
                z = rng.Next(5);
                if (rng.Next(100) < 50)
                {
                    x = 0;
                    dir = MAPDIRECTION.EAST;
                }
                else
                {
                    x = 5 - 1;
                    dir = MAPDIRECTION.WEST;
                }
            }

            VerifyPiece(new MapCoordinate( x, y, z));
            piece = pieces[x][y][z];
            return true;
        }


        internal ISection GetRandomRoom()
        {
            return sections[rng.Next(0, sections.Count)];
        }



        internal MapPiece GetRandomPiece()
        {
            int x = pieces.ElementAt(rng.Next(pieces.Keys.Count)).Key;
            int y = pieces[x].ElementAt(rng.Next(pieces[x].Keys.Count)).Key;
            int z = pieces[x][y].ElementAt(rng.Next(pieces[x][y].Keys.Count)).Key;
            MapPiece pick = pieces[x][y][z];
            while (pick.keyFloor.key != PIECEKEYS.F)
            {
                x = pieces.ElementAt(rng.Next(pieces.Keys.Count)).Key;
                y = pieces[x].ElementAt(rng.Next(pieces[x].Keys.Count)).Key;
                z = pieces[x][y].ElementAt(rng.Next(pieces[x][y].Keys.Count)).Key;
                pick = pieces[x][y][z];
            }
            //ProcGenMKIII.Log("MapData", "GetRandomPiece", $"pieces.Keys.Count[{pieces.Keys.Count}] pieces.ElementAt(0)[{pieces.ElementAt(0)}]");
            //ProcGenMKIII.Log("MapData", "GetRandomPiece", $"pieces.Keys.Count[{pieces[x].Keys.Count}] pieces[x].ElementAt(0)[{(pieces[x].ElementAt(0))}]");
            return pick;
        }
        /// <summary>
        /// Uses piece verification and then return the piece.
        /// Will create piece if needed.
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        internal MapPiece GetPiece(MapCoordinate coord)
        {
            VerifyPiece(coord);
            return pieces[coord.x][coord.y][coord.z];
        }
     

        /// <summary>
        /// Get piece if it exists or return null
        /// Use this when iterating across map to not change map
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        private MapPiece GetExistingPiece(MapCoordinate coord)
        {
            if (pieces.ContainsKey(coord.x))
            {
                if (pieces[coord.x].ContainsKey(coord.y))
                {
                    if (pieces[coord.x][coord.y].ContainsKey(coord.z))
                    {
                        return pieces[coord.x][coord.y][coord.z];
                    }
                }
            }
            return null;
        }
   
        /// <summary>
        /// Returns a List of pieces with the state
        /// </summary>
        /// <param name="queriedState"></param>
        /// <returns></returns>
        internal List<MapPiece> GetPieces(MAPPIECESTATE queriedState)
        {
            List<MapPiece> picked = new List<MapPiece>();

            foreach (int keyX in pieces.Keys)
            {
                foreach (int keyY in pieces[keyX].Keys)
                {
                    foreach (int keyZ in pieces[keyX][keyY].Keys)
                    {
                        if (pieces[keyX][keyY][keyZ].State == queriedState)
                        {
                            picked.Add(pieces[keyX][keyY][keyZ]);
                        }
                    }
                }
            }
            return picked;
        }
  
        /// <summary>
        /// Verifies piece instance exists. Makes one if needed
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="verbose"></param>
        private void VerifyPiece(MapCoordinate coord, bool verbose = false)
        {
            if (pieces == null) { pieces = new Dictionary<int, Dictionary<int, Dictionary<int, MapPiece>>>(); }

            if (!pieces.Keys.Contains(coord.x)) { pieces[coord.x] = new Dictionary<int, Dictionary<int, MapPiece>>(); }
            if (!pieces[coord.x].Keys.Contains(coord.y)) { pieces[coord.x][coord.y] = new Dictionary<int, MapPiece>(); }
            if (!pieces[coord.x][coord.y].Keys.Contains(coord.z))
            {
                if (verbose) { DungeonGenerator.Log(this, "VerifyPieceSpace", $"insert blank piece [{coord.x}.{coord.y}.{coord.z}]"); }

                pieces[coord.x][coord.y][coord.z] = new MapPiece(this, coord);
            }

        }

        internal bool AddOpeningToSection(MapPiece piece, MAPDIRECTION dir, bool wide, bool overrideLocked)
        {
            if (piece.SectionIndex < 0) return false;
            //ProcGenMKIII.Log("MapData", $"AddOpeningToRoom", $"Room[{piece.RoomIndex}] piece({piece}) {dir}");
            return sections[piece.SectionIndex].AddOpening(piece.Coord, dir, wide, overrideLocked);
        }

        internal void AddOpeningBetweenSections(MapPiece p1, MapPiece p2, MAPDIRECTION dir, bool overrideLocked)
        {
            if (p1.SectionIndex < 0 || p2.SectionIndex < 0) return;
            if (p1.SectionIndex == p2.SectionIndex) return;
            sections[p1.SectionIndex].AddOpening(p1.Coord, dir, false, overrideLocked);
            sections[p2.SectionIndex].AddOpening(p2.Coord, Dungeon.Flip(dir), false, overrideLocked);
        }
        public MapPiece GetNextPieceOver(MapPiece startPiece, MAPDIRECTION orientation)
        {

            switch (orientation)
            {
                case MAPDIRECTION.NORTH:
                    startPiece = GetPiece(startPiece.Coord.StepNorth);
                    break;
                case MAPDIRECTION.EAST:
                    startPiece = GetPiece(startPiece.Coord.StepEast);
                    break;
                case MAPDIRECTION.SOUTH:
                    startPiece = GetPiece(startPiece.Coord.StepSouth);
                    break;
                case MAPDIRECTION.WEST:
                    startPiece = GetPiece(startPiece.Coord.StepWest);
                    break;
                case MAPDIRECTION.UP:
                    startPiece = GetPiece(startPiece.Coord.StepUp);
                    break;
                case MAPDIRECTION.DOWN:
                    startPiece = GetPiece(startPiece.Coord.StepDown);
                    break;
            }
            return startPiece;
        }
        /// <summary>
        /// Removes all pieces in mapdata that isEmpty
        /// Run this as last step of the generation.
        /// </summary>
        private void RemoveAllEmpty()
        {
            List<MapCoordinate> toDelete = new List<MapCoordinate>();
            foreach (int X in pieces.Keys)
            {
                foreach (int Y in pieces[X].Keys)
                {
                    foreach (int Z in pieces[X][Y].Keys)
                    {
                        if (pieces[X][Y][Z].isEmpty)
                        {
                            toDelete.Add(pieces[X][Y][Z].Coord);
                        }
                    }
                }
            }
            foreach (MapCoordinate c in toDelete)
            {
                pieces[c.x][c.y].Remove(c.z);
            }
        }
        #region Functional to manipulate mappieces
        /// <summary>
        /// Add WD between the pieces using piece1's orientation
        /// piece2 is usually piece1.step(piece1.orientation)
        /// </summary>
        public void AddDoor(MapPiece piece1, MapPiece piece2, bool overrideLocked)
        {
            piece1.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = piece1.Orientation }, overrideLocked);
            piece2.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = Dungeon.Flip(piece1.Orientation) }, overrideLocked);
        }

        internal void AddDoorWide(MapPiece piece1, bool overrideLocked)
        {
            MapPiece piece2 = piece1.Neighbour(Dungeon.TwistRight(piece1.Orientation));
            piece1.AssignWall(new KeyData() { key = PIECEKEYS.OCCUPIED, dir = Dungeon.Flip(piece1.Orientation) }, overrideLocked); 
            piece2.AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = Dungeon.Flip(piece1.Orientation) }, overrideLocked);
            piece1.Neighbour(Dungeon.Flip(piece1.Orientation)).AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = piece1.Orientation }, overrideLocked);
            piece2.Neighbour(Dungeon.Flip(piece1.Orientation)).AssignWall(new KeyData() { key = PIECEKEYS.OCCUPIED, dir = piece1.Orientation }, overrideLocked);
        }

        #endregion
    }// EOF CLASS
}
