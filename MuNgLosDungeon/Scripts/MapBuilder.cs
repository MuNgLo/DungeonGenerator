using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using Munglo.DungeonGenerator.Sections;
using System.Reflection;

namespace Munglo.DungeonGenerator
{
    internal class MapBuilder
    {
        private MapData map;
        private PRNGMarsenneTwister rng;
        private GenerationSettingsResource Args => map.MapArgs;

        public MapBuilder(MapData map)
        {
            this.map = map;
        }
        internal async Task BuildMapData()
        {
            rng = new PRNGMarsenneTwister(Args.Seed);
            // Build Start Room
            ulong[] roomSeed = new ulong[4] { (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999) };
            RoomSection centerRoom = new RoomSection(
                new SectionbBuildArguments()
                {
                    map = map,
                    piece = map.GetPiece(MapCoordinate.Zero),
                    sectionID = map.Sections.Count,
                    sectionSeed = roomSeed,
                    cfg = Args
                }, Args.roomStart);
            centerRoom.Build();
            centerRoom.Save();
            map.Sections.Add(centerRoom);
            GD.Print("MapBuilder::BuildMapData() Heart Done.");

            // Do generation per floor
            for (int floor = 0; floor < Args.nbOfFloors; floor++)
            {

                // Build corridors out from the Startroom
                if (Args.corridorPass && Args.corPerFloor > 0)
                {
                    List<MapPiece> candidates = centerRoom.GetWallPieces(floor);
                    if (candidates.Count < 1) { continue; }
                    int spread = candidates.Count / Math.Min(candidates.Count, Args.corPerFloor);

                    for (int i = 1; i < Args.corPerFloor + 1; i++)
                    {
                        int index = rng.Next(spread * (i - 1), spread * i);

                        if (index >= candidates.Count) { break; }

                        MapPiece startLocation = map.GetPiece(candidates[index].Coord + candidates[index].OutsideWallDirection());
                        AddCorridor(startLocation, candidates[index].OutsideWallDirection(), rng.Next(100) < Args.chanceForWideCorridor ? 2 : 1);

                        // add branches
                        if (map.Sections.Last() is PathSection)
                        {
                            PathSection pData = map.Sections.Last() as PathSection;
                            for (int b = 0; b < Args.maxBranches; b++)
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
                BuildRooms();
                await Task.Delay(1);
            }

            BuildSectionConnections();

            FitSmallArches();
            // Start Fitting every piece and add debug
            foreach (int X in map.Pieces.Keys)
            {
                foreach (int Y in map.Pieces[X].Keys)
                {
                    foreach (int Z in map.Pieces[X][Y].Keys)
                    {
                        if (Args.wallPass) { FitRoundedCorners(map.Pieces[X][Y][Z]); }
                        //FitLocation(pieces[X][Y][Z]);
                        if (Args.debugPass) { AddDebugKeys(map.Pieces[X][Y][Z]); }
                    }
                }
            }
            LatePassBridges();
            LatePassRooms();
            RemoveAllEmpty();
        }// EOF GenerateMap()

        private void BuildSectionConnections()
        {
            foreach (ISection section in map.Sections)
            {
                section.PunchBackDoor();
                section.BuildConnections();
            }
        }

        internal async Task BuildSection(string sectionTypeName)
        {
            MapPiece piece = map.GetPiece(MapCoordinate.Zero);
            piece.Orientation = MAPDIRECTION.NORTH;

            SectionbBuildArguments buildArgs = new SectionbBuildArguments() { map = map, piece = piece, sectionID = map.Sections.Count, cfg = Args };

            Assembly assembly = Assembly.GetExecutingAssembly();
            Type type = assembly.GetTypes().First(t => t.Name == sectionTypeName);

            object instance = Activator.CreateInstance(type, new object[] { buildArgs } );

            ISection section = instance as SectionBase;

            GD.Print($"MapBuilder::BuildSection() [{section.GetType().Name}]");

            section.Build();
            map.Sections.Add(section);

            FitSmallArches();
            // Start Fitting every piece and add debug
            foreach (int X in map.Pieces.Keys)
            {
                foreach (int Y in map.Pieces[X].Keys)
                {
                    foreach (int Z in map.Pieces[X][Y].Keys)
                    {
                        if (Args.wallPass) { FitRoundedCorners(map.Pieces[X][Y][Z]); }
                        //FitLocation(pieces[X][Y][Z]);
                        if (Args.debugPass) { AddDebugKeys(map.Pieces[X][Y][Z]); }
                    }
                }
            }
            LatePassBridges();
            LatePassRooms();
            RemoveAllEmpty();
            await Task.Delay(1);
        }

        private void AddDebugKeys(MapPiece piece)
        {
            piece.AddDebug(new KeyData() { key = PIECEKEYS.DEBUG, dir = piece.Orientation });

            if (piece.HasNorthWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.WFRED, dir = MAPDIRECTION.NORTH }); }
            if (piece.HasEastWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.WFRED, dir = MAPDIRECTION.EAST }); }
            if (piece.HasSouthWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.WFRED, dir = MAPDIRECTION.SOUTH }); }
            if (piece.HasWestWall) { piece.AddDebug(new KeyData() { key = PIECEKEYS.WFRED, dir = MAPDIRECTION.WEST }); }
        }
        /// <summary>
        /// Instances a BridgePlacer and 
        /// </summary>
        private void LatePassBridges()
        {
            BridgePlacer bridgeMaker = new BridgePlacer(map);
            bridgeMaker.Place();
        }

        private void FitRoundedCorners(MapPiece piece)
        {
            MapPiece adjacentN = map.GetExistingPiece(piece.Coord.StepNorth); // These need to NOT create new piecess
            MapPiece adjacentE = map.GetExistingPiece(piece.Coord.StepEast);
            MapPiece adjacentS = map.GetExistingPiece(piece.Coord.StepSouth);
            MapPiece adjacentW = map.GetExistingPiece(piece.Coord.StepWest);
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
                            NE = true;
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
                if (piece.hasCieling)
                {
                    piece.AddProp(new KeyData() { key = PIECEKEYS.ASIC, dir = MAPDIRECTION.NORTH });
                }
            }
            if (SE)
            {
                piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.EAST }, true);
                if (piece.hasCieling)
                {
                    piece.AddProp(new KeyData() { key = PIECEKEYS.ASIC, dir = MAPDIRECTION.EAST });
                }
            }
            if (SW)
            {
                piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.SOUTH }, true);
                if (piece.hasCieling)
                {
                    piece.AddProp(new KeyData() { key = PIECEKEYS.ASIC, dir = MAPDIRECTION.SOUTH });
                }
            }
            if (NW)
            {
                if (adjacentN.HasWestWall && adjacentW.HasNorthWall)
                {
                    piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.WEST }, true);
                    if (piece.hasCieling)
                    {
                        piece.AddProp(new KeyData() { key = PIECEKEYS.ASIC, dir = MAPDIRECTION.WEST });
                    }
                }
            }
        }
        private void FitSmallArches()
        {
            foreach (int X in map.Pieces.Keys)
            {
                foreach (int Y in map.Pieces[X].Keys)
                {
                    foreach (int Z in map.Pieces[X][Y].Keys)
                    {
                        if (Args.propPass) { FitSmallArch(map.Pieces[X][Y][Z]); }
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


        private void BuildRooms()
        {
            // Build Rooms attached to paths
            if (Args.roomPass)
            {
                for (int i = 0; i < map.Sections.Count; i++)
                {
                    for (int inx = 0; inx < Args.maxRoomsPerPath; inx++)
                    {
                        if (map.Sections[i] is not PathSection) { continue; }
                        MapPiece piece = (map.Sections[i] as PathSection).GetRandomAlongPath(out MAPDIRECTION dir);
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
            RoomSection room = new RoomSection(
                new SectionbBuildArguments()
                {
                    map = map,
                    piece = piece,
                    sectionID = map.Sections.Count,
                    sectionSeed = roomSeed,
                    cfg = Args
                });
            room.Build();
            room.Save();
            map.Sections.Add(room);
        }

        private void LatePassRooms()
        {
            // Props pass of rooms
            foreach (ISection room in map.Sections)
            {
                if (room is not RoomSection) { continue; }
                room.BuildProps();
            }
        }

        private void AddCorridor(MapPiece startpoint, MAPDIRECTION dir, int size, bool canBranch = true)
        {
            // make the seed to usefor the path
            ulong[] bosse = new ulong[4] { (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999) };
            startpoint.Orientation = dir;
            PathSection path = new PathSection(new SectionbBuildArguments() { map = map, piece = startpoint, sectionID = map.Sections.Count, sectionSeed = bosse, cfg = Args });
            if (path.IsValid) { path.Save(); map.Sections.Add(path); }
        }

        /// <summary>
        /// Removes all pieces in mapdata that isEmpty
        /// Run this as last step of the generation.
        /// </summary>
        private void RemoveAllEmpty()
        {
            if (!Args.clearEmpties) { return; }
            List<MapCoordinate> toDelete = new List<MapCoordinate>();
            foreach (int X in map.Pieces.Keys)
            {
                foreach (int Y in map.Pieces[X].Keys)
                {
                    foreach (int Z in map.Pieces[X][Y].Keys)
                    {
                        if (map.Pieces[X][Y][Z].isEmpty)
                        {
                            toDelete.Add(map.Pieces[X][Y][Z].Coord);
                        }
                    }
                }
            }
            foreach (MapCoordinate c in toDelete)
            {
                map.Pieces[c.x][c.y].Remove(c.z);
            }
        }
    }// EOF CLASS
}
