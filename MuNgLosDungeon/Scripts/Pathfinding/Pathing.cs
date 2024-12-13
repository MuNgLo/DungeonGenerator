using System;
using System.Collections.Generic;
using System.Linq;

namespace Munglo.DungeonGenerator.Pathfinding;

internal static class Pathing
{
    internal static bool FindSectionPath(int sectionIndex1, int sectionIndex2, out List<int> sectionPath)
    {
        sectionPath = new List<int>(){sectionIndex1, sectionIndex2};
        return true;
    }
    internal static bool FindSectionInnerPath(ISection section, MapCoordinate from, MapCoordinate to, out List<PathLocation> path)
    {
        Map map = new Map(section);
        map.SetNeighbours();
        PathLocation start = new PathLocation(section.Pieces.Find(p => p.Coord == from));
        start.SetNeighbours(map);
        PathLocation goal = new PathLocation(section.Pieces.Find(p => p.Coord == to));
        goal.SetNeighbours(map);
        path = AStar(start, goal, map);
        return path is not null;
    }
    internal static List<PathLocation> AStar(PathLocation start, PathLocation goal, Map map)
    {
        var openList = new List<PathLocation> { start };
        var closedList = new HashSet<PathLocation>();

        // Dictionaries to hold g(n), h(n), and parent pointers
        var gScore = new Dictionary<MapCoordinate, double> { [start.coord] = 0 };
        var hScore = new Dictionary<MapCoordinate, double> { [start.coord] = StepDistance(start, goal) };
        var parentMap = new Dictionary<MapCoordinate, PathLocation>();

        while (openList.Count > 0)
        {
            // Find node in open list with the lowest F score
            var current = openList.OrderBy(node => gScore[node.coord] + hScore[node.coord]).First();

            if (current.coord == goal.coord)
            {
                return ReconstructPath(parentMap, current);
            }

            openList.Remove(current);
            closedList.Add(current);

            foreach (MapCoordinate nbCoord in current.Neighbors.Keys)
            {
                PathLocation neighbor = map.GetNodeById(nbCoord);
                if (neighbor == null || closedList.Contains(neighbor)) continue;
                // dont cut diagonaly through corners
                /*MapCoordinate direction = current.coord - neighborId;
                if (direction == MapCoordinate.NorthEast)
                {
                    if (map.GetNodeById(current.coord + MapCoordinate.North) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(North) Skipped NE diagonal because corner! direction[{direction}]");
                        continue;
                    }
                    if (map.GetNodeById(current.coord + MapCoordinate.East) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(East) Skipped NE diagonal because corner! direction[{direction}]");
                        continue;
                    }
                }
                if (direction == MapCoordinate.NorthWest)
                {
                    if (map.GetNodeById(current.coord + MapCoordinate.North) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(North) Skipped NW diagonal because corner! direction[{direction}]");
                        continue;
                    }
                    if (map.GetNodeById(current.coord + MapCoordinate.West) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(West) Skipped NW diagonal because corner! direction[{direction}]");
                        continue;
                    }
                }


                  if (direction == MapCoordinate.SouthEast)
                {
                    if (map.GetNodeById(current.coord + MapCoordinate.South) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(South) Skipped SE diagonal because corner! direction[{direction}]");
                        continue;
                    }
                    if (map.GetNodeById(current.coord + MapCoordinate.East) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(East) Skipped SE diagonal because corner! direction[{direction}]");
                        continue;
                    }
                }
                if (direction == MapCoordinate.SouthWest)
                {
                    if (map.GetNodeById(current.coord + MapCoordinate.South) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(South) Skipped SW diagonal because corner! direction[{direction}]");
                        continue;
                    }
                    if (map.GetNodeById(current.coord + MapCoordinate.West) is null)
                    {
                        Godot.GD.Print($"Pathing::AStar(West) Skipped SW diagonal because corner! direction[{direction}]");
                        continue;
                    }
                }
                */




                // Tentative gScore (current gScore + distance to neighbor)
                double tentativeGScore = gScore[current.coord] + current.Neighbors[nbCoord];

                if (!gScore.ContainsKey(neighbor.coord) || tentativeGScore < gScore[neighbor.coord])
                {
                    // Update gScore and hScore
                    gScore[neighbor.coord] = tentativeGScore;
                    hScore[neighbor.coord] = StepDistance(neighbor, goal);

                    // Set the current node as the parent of the neighbor
                    parentMap[neighbor.coord] = current;

                    if (!openList.Contains(neighbor))
                    {
                        openList.Add(neighbor);
                    }
                }
            }
        }

        return new List<PathLocation>(); // No path found
    }




