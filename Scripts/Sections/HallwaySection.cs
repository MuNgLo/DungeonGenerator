using Godot;
using System;
using System.Linq;

namespace Munglo.DungeonGenerator.Sections;
public class HallwaySection : SectionBase
{
    private int maxLength = 10;
    public HallwaySection(SectionbBuildArguments args) : base(args, false)
    {
        PathResource pathRes = args.sectionDefinition as PathResource;
        maxLength = pathRes.corMaxTotal;
        args.sectionDefinition.sizeDepthMax = maxLength;
        args.sectionDefinition.sizeDepthMin = maxLength;
    }
    public override void Build()
    {
        SetMinMaxCoord(coord + Dungeon.TwistRight(orientation) + MapCoordinate.Down);

        BuildHallwayStart(coord, orientation);
        MapCoordinate stepLocation = coord + orientation;
        for (int i = 0; i < maxLength - 1; i++)
        {
            AddHallwayStep(stepLocation, orientation);
            stepLocation = stepLocation + orientation;
        }
        BuildHallwayEnd(stepLocation, orientation);
        SealSection(0, -1, 0);
        foreach (MapPiece mapPiece in pieces)
        {
            mapPiece.SectionIndex = sectionIndex;
            map.SavePiece(mapPiece);           
        }
    }
    private void BuildHallwayStart(MapCoordinate startCoord, MAPDIRECTION dir)
    {
        ClaimSlice(startCoord, dir);
        MapPiece start = map.GetPiece(startCoord);
        start.State = MAPPIECESTATE.PENDING;
        start.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = dir, variantID = 2 });
        start.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = dir, variantID = 3 };
        pieces.Add(start);
        start.Save();
    }
     private void BuildHallwayEnd(MapCoordinate endCoord, MAPDIRECTION dir)
    {
        ClaimSlice(endCoord, dir);
        MapPiece end = map.GetPiece(endCoord + Dungeon.TwistRight(dir) + Dungeon.TwistRight(dir) + Dungeon.TwistRight(dir));
        end.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = Dungeon.Flip(dir), variantID = 2 });
        end.State = MAPPIECESTATE.PENDING;
        end.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = Dungeon.Flip(dir), variantID = 3 };
        pieces.Add(end);
        end.Save();
    }
    private void AddHallwayStep(MapCoordinate stepCoord, MAPDIRECTION dir)
    {
        ClaimSlice(stepCoord, dir);

        MapPiece stepLeft = map.GetPiece(stepCoord);
        stepLeft.State = MAPPIECESTATE.PENDING;
        stepLeft.AddExtra(new KeyData() { key = PIECEKEYS.ARCH, dir = dir, variantID = 2 });
        stepLeft.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = dir, variantID = 4 };
        pieces.Add(stepLeft);
        stepLeft.Save();
        MapPiece stepRight = map.GetPiece(stepCoord + Dungeon.TwistRight(dir) + Dungeon.TwistRight(dir) + Dungeon.TwistRight(dir));
        stepRight.State = MAPPIECESTATE.PENDING;
        stepRight.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = Dungeon.Flip(dir), variantID = 4 };
        pieces.Add(stepRight);
        stepRight.Save();
    }

    private void ClaimSlice(MapCoordinate stepCoord, MAPDIRECTION dir)
    {
        MAPDIRECTION r = Dungeon.TwistRight(dir);
        MapCoordinate startCoord = stepCoord + MapCoordinate.Down;
        // do it by row
        for (int i = 0; i < 3; i++)
        {
            ClaimPiece(startCoord, dir);
            ClaimPiece(startCoord + r, dir);
            ClaimPiece(startCoord + r + r, dir);
            ClaimPiece(startCoord + r + r + r, dir);
            startCoord += MapCoordinate.Up;
        }
    }
    private void ClaimPiece(MapCoordinate pieceCoord, MAPDIRECTION dir)
    {
        MapPiece mp = map.GetPiece(pieceCoord);
        mp.State = MAPPIECESTATE.PENDING;
        mp.Orientation = dir;
        pieces.Add(mp);
        mp.Save();
    }
}// EOF CLASS