using System;
using System.Collections.Generic;

namespace Munglo.DungeonGenerator;
public class SectionConnection
{
    public readonly int ParentSection;
    public readonly int ChildSection;
    public readonly MapCoordinate Coord;
    public readonly MAPDIRECTION Dir;
    /// <summary>
    /// Key: neighbor's MapCoordinate, Value: distance
    /// </summary>
    public Dictionary<MapCoordinate, double> Neighbors;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="p">Parent</param>
    /// <param name="c">Child</param>
    /// <param name="dir"></param>
    /// <param name="coord"></param>
    public SectionConnection(int p, int c, MAPDIRECTION dir, MapCoordinate coord)
    {
        ParentSection = p;
        ChildSection = c;
        Coord = coord;
        Dir = dir;
        Neighbors = new Dictionary<MapCoordinate, double>();
    }
    /// <summary>
    /// Returns the side of the connection that is in hte given sectionID
    /// </summary>
    /// <param name="sectionID"></param>
    /// <returns></returns>
    internal MapCoordinate GetSide(int sectionID)
    {
        return ParentSection == sectionID ? Coord : Coord + Dir;
    }

    public override string ToString(){
        string text = $"\n::::SectionConnection:::: {Coord} \n";
        foreach (KeyValuePair<MapCoordinate, double> item in Neighbors)
        {
            text += $"Coord{item.Key} cost [{item.Value}]\n";
        }
        text += "***********************************";
        return text;
    }
}