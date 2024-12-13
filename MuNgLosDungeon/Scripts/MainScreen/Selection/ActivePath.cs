using System;
using System.Collections.Generic;
using Godot;
using Munglo.DungeonGenerator.Gizmos;
using Munglo.DungeonGenerator.Pathfinding;

namespace Munglo.DungeonGenerator.UI.Selection;
internal class ActivePath
{
    private Manager man;
    private List<PathLocation> pathRoute;
    private List<PathLocation> PathRoute {get=> pathRoute; set{ pathRoute = value; RaiseEvent();}}
    private List<int> sectionRoute;
    private List<int> SectionRoute {get=> sectionRoute; set{ sectionRoute = value; RaiseEvent();}}

    private void RaiseEvent()
    {
        man.addon.MS.RaiseOnPathDataPushed(new PathData(pathRoute, sectionRoute));
    }

    internal ActivePath(Manager man){
        this.man = man;
    }

    internal void UpdatePathGizmos(MapPiece selectedMapPiece, MapPiece selectedMapPiece2nd)
    {
        if (selectedMapPiece.SectionIndex == selectedMapPiece2nd.SectionIndex)
            {
                // Build path
                if (Pathing.FindSectionInnerPath(man.addon.MS.Map.Sections[selectedMapPiece.SectionIndex], selectedMapPiece.Coord, selectedMapPiece2nd.Coord, out List<PathLocation> path))
                {
                    //GD.Print($"Selection.Manager::UpdateGizmos() FindPath returned TRUE with path.Count[{path.Count}]");
                    PathRoute = path;
                    SegmentedGizmo pathy = man.GizmoSegmented.Instantiate() as SegmentedGizmo;
                    pathy.pathScale = 1.0f;
                    pathy.offset = Vector3.Up * 0.5f;
                    pathy.ClearSegments();
                    pathy.AddSegments(pathRoute);
                    pathy.color = Colors.Green;
                    man.addon.MS.Gizmos.AddChild(pathy);
                    pathy.Position = Vector3.Zero;
                }
            }
            else if (Pathing.FindSectionPath(selectedMapPiece.SectionIndex, selectedMapPiece2nd.SectionIndex, out List<int> sectionPath))
            {
                SectionRoute = sectionPath;
                for (int i = 0; i < sectionRoute.Count; i++)
                {
                    man.AddSectionGizmo(sectionRoute[i]);
                }
            }
            else
            {
                pathRoute = new List<PathLocation>();
            }
    }
}