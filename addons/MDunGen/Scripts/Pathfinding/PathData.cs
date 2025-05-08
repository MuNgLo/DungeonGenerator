using System.Collections.Generic;
namespace Munglo.DungeonGenerator.Pathfinding;
internal class PathData{

    internal List<PathLocation> path;
    internal List<int> sections;

    internal PathData(List<PathLocation> pathLocations, List<int> sectionRoute=null){
        path = pathLocations;
        if(sectionRoute is null){sections = new();}else{sections = sectionRoute;}
    }
    public override string ToString(){
        string text = string.Empty;
        if(path is null) {return $"PathData: path is NULL";}
        if(path.Count > 0){
            text += $"PathData: path[{path.Count}]";
        }
        if(sections is not null && sections.Count > 0){
            text += $" sections[{sections.Count}]";
        }
        return text;
    }
}