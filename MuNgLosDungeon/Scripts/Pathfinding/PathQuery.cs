using System;
using System.Collections.Generic;

namespace Munglo.DungeonGenerator.Pathfinding;

internal class PathQuery
{
    private bool isValid = false;
    private MapData map;
    private int startSection;
    private int endSection;
    internal readonly MapCoordinate startLocation;
    internal readonly MapCoordinate endLocation;
    internal SectionConnection startConnection;
    internal SectionConnection endConnection;
    internal bool IsValid => isValid;
    internal bool IsSectionPath => startSection != endSection;
    internal Dictionary<int, SectionConnection> Connections => map.Connections;
    internal ISection StartSection => map.Sections[startSection];
    internal int StartSectionIndex => map.Sections[startSection].SectionIndex;
    internal ISection EndSection => map.Sections[endSection];
    internal int EndSectionIndex => map.Sections[endSection].SectionIndex;

    internal List<MapPiece> Extras => BuildExtras();

    internal PathQuery(MapData mapData, MapPiece mpStart, MapPiece mpEnd)
    {
        map = mapData;
        startSection = mpStart.SectionIndex;
        endSection = mpEnd.SectionIndex;
        startLocation = mpStart.Coord;
        endLocation = mpEnd.Coord;
        if (startSection != endSection)
        {
            startConnection = MakeTempConnection(startSection, startLocation, -1);
            endConnection = MakeTempConnection(endSection, endLocation, -2);
        }
        isValid = true;
    }

    private SectionConnection MakeTempConnection(int sectionIndex, MapCoordinate location, int tempConnId)
    {
      
            SectionConnection tempConn = new SectionConnection(tempConnId, location, sectionIndex, MAPDIRECTION.ANY);
            // Get connections from section
            ISection section = map.Sections[sectionIndex];
            // Process connnection ID's from section
            foreach (int otherConnID in section.Connections)
            {
                SectionConnection otherConn = map.Connections[otherConnID];
                MapPiece mpStart = map.GetExistingPiece(tempConn.coord);
                MapPiece mpEnd = map.GetExistingPiece(otherConn.coord);
                if (sectionIndex == otherConn.sectionID)
                {
                    //if (Pathing.FindPath(
                    //    new PathQuery(map, mpStart, mpEnd), out PathAnswer answer))
                    //{
                    //    if (answer.path.Count > 0)
                    //    {
                    //        tempConn.Add(otherConn.connectionID, otherConn.coord, answer.path.Count);
                    //    }
                    //}
                    tempConn.Add(otherConn.connectionID, otherConn.coord, 1);
                }
            }
            map.Connections[tempConn.connectionID] = tempConn;
            return tempConn;
    }
    private List<MapPiece> BuildExtras()
    {
        List<MapPiece> extras = new List<MapPiece>();

        for (int i = 0; i < StartSection.ExtraPieces.Count; i++)
        {
            MapPiece mp = map.GetExistingPiece(StartSection.ExtraPieces[i]);
            if (mp is not null) { extras.Add(mp); }
        }
        return extras;
    }

    internal void OverrideSections(int start, int end)
    {
        startSection = start;
        endSection = end;
    }
}// EOF CLASS