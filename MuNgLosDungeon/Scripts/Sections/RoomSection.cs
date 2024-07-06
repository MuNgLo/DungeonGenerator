using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Munglo.DungeonGenerator
{
    public class RoomSection : SectionBase
    {
        private RoomResource roomDef;
        private protected bool centerSpiralStairs;

        internal RoomSection(MapData mapData, MapCoordinate startLoc, MAPDIRECTION dir, RoomResource roomDef, int index, ulong[] seed) : base(seed, index, roomDef.roomStyle, roomDef.roomName, mapData)
        {
            this.roomDef = roomDef;
            coord = startLoc;
            orientation = dir;
            centerSpiralStairs = roomDef.centerSpiralStairs;
            defaultConnectionResponses = roomDef.defaultResponses;
            sizeX = rng.Next(roomDef.sizeWidthMin, roomDef.sizeWidthMax + 1);
            sizeZ = rng.Next(roomDef.sizeDepthMin, roomDef.sizeDepthMax + 1);
            sizeY = rng.Next(roomDef.nbFloorsMin, roomDef.nbFloorsMax + 1);
            if (dir == MAPDIRECTION.ANY) { orientation = (MAPDIRECTION)rng.Next(1, 5); }
            if (orientation != MAPDIRECTION.NORTH || orientation != MAPDIRECTION.SOUTH)
            {
                int d = sizeZ;
                sizeZ = sizeX;
                sizeX = d;
            }
            SetMinMaxCoord();
        }

        #region ISection methods
        public override void Build()
        {
            MapPiece start = map.GetPiece(coord);
            start.State = MAPPIECESTATE.PENDING;
            pieces.Add(start);
            start.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = orientation, variantID = 0 };
            map.SavePiece(start);
            //ProcGenMKIII.Log("RoomBase", $"BuildRoom", $"Loc{coord}  Size({sizeX}.{sizeY}.{sizeZ}) minX({minX}) maxX({maxX})");
            int breaker = 0;
            while (pieces.Exists(p => p.State == MAPPIECESTATE.PENDING))
            {
                ProcessPiece(pieces.Find(p => p.State == MAPPIECESTATE.PENDING));
                breaker++;
                if (breaker > 1000)
                {
                    DungeonGenerator.Log("RoomBase", $"BuildRoom", $"ProcessPiece loop hit breaker!");
                    break;
                }
            }


            SealSection();


            if (roomDef.firstPieceDoor)
            {
                pieces.First().AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = Dungeon.Flip(pieces.First().Orientation) }, true);
                pieces.First().Neighbour(Dungeon.Flip(pieces.First().Orientation)).AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = pieces.First().Orientation }, true);
            }

            if (pieces.Count < 10)
            {
                foreach (MapPiece piece in pieces)
                {
                    if (piece.hasFloor)
                    {
                        piece.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = piece.Orientation, variantID = 2 };
                    }
                    if(piece.WallKey(Dungeon.Flip(piece.Orientation)).key == PIECEKEYS.WD)
                    {
                        piece.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = Dungeon.Flip(piece.Orientation), variantID = 1 }, true);
                    }
                }
            }
            else
            {
                // Roll cracked floor
                int crackedCount = pieces.Count / 20;
                for (int i = 0; i < crackedCount + 1; i++)
                {
                    if (rng.Next(100) < 20)
                    {
                        MapPiece rp = GetRandomFloor();
                        rp.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = rp.Orientation, variantID = 1 };
                    }
                }
            }

            if(pieces.Count == 1)
            {
                // One tile room so add alkov on opposite wall
                pieces[0].AssignWall(new KeyData() { key= PIECEKEYS.W, dir = orientation, variantID = 2 }, true);
            }
            else
            {
                // roll chance for alkov
                if(rng.Next(100) < 40)
                {
                    List<MapPiece> candidates = GetWallPieces(0, true);
                    if(candidates.Count > 0)
                    {
                        breaker = 10;
                        while (breaker > 0)
                        {
                            breaker--;
                            if(candidates.Count < 1) { break; }
                            MapPiece pick = candidates[rng.Next(candidates.Count)];
                            if(pick.WallKey(pick.OutsideWallDirection()).key == PIECEKEYS.W)
                            {
                                pick.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = pick.OutsideWallDirection(), variantID = 2 }, true);
                                break;
                            }
                            candidates.Remove(pick);
                        }
                    }
                }
            }
            

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
                MapPiece nb = rp.Neighbour(processingDirection);
                if (nb.State == MAPPIECESTATE.UNUSED)
                {
                    if ((nb.Coord.x >= minX && nb.Coord.x <= maxX
                        && nb.Coord.y >= MinY && nb.Coord.y < MaxY
                        && nb.Coord.z >= minZ && nb.Coord.z <= maxZ)
                        )
                    {
                        // Not bottomfloor so have to have same sectionindex underneath
                        if(nb.Coord.y > MinY && !pieces.Exists(p => p.Coord == nb.Coord + MAPDIRECTION.DOWN))
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

                        if (nb.sectionfloor == 0 || (roomDef.allFloor && !nb.hasFloor && nb.sectionfloor < sizeY - 1))
                        {
                            //nb.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = orientation, variantID = 0 };
                        }

                        pieces.Add(nb);
                        map.SavePiece(nb);
                    }
                    else
                    {
                        if(rp.Coord + MAPDIRECTION.UP == nb.Coord)
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


        public override void BuildProps()
        {
            if (centerSpiralStairs)
            {
                AddCentralSpiralStairs();
            }

            //RoomPiece lowerLeftest = GetLowerLeftestCorner();
            //if(lowerLeftest == null) { GD.PrintErr($"RoomBase::BuildProps() Room[{roomIndex}] Failed to find lower left corner! Room.Y[{Coord.y}] firstPiece.Y[{pieces[0].Coord.y}]"); return; }

            //foreach (RoomPiece roomPiece in pieces.FindAll(p=>p.Coord.y == 1 && !p.hasFloor && (p.HasNorthWall || p.HasEastWall || p.HasSouthWall || p.HasWestWall)))
            //{
            //    PropHelper.InsertGallery(this, roomPiece, 1, orientation, 2);
            //}

            //List<RoomPiece> bridges = new List<RoomPiece>();


            //foreach (RoomPiece rp in pieces.FindAll(p => p.isBridge))
            //{
            //    if (rp.NeighbourNorth.isBridge) { bridges.Add(rp); }
            //    if (rp.NeighbourEast.isBridge) { bridges.Add(rp); }
            //    if (rp.NeighbourSouth.isBridge) { bridges.Add(rp); }
            //    if (rp.NeighbourWest.isBridge) { bridges.Add(rp); }
            //    //roomPiece.SetError(true);
            //}
            //foreach (RoomPiece bp in bridges)
            //{
            //    //PropHelper.InsertLayer(this, bp, PIECEKEYS.STAIRPLATFORM, 1, orientation);
            //}



       

         
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
            pick.AddProp(keyData);
            return true;
        }
        public override void PunchBackDoor()
        {
            if (roomDef.backDoorChance < 1 || rng.Next(100) > roomDef.backDoorChance)
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
                MapPiece nb = pick.Neighbour(dir);
                if (nb.isEmpty) { continue; }
                if (nb.WallKey(Dungeon.Flip(dir)).key != PIECEKEYS.W) { continue; }

                pick.SetFaulty(true);
                nb.SetFaulty(true);

                if (nb.SectionIndex != sectionIndex)
                {
                    map.AddOpeningBetweenSections(pick, nb, dir, true);
                    return;
                }

         
            }
        }
        #endregion

       
        private void AddCentralSpiralStairs()
        {
            if (pieces.Count < 1) { GD.PrintErr($"RoomBase::AddCentralSpiralStairs() RoomIndex[{sectionIndex}] Has Only [{pieces.Count}] pieces. Skipping spiral stairs"); return; }

            MapPiece spiralPiece = GetCenterPiece();
            //spiralPiece.SetError(true);
            MapCoordinate startCoord = spiralPiece.Coord;

            if (spiralPiece.hasFloor)
            {
                for (int i = 0; i < sizeY - 1; i++)
                {
                    // clear floors
                    if (i > 0)
                    {
                        pieces.Find(p => p.Coord == startCoord + MapCoordinate.Up * i).keyFloor = new KeyData() { key = PIECEKEYS.NONE, dir = orientation };
                        pieces.Find(p => p.Coord == startCoord + MapCoordinate.Up * i + orientation).keyFloor = new KeyData() { key = PIECEKEYS.NONE, dir = orientation };
                        pieces.Find(p => p.Coord == startCoord + MapCoordinate.Up * i + Dungeon.TwistRight(orientation)).keyFloor = new KeyData() { key = PIECEKEYS.NONE, dir = orientation };
                        pieces.Find(p => p.Coord == startCoord + MapCoordinate.Up * i + orientation + Dungeon.TwistRight(orientation)).keyFloor = new KeyData() { key = PIECEKEYS.NONE, dir = orientation };
                    }


                    int vID = PickSpiralStairVariation(spiralPiece);

                    if (vID == 1)
                    {
                        // check for bridge marked pieces on the floor above and if so mark all surrounding gallery as bridges
                        List<MapPiece> bridgePieces = pieces.FindAll(p => p.isBridge && p.Coord.y == spiralPiece.Coord.y + 1);
                        if (bridgePieces.Count > 0)
                        {
                            MarkGalleryNeighboursAsBridges(spiralPiece);
                        }
                    }

                    AddProp(startCoord + MapCoordinate.Up * i, new RoomProp() { key = PIECEKEYS.STAIRSPIRAL, dir = spiralPiece.Orientation, Offset = new Vector3I(0,0,0), variantID = vID });

                    if (!pieces.Exists(p => p.Coord == spiralPiece.Coord.StepUp))
                    {
                        GD.Print("RoomBase::BuildProps() Grabbing extra above to add to room!");
                        // This might eat up things above later
                        pieces.Add(map.GetPiece(spiralPiece.Coord.StepUp));
                        pieces.Last().State = MAPPIECESTATE.PENDING;
                        pieces.Last().SectionIndex = sectionIndex;
                        pieces.Last().Orientation = spiralPiece.Orientation;
                    }
                    spiralPiece = pieces.Find(p => p.Coord == spiralPiece.Coord.StepUp);
                    if (spiralPiece.SectionIndex != sectionIndex) { spiralPiece.SectionIndex = sectionIndex; }
                    map.SavePiece(spiralPiece);
                }
            }
        }

        private MapPiece GetLowerLeftestCorner()
        {
            List<MapPiece> candidates = pieces.FindAll(p => p.Coord.y == Coord.y && p.HasWall(Dungeon.TwistLeft(orientation)) && p.HasWall(Dungeon.Flip(orientation)));
            if(candidates.Count > 0) { return candidates[0]; }
            return null;
        }

        private void MarkGalleryNeighboursAsBridges(MapPiece piece)
        {
            MAPDIRECTION dir = piece.Orientation;

            MapCoordinate[] locs = new MapCoordinate[8]
            {
                piece.Coord + MAPDIRECTION.UP + Dungeon.Flip(dir),
                piece.Coord + MAPDIRECTION.UP + Dungeon.Flip(dir) + Dungeon.TwistRight(dir),
                piece.Coord + MAPDIRECTION.UP + Dungeon.TwistLeft(dir),
                piece.Coord + MAPDIRECTION.UP + Dungeon.TwistLeft(dir) + dir,
                piece.Coord + MAPDIRECTION.UP + dir + dir,
                piece.Coord + MAPDIRECTION.UP + dir + dir + Dungeon.TwistRight(dir),
                piece.Coord + MAPDIRECTION.UP + Dungeon.TwistRight(dir) + Dungeon.TwistRight(dir),
                piece.Coord + MAPDIRECTION.UP + Dungeon.TwistRight(dir) + Dungeon.TwistRight(dir) + dir
            };

            foreach (MapCoordinate coordinate in locs)
            {
                if(!pieces.Exists(p=>p.Coord == coordinate)){
                    pieces.Add(map.GetPiece(coordinate));
                }
                pieces.Find(p=>p.Coord == coordinate).isBridge = true;
                map.SavePiece(pieces.Find(p => p.Coord == coordinate));
            }
        }
        private int PickSpiralStairVariation(MapPiece piece)
        {
            MAPDIRECTION dir = piece.Orientation;
            // Check for floors that blick gallery
            if(piece.NeighbourUp.Neighbour(Dungeon.Flip(dir)).hasFloor) { return 2; }
            if(map.GetPiece( piece.Coord + Dungeon.Flip(dir) + Dungeon.TwistRight(dir)).hasFloor) { return 2; }
            if (map.GetPiece(piece.Coord + Dungeon.TwistLeft(dir)).hasFloor) { return 2; }
            if (map.GetPiece(piece.Coord + Dungeon.TwistLeft(dir) + dir).hasFloor) { return 2; }
            if (map.GetPiece(piece.Coord + dir + dir).hasFloor) { return 2; }
            if (map.GetPiece(piece.Coord + dir + dir + Dungeon.TwistRight(dir)).hasFloor) { return 2; }
            if (map.GetPiece(piece.Coord + Dungeon.TwistRight(dir) + Dungeon.TwistRight(dir)).hasFloor) { return 2; }
            if (map.GetPiece(piece.Coord + Dungeon.TwistRight(dir) + Dungeon.TwistRight(dir) + dir).hasFloor) { return 2; }

            return 1;
        }
       
        
       
        

    }// EOF CLASS
}
