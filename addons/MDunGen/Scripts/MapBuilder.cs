using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using Munglo.DungeonGenerator.Sections;
using System.Reflection;
using Godot.Collections;
using Munglo.DungeonGenerator.Pathfinding;

namespace Munglo.DungeonGenerator;
internal class MapBuilder
{
    private ulong[] seed;
    private MapData map;
    private PRNGMarsenneTwister rng;
    private GenerationSettingsResource Args => map.MapArgs;

    public MapBuilder(MapData map, ulong[] seed)
    {
        this.map = map;
        this.seed = seed;
        rng = new PRNGMarsenneTwister(this.seed);
    }
    internal async Task BuildFloorMapData(FloorResource floor, bool doPathing)
    {
        GD.Print("MapBuilder::BuildFloorMapData() starting.");

        foreach (BuildRuleResource ruleResource in floor.rules)
        {
            switch (ruleResource.catergory)
            {
                case CATEGORYRULE.BUILD:
                    await ResolveBuildRule(ruleResource);
                    break;
            }
        }

        GD.Print("MapBuilder::BuildFloorMapData() Rules done.");

        BuildOpeningsFromConnections();

        // Start Fitting every piece and add debug
        foreach (int X in map.Pieces.Keys)
        {
            foreach (int Y in map.Pieces[X].Keys)
            {
                foreach (int Z in map.Pieces[X][Y].Keys)
                {
                    FitRoundedCorners(map.Pieces[X][Y][Z]);
                    AddDebugKeys(map.Pieces[X][Y][Z]);
                }
            }
        }
        LatePassRooms();
        RemoveAllEmpty();
        // Do pathing for all connections
        if (doPathing)
        {
            GD.Print("MapBuilder::BuildFloorMapData() adding pathing.");
            DoPathingPass();
        }

        GD.Print("MapBuilder::BuildFloorMapData() Finished.");
    }
    private async Task ResolveBuildRule(BuildRuleResource rule)
    {
        ISection prevSec = map.Sections.Count > 0 ? map.Sections.Last() : null;
        for (int i = 0; i < rule.amount; i++)
        {
            if (ResolveLocationRule(rule.location, out MapPiece mp, prevSec))
            {
                ISection section = ResolveSectionInstance(mp.Coord, mp.Orientation, rule.section);
                //section.RNG = new ulong[4] { (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999) };
                section.Build();
                map.Sections.Add(section);
            }
            await Task.Delay(1);
        }
    }
    private ISection ResolveSectionInstance(MapCoordinate location, MAPDIRECTION direction, SectionResource sectionDef)
    {
        MapPiece piece = map.GetPiece(location);
        piece.Orientation = direction;

        SectionbBuildArguments buildArgs = new SectionbBuildArguments() { map = map, piece = piece, sectionID = map.Sections.Count, cfg = Args, sectionDefinition = sectionDef };

        Assembly assembly = Assembly.GetExecutingAssembly();
        Type type = assembly.GetTypes().First(t => t.Name == sectionDef.sectionType);

        object instance = Activator.CreateInstance(type, new object[] { buildArgs });

        ISection section = instance as SectionBase;

        GD.Print($"MapBuilder::ResolveSectionInstance() [{section.GetType().Name}]");
        return section;
    }
    private MAPDIRECTION ResolveDirectionRule(MAPDIRECTION direction)
    {
        return direction;
    }

    /// <summary>
    /// Returns 0,0,0 as default
    /// </summary>
    /// <param name="location"></param>
    /// <returns></returns>
    private bool ResolveLocationRule(STARTLOCATIONRULE location, out MapPiece mp, ISection prevSec)
    {
        switch (location)
        {
            case STARTLOCATIONRULE.ATTACHEDTOPREVIOUS:
                if (prevSec is null) { break; }
                if (prevSec.GetOuterWallFreeNeighbour(out mp, out MAPDIRECTION dir))
                {
                    mp.Orientation = dir;
                    return true;
                }
                break;
            case STARTLOCATIONRULE.CENTER:
                mp = map.GetPiece(MapCoordinate.Zero);
                return true;
        }
        mp = null;
        return false;
    }


