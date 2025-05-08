using System;
using System.Collections.Generic;

namespace Munglo.DungeonGenerator.Pathfinding;

internal class PathAnswer
{
    internal readonly SectionConnection startConnectionn;
    internal readonly SectionConnection endConnection;
    internal List<PathLocation> path = new List<PathLocation>();
    internal List<int> connectionPath = new List<int>();
    internal PathAnswer(SectionConnection startConnectionn, SectionConnection endConnection, int tempConnection=-1, int tempConnection2=-1){
        this.startConnectionn = startConnectionn;
        this.endConnection = endConnection;
    }
     internal PathAnswer(PathQuery query){
        startConnectionn = query.startConnection;
        endConnection = query.endConnection;
    }
}// EOF CLASS