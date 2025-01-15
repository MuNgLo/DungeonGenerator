using System;
using System.Collections.Generic;

namespace Munglo.DungeonGenerator;
/// <summary>
/// Represents onse side of a dooropeninng you could say.
/// It defines the location in a section that is connected to another section
/// </summary>
public class SectionConnection
{
    public readonly int sectionID;
    public readonly int connectionID;
    public int connectedToConnectionID;
    public MapCoordinate coord;
    private List<ConnectedLocation> connectedLocations;
    internal List<ConnectedLocation> ConnectedLocations => connectedLocations;
    
    public readonly MAPDIRECTION Dir;

    public SectionConnection(int id, MapCoordinate mapLocation, int inSection, MAPDIRECTION dir)
    {
        connectedLocations = new();
        coord = mapLocation;
        connectionID = id;
        Dir = dir;
        sectionID = inSection;
        connectedToConnectionID = -1;
    }
    public void Add(int connectionID, MapCoordinate location, double cost){
        if(!connectedLocations.Exists(p=>p.connectionID == connectionID)){
            connectedLocations.Add(new ConnectedLocation(sectionID, connectionID, location, cost));
        }
    }
    /// <summary>
    /// Returns the side of the connection that is in hte given sectionID
    /// </summary>
    /// <param name="sectionID"></param>
    /// <returns></returns>
    internal MapCoordinate GetSide(int sectionID)
    {
        return connectedLocations.Find(p=>p.section == sectionID).coord;
    }
  
    public override string ToString(){
        string text = $"uniqueID[{connectionID}] in [S{sectionID}] to [C{connectedToConnectionID}]\n";
        text += string.Join(' ', connectedLocations);
        return text;
    }

    internal double GetCost(MapCoordinate coord)
    {
        if(ConnectedLocations.Exists(p=>p.coord == coord)){
            return ConnectedLocations.Find(p=>p.coord == coord).cost;
        }
        Godot.GD.PushError($"SectionConnection::GetCost({coord}) coord was not found as connectedLocation. Returning max cost!");
        return double.MaxValue;
    }
}