    /*
    internal async Task BuildMapData(bool doPathing)
{
    rng = new PRNGMarsenneTwister(Args.Seed);
    // Build Start Room
    ulong[] roomSeed = new ulong[4] { (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999) };
    RoomSection centerRoom = new RoomSection(
        new SectionbBuildArguments()
        {
            sectionDefinition = Args.roomStart,
            map = map,
            piece = map.GetPiece(MapCoordinate.Zero),
            sectionID = map.Sections.Count,
            sectionSeed = roomSeed,
            cfg = Args
        });
    map.Sections.Add(centerRoom);
    centerRoom.Build();
    centerRoom.Save();
    GD.Print("MapBuilder::BuildMapData() Heart Done.");

    // Do generation per floor
    for (int floor = 0; floor < Args.nbOfFloors; floor++)
    {

        // Build corridors out from the Startroom
        if (Args.corPerFloor > 0)
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

    BuildOpeningsFromConnections();

    FitSmallArches();
    // Start Fitting every piece and add debug
    foreach (int X in map.Pieces.Keys)
    {
        foreach (int Y in map.Pieces[X].Keys)
        {
            foreach (int Z in map.Pieces[X][Y].Keys)
            {
                FitRoundedCorners(map.Pieces[X][Y][Z]);
                //FitLocation(pieces[X][Y][Z]);
                AddDebugKeys(map.Pieces[X][Y][Z]);
            }
        }
    }
    LatePassRooms();
    RemoveAllEmpty();


    // Do pathing for all connections
    if (doPathing)
    {
        DoPathingPass();
    }
}// EOF GenerateMap()
*/

    private void DoPathingPass()
    {
        // Process all connections
        foreach (KeyValuePair<int, SectionConnection> connPair in map.Connections)
        {
            if (connPair.Value.sectionID < 0 || connPair.Value.sectionID >= map.Sections.Count)
            {
                GD.PushError($"MapBuilder::DoPathingPass() missing section for connection Connection[key:{connPair.Key}][value.sectionID:{connPair.Value.sectionID}] Map has [{map.Sections.Count}] sections.");
                continue;
            }

            ISection section = map.Sections[connPair.Value.sectionID];
            //section.AddConnection(connection.Key);
        }

        foreach (ISection section in map.Sections)
        {
            foreach (int fromID in section.Connections)
            {
                SectionConnection conn = map.Connections[fromID];

                // path to the other connections in the same section
                foreach (int toID in section.Connections)
                {
                    if (fromID == toID) { continue; }
                    SectionConnection to = map.Connections[toID];

                    //if(section.SectionIndex != to.sectionID){ 
                    //    Godot.GD.PushError($"MapBuilder::DoPathingPass() Section missmatch! Skipping!");
                    //    continue;
                    //}
                    MapPiece mpStart = map.GetExistingPiece(conn.coord);
                    MapPiece mpEnd = map.GetExistingPiece(to.coord);
                    if (Pathing.FindPath(
                    new PathQuery(map, mpStart, mpEnd), out PathAnswer answer))
                    {
                        if (answer.path.Count > 0)
                        {
                            conn.Add(to.connectionID, to.coord, answer.path.Count);
                        }
                    }
                }
            }
        }
        //GD.Print("pathing Pass in builder!");
        //GD.Print(map.Connections.First().ToString());
    }

    private void BuildOpeningsFromConnections()
    {
        // TODO maybe move this last chance for a section to get a connection in
        for (int i = 0; i < map.Sections.Count; i++)
        {
            map.Sections[i].PunchBackDoor();
        }
        foreach (KeyValuePair<int, SectionConnection> con in map.Connections)
        {
            map.AddOpeningBetweenSections(con.Value, true);
        }
    }


