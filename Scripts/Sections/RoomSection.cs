using Godot;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using static System.Collections.Specialized.BitVector32;

namespace Munglo.DungeonGenerator.Sections
{
    public class RoomSection : SectionBase
    {
        public RoomSection(SectionbBuildArguments args) : base(args) { }

        #region ISection methods
        public override void Build()
        {
            MapPiece start = map.GetPiece(coord);
            start.State = MAPPIECESTATE.PENDING;
            start.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = orientation, variantID = 0 };
            pieces.Add(start);
            start.Save();

            MapPiece parent = map.GetExistingPiece(coord + Dungeon.Flip(orientation));
            if (parent is not null)
            {
                //AddConnection(Dungeon.Flip(orientation), map.Sections[parent.SectionIndex], start.Coord, parent.Coord, true);

                ISection parentSection = map.Sections[parent.SectionIndex];
                int c1 = AddConnection(Dungeon.Flip(orientation), parentSection, start.Coord, parent.Coord, true);
                int c2 = parentSection.AddConnection(orientation, this, parent.Coord, start.Coord, true);
                map.Connections[c1].connectedToConnectionID = c2;
                map.Connections[c2].connectedToConnectionID = c1;


            }

            //ProcGenMKIII.Log("RoomBase", $"BuildRoom", $"Loc{coord}  Size({sizeX}.{sizeY}.{sizeZ}) minX({minX}) maxX({maxX})");
            int breaker = 0;
            while (pieces.Exists(p => p.State == MAPPIECESTATE.PENDING))
            {
                ProcessPiece(pieces.Find(p => p.State == MAPPIECESTATE.PENDING));
                breaker++;
                if (breaker > 1000)
                {
                    GD.PrintErr($"RoomBase", $"BuildRoom", $"ProcessPiece loop hit breaker!");
                    break;
                }
            }


            SealSection();


            if (sectionDefinition.firstPieceDoor)
            {
                pieces.First().AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = Dungeon.Flip(pieces.First().Orientation) }, true);
                pieces.First().Neighbour(Dungeon.Flip(pieces.First().Orientation), true).AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = pieces.First().Orientation }, true);
            }
            FitSmallArches();
        }

        private void ProcessPiece(MapPiece rp)
        {
            if (rp.State != MAPPIECESTATE.PENDING)
            {
                return;
            }
            rp.Orientation = orientation;
            rp.SectionIndex = sectionIndex;

            // Do all MAPDIRECTIONs
            for (int i = 1; i < 7; i++)
            {
                MAPDIRECTION processingDirection = (MAPDIRECTION)i;
                MapPiece nb = rp.Neighbour(processingDirection, true);
                if (nb.State == MAPPIECESTATE.UNUSED)
                {
                    if ((nb.Coord.x >= minX && nb.Coord.x <= maxX
                        && nb.Coord.y >= MinY && nb.Coord.y < MaxY
                        && nb.Coord.z >= minZ && nb.Coord.z <= maxZ)
                        )
                    {
                        // Not bottomfloor so have to have same sectionindex underneath
                        if (nb.Coord.y > MinY && !pieces.Exists(p => p.Coord == nb.Coord + MAPDIRECTION.DOWN))
                        {
                            // blocked by piece no part of room so wall it
                            //rp.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = processingDirection }, false);
                            rp.State = MAPPIECESTATE.LOCKED;
                            map.SavePiece(rp);
                            return;
                        }

                        // Expand room to tile if within limits
                        nb.State = MAPPIECESTATE.PENDING;
                        nb.SectionIndex = sectionIndex;
                        nb.sectionfloor = Math.Abs(nb.Coord.y - pieces.First().Coord.y);

                        if (nb.sectionfloor == 0 || (sectionDefinition.allFloor && !nb.hasFloor && nb.sectionfloor < sizeY - 1))
                        {
                            //nb.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = orientation, variantID = 0 };
                        }

                        pieces.Add(nb);
                        map.SavePiece(nb);
                    }
                    else
                    {
                        if (rp.Coord + MAPDIRECTION.UP == nb.Coord)
                        {
                            //rp.keyCeiling = new KeyData() { key = PIECEKEYS.C, dir = processingDirection };
                        }
                        // cant expand so set wall
                        //rp.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = processingDirection }, false);
                    }
                }
                else
                {
                    if (!pieces.Exists(p => p.Coord == nb.Coord))
                    {
                        // blocked by piece no part of room so wall it
                        //rp.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = processingDirection }, false);
                    }
                }
            }
            rp.State = MAPPIECESTATE.LOCKED;
            map.SavePiece(rp);
        }


        public override bool AddPropOnRandomTile(KeyData keyData, out MapPiece pick)
        {
            pick = null;
            if (pieces.Count < 3)
            {
                return false;
            }
            pick = pieces[rng.Next(pieces.Count - 1) + 1];
            while (pick.keyFloor.key == PIECEKEYS.NONE)
            {
                pick = pieces[rng.Next(pieces.Count - 1) + 1];
            }
            //ProcGenMKIII.Log("RoomBase", "AddPropOnRandomTile", $"Adding [{keyData.key}] to [{pick}] in room[{roomIndex}]");
            pick.AddExtra(keyData);
            return true;
        }
        public override void PunchBackDoor()
        {
            if (sectionDefinition.backDoorChance < 1 || rng.Next(100) > sectionDefinition.backDoorChance)
            {
                return;
            }
            int breaker = 20;

            List<MapPiece> candidates = GetWallPieces(0, true);
            candidates.RemoveAll(p => p.HasWall(Dungeon.Flip(orientation)));

            while (breaker > 0 && candidates.Count > 2)
            {
                breaker--;
                MapPiece pick = candidates[rng.Next(candidates.Count)];
                candidates.Remove(pick);

                MAPDIRECTION dir = pick.OutsideWallDirection();
                if (pick.WallKey(dir).key != PIECEKEYS.W) { continue; }
                MapPiece nb = pick.Neighbour(dir, true);
                if (nb.isEmpty) { continue; }
                if (nb.WallKey(Dungeon.Flip(dir)).key != PIECEKEYS.W) { continue; }

                pick.SetFaulty(true);
                nb.SetFaulty(true);

                if (nb.SectionIndex != sectionIndex)
                {
                    AddConnection(dir, map.Sections[nb.SectionIndex], pick.Coord, nb.Coord, true);
                    return;
                }


            }
        }
        #endregion
    }// EOF CLASS
}
