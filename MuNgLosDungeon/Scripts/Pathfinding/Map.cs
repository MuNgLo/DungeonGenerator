using System.Collections.Generic;
using System.Linq;

namespace Munglo.DungeonGenerator.Pathfinding;

internal class Map
{
    private List<PathLocation> nodes;
    public List<PathLocation> Nodes { get => nodes; }

    // Retrieve a node by its ID
    public PathLocation GetNodeById(MapCoordinate coord)
    {
        return Nodes.Find(node => node.coord == coord);
    }
    internal Map(ISection section, List<MapPiece>extraPieces) {
        nodes = new List<PathLocation>();
        foreach (MapPiece piece in section.Pieces)
        {
            PathLocation loc = new PathLocation(piece);
            nodes.Add(loc);
        }
        foreach(MapPiece ep in extraPieces){
            PathLocation loc = new PathLocation(ep);
            nodes.Add(loc);
        }
    }
    internal void SetNeighbours()
    {
        foreach (PathLocation node in nodes)
        {
            node.SetNeighbours(this);
        }
        //Godot.GD.Print($"Map::Map() Nodes.Count[{Nodes.Count}]");
    }
}