    internal async Task BuildSection(string sectionTypeName, SectionResource sectionDef)
    {
        MapPiece piece = map.GetPiece(MapCoordinate.Zero);
        piece.Orientation = MAPDIRECTION.NORTH;

        SectionbBuildArguments buildArgs = new SectionbBuildArguments() { map = map, piece = piece, sectionID = map.Sections.Count, cfg = Args, sectionDefinition = sectionDef };

        Assembly assembly = Assembly.GetExecutingAssembly();
        Type type = assembly.GetTypes().First(t => t.Name == sectionTypeName);

        object instance = Activator.CreateInstance(type, new object[] { buildArgs });

        ISection section = instance as SectionBase;

        GD.Print($"MapBuilder::BuildSection() [{section.GetType().Name}]");

        section.Build();
        map.Sections.Add(section);

        // Start Fitting every piece and add debug
        foreach (int X in map.Pieces.Keys)
        {
            foreach (int Y in map.Pieces[X].Keys)
            {
                foreach (int Z in map.Pieces[X][Y].Keys)
                {
                    FitRoundedCorners(map.Pieces[X][Y][Z]);
                    //FitLocation(pieces[X][Y][Z]);
                    AddDebugKeys(map.Pieces[X][Y][Z]);
                }
            }
        }
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
    private void PlaceBridges(ISection section)
    {
        BridgePlacer bridgeMaker = new BridgePlacer(section, map);
        bridgeMaker.Place(section);
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
                piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.NORTH, variantID = 1 });
            }
        }
        if (SE)
        {
            piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.EAST }, true);
            if (piece.hasCieling)
            {
                piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.EAST, variantID = 1 });
            }
        }
        if (SW)
        {
            piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.SOUTH }, true);
            if (piece.hasCieling)
            {
                piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.SOUTH, variantID = 1 });
            }
        }
        if (NW)
        {
            if (adjacentN.HasWestWall && adjacentW.HasNorthWall)
            {
                piece.AssignWall(new KeyData() { key = PIECEKEYS.WCI, dir = MAPDIRECTION.WEST }, true);
                if (piece.hasCieling)
                {
                    piece.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = MAPDIRECTION.WEST, variantID = 1 });
                }
            }
        }
    }
    


    /*private void BuildRooms()
    {
        // Build Rooms attached to paths
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
*/
    /*
        private void BuildRoom(MapPiece piece)
        {
            ulong[] roomSeed = new ulong[4] { (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999) };
            RoomSection room = new RoomSection(
                new SectionbBuildArguments()
                {
                    sectionDefinition = Args.roomDefault,
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
    */
    private void LatePassRooms()
    {
        // Props pass of rooms
        foreach (ISection room in map.Sections)
        {
            if (room is not RoomSection) { continue; }
            //PlaceBridges(room);
        }
    }

    private void AddCorridor(MapPiece startpoint, MAPDIRECTION dir, int size, bool canBranch = true)
    {
        // make the seed to usefor the path
        ulong[] bosse = new ulong[4] { (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999), (ulong)rng.Next(1111, 9999) };
        startpoint.Orientation = dir;

        SectionResource corr = ResourceLoader.Load("res://addons/MDunGen/Config/Sections/DefaultCorridor.tres") as SectionResource;


        SectionbBuildArguments args = new SectionbBuildArguments() { sectionDefinition = corr, map = map, piece = startpoint, sectionID = map.Sections.Count, sectionSeed = bosse, cfg = Args };
        PathSection path = new PathSection(args);
        path.Build();
        if (path.IsValid) { path.Save(); map.Sections.Add(path); }
    }

    /// <summary>
    /// Removes all pieces in mapdata that isEmpty
    /// Run this as last step of the generation.
    /// </summary>
    private void RemoveAllEmpty()
    {
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

