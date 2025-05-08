
using System.Collections.Generic;

namespace Munglo.DungeonGenerator.Pathfinding
{
    internal class PathLocation
    {
        public MapCoordinate coord;
        public MapCoordinate parentCoord;
        public bool isClosed = false;
        public bool isWalkable;

        public int straightLineCost = int.MaxValue;
        public int calculatedCost = int.MaxValue;

        /// <summary>
        /// Key: neighbor's MapCoordinate, Value: distance
        /// </summary>
        public Dictionary<MapCoordinate, double> Neighbors;

        public PathLocation(MapPiece mapPiece)
        {
            if(mapPiece is null){
                Godot.GD.PushError($"PathLocation::Constructor() mapPiece was NULL!!");
                //System.Diagnostics.Debug.Assert(false);
                Godot.GD.Print(System.Environment.StackTrace);
                return;
            }
            isWalkable = mapPiece.hasFloor;
            coord = mapPiece.Coord;
        }
        public void SetNeighbours(Map map)
        {
            Neighbors = new Dictionary<MapCoordinate, double>();
            MapCoordinate[] mapCoordinates = Dungeon.NeighbourCoordinates(coord);
            for (int i = 0; i < 4; i++)
            {
                if (map.GetNodeById(mapCoordinates[i]) is not null)
                {
                    Neighbors[mapCoordinates[i]] = double.MaxValue;
                }
            }
            if (Neighbors.ContainsKey(MapCoordinate.North + coord) && Neighbors.ContainsKey(MapCoordinate.East + coord))
            {
                if (map.GetNodeById(coord + MapCoordinate.NorthEast) is not null)
                {
                    Neighbors[coord + MapCoordinate.NorthEast] = double.MaxValue;
                }
            }
            if (Neighbors.ContainsKey(MapCoordinate.North + coord) && Neighbors.ContainsKey(MapCoordinate.West + coord))
            {
                if (map.GetNodeById(coord + MapCoordinate.NorthWest) is not null)
                {
                    Neighbors[coord + MapCoordinate.NorthWest] = double.MaxValue;
                }
            }
            if (Neighbors.ContainsKey(MapCoordinate.South + coord) && Neighbors.ContainsKey(MapCoordinate.East + coord))
            {
                if (map.GetNodeById(coord + MapCoordinate.SouthEast) is not null)
                {
                    Neighbors[coord + MapCoordinate.SouthEast] = double.MaxValue;
                }
            }
            if (Neighbors.ContainsKey(MapCoordinate.South + coord) && Neighbors.ContainsKey(MapCoordinate.West + coord))
            {
                if (map.GetNodeById(coord + MapCoordinate.SouthWest) is not null)
                {
                    Neighbors[coord + MapCoordinate.SouthWest] = double.MaxValue;
                }
            }
        }
    }// EOF CLASS
}