    /*
        public static bool FindPath(MapData map, MapCoordinate from, MapCoordinate to, out List<MapCoordinate> path, bool leaveSection = false)
        {
            path = new List<MapCoordinate>();
            MapPiece fmp = map.GetPiece(from);
            MapPiece tmp = map.GetPiece(to);
            if (leaveSection == false && fmp.SectionIndex != tmp.SectionIndex)
            {
                // target mappiece oputside section. Not allowed! 
                return false;
            }
            List<PathLocation> locations = BuildLocations(map.Sections[fmp.SectionIndex]);
            if (locations.Count < 1) { return false; }
            List<PathLocation> locationsToProcess = new();
            //SetStraightPathCost(ref locations, to);
            locations.RemoveAll(p => p.straightLineCost == int.MaxValue);
            locations = locations.OrderBy(p => p.straightLineCost).ToList();
            locationsToProcess.Add(locations[0]);

            int breaker = 500;

            while (locations.Exists(p => p.isClosed == false))
            {
                if (locationsToProcess.Count < 1) { locationsToProcess.Add(locations.Find(p => p.isClosed == false)); }
                ProcessLocation(locationsToProcess[0], from, to, ref locations, ref locationsToProcess);
                locationsToProcess.RemoveAll(p => p.isClosed);
                breaker--;
                if (breaker < 1)
                {
                    Godot.GD.PrintErr($"Pathing::FindPath() FAILED! Breaker tripped. locations.Count[{locations.Count}] locationsToProcess.Count[{locationsToProcess.Count}]");
                    break;
                }
            }

            return false;
        }
    */
    static private void ProcessLocation(PathLocation loc, MapCoordinate from, MapCoordinate to, ref List<PathLocation> locations, ref List<PathLocation> locationsToProcess)
    {
        if (loc.coord == to) { loc.isClosed = true; loc.calculatedCost = 0; }

        ProcessNeighbour(loc.coord + MapCoordinate.North, loc, ref locations, ref locationsToProcess);
        ProcessNeighbour(loc.coord + MapCoordinate.North + MapCoordinate.East, loc, ref locations, ref locationsToProcess);
        ProcessNeighbour(loc.coord + MapCoordinate.East, loc, ref locations, ref locationsToProcess);
        ProcessNeighbour(loc.coord + MapCoordinate.East + MapCoordinate.South, loc, ref locations, ref locationsToProcess);
        ProcessNeighbour(loc.coord + MapCoordinate.South, loc, ref locations, ref locationsToProcess);
        ProcessNeighbour(loc.coord + MapCoordinate.South + MapCoordinate.West, loc, ref locations, ref locationsToProcess);
        ProcessNeighbour(loc.coord + MapCoordinate.West, loc, ref locations, ref locationsToProcess);
        ProcessNeighbour(loc.coord + MapCoordinate.West + MapCoordinate.North, loc, ref locations, ref locationsToProcess);


    }
    static private void ProcessNeighbour(MapCoordinate nc, PathLocation loc, ref List<PathLocation> locations, ref List<PathLocation> locationsToProcess)
    {
        if (locations.Exists(p => p.coord == nc))
        {
            PathLocation n = locations.Find(p => p.coord == nc);
            if (loc.calculatedCost + 1 < n.calculatedCost)
            {
                n.parentCoord = loc.coord;
                n.isClosed = false;
                if (!locationsToProcess.Exists(p => p.coord == n.coord))
                {
                    locationsToProcess.Add(n);
                }
            }
            if (n.calculatedCost + 1 < loc.calculatedCost)
            {
                loc.parentCoord = n.coord;
            }
            loc.isClosed = true;
        }
    }

    /*static private void SetStraightPathCost(ref List<PathLocation> locations, MapCoordinate to)
    {
        for (int i = 0; i < locations.Count; i++)
        {
            if (locations[i].isWalkable == false)
            {
                locations[i].straightLineCost = int.MaxValue;
            }
            locations[i].straightLineCost = StepDistance(locations[i], to);
        }
    }*/


    static private List<PathLocation> BuildLocations(ISection section)
    {
        List<PathLocation> locations = new List<PathLocation>();
        for (int i = 0; i < section.Pieces.Count; i++)
        {
            locations.Add(new PathLocation(section.Pieces[i]));
        }
        return locations;
    }

    /*************************************************************************
                            Should be fine under this
    **************************************************************************/

    static private List<PathLocation> ReconstructPath(Dictionary<MapCoordinate, PathLocation> parentMap, PathLocation current)
    {
        var path = new List<PathLocation> { current };

        while (parentMap.ContainsKey(current.coord))
        {
            current = parentMap[current.coord];
            path.Add(current);
        }

        path.Reverse();
        return path;
    }

    /// <summary>
    /// Manhattan distance heuristic cost
    /// </summary>
    /// <param name="coord"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    static private int StepDistance(PathLocation start, PathLocation goal)
    {
        MapCoordinate cd = start.coord - goal.coord;
        int x = Math.Abs(cd.x);
        int y = Math.Abs(cd.y);
        return x + y;
    }

  
}// EOF CLASS