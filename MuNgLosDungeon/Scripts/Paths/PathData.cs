using System;
using System.Collections.Generic;
using System.Linq;

namespace Munglo.DungeonGenerator
{
    internal class PathData
    {
        private List<MapPiece> path = new List<MapPiece>();
        private List<MAPDIRECTION> dirs = new List<MAPDIRECTION>();
        private List<bool> turns = new List<bool>();
        private WATERAMOUNT water;
        private PATHSIZE pSize;
        internal PATHSIZE Size=> pSize;
        private bool isFinished = false;
        internal MapCoordinate Start => path.First().Coord;
        internal MapCoordinate End => path.Last().Coord;
        internal int Length => path.Count;
        private List<MapPiece> Path => path;
        private PRNGMarsenneTwister rng;
        private MapData map;
        internal WATERAMOUNT Water => water;

        private int lastDrop = 0;
        private int maxTotal = 20;

        private bool CanDrop => lastDrop < Path.Count - 3;
        private int Count => path.Count;
        private MapPiece FirstStep => (pSize != PATHSIZE.MEDIUM && pSize != PATHSIZE.LARGE) ? path[0] : path[1];
        private MapPiece FirstExpandedStep => (pSize == PATHSIZE.MEDIUM || pSize == PATHSIZE.LARGE) ? path[0] : null;
        private MapPiece LastExpandedStep => (pSize == PATHSIZE.MEDIUM || pSize == PATHSIZE.LARGE) ? path[path.Count - 2] : null;

        private int straightMin = 3;
        private int StraightMin => (pSize == PATHSIZE.MEDIUM || pSize == PATHSIZE.LARGE) ? Math.Max(straightMin, 4) : Math.Max(straightMin, 2);
        private int straightMax = 3;
        private int StraightMax => (pSize == PATHSIZE.MEDIUM || pSize == PATHSIZE.LARGE) ? Math.Max(straightMax, StraightMin + 2) : Math.Max(straightMax, StraightMin +1);

        public PathData(MapData mapData, MapPiece step, MAPDIRECTION startdir, PATHSIZE size, ulong[] seed, int maxTotal, int maxS, int minS) {
            // Any path has to start on unused piece
            if(step.State != MAPPIECESTATE.UNUSED) {
                //DungeonGenerator.Log("PathData", "CONSTRUCTOR", $"Path creation failed. First step not unused. State[{step.State}]");    
                return; 
            }
            // Initilizing things we need
            map = mapData;
            straightMin = minS;
            straightMax = maxS;
            rng = new PRNGMarsenneTwister(seed); // This paths seeded rng
            path = new List<MapPiece>();
            dirs = new List<MAPDIRECTION>() { startdir };
            pSize = size;
            this.maxTotal = maxTotal;
            // pick which way to turn first
            bool left = true;
            bool flipped = false;
            if (rng.Next(100) < 50)
            {
                left = false;
            }
            turns.Add(left);
            // resolve expand side
            MAPDIRECTION expandTo = left ? Dungeon.TwistLeft(startdir) : Dungeon.TwistRight(startdir);
            int breaker = maxTotal * 2;
            // Build path until finished
            while (!isFinished && breaker > 0)
            {

                // Build straight line. It includes the first step
                List<MapPiece> line = Walk(step, dirs.Last(), rng.Next(StraightMin, StraightMax), expandTo);
                // Process the returned line we walked
                foreach (MapPiece linePiece in line)
                {
                    // Add extra tile for wider path
                    if (pSize == PATHSIZE.MEDIUM || pSize == PATHSIZE.LARGE)
                    {
                        MapPiece extraPiece = linePiece.Neighbour(expandTo);
                        // Skip already pathed piece after a turn
                        if (path.Exists(p => p.Coord == extraPiece.Coord))
                        {
                            if (!flipped)
                            {
                                path[path.Count - 4].AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = expandTo }, false);
                                path[path.Count - 4].AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.TwistRight(expandTo) }, false);
                                path[path.Count - 4].AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.TwistLeft(expandTo) }, false);
                                path[path.Count - 4].AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(expandTo) }, false);
                            }
                            else
                            {
                                extraPiece.Orientation = linePiece.Orientation;
                                //path.Find(p => p.X == extraPiece.X && p.Y == extraPiece.Y && p.Z == extraPiece.Z).SetAllOpen();
                                extraPiece.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = expandTo }, false);
                                extraPiece.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.TwistRight(expandTo) }, false);
                                extraPiece.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.TwistLeft(expandTo) }, false);
                                extraPiece.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(expandTo) }, false);
                            }
                            flipped = false;
                        }
                        else
                        {
                            if (flipped)
                            {
                                //Path[path.Count - 3].SetFaulty(true);
                                Path[path.Count - 3].AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = dirs.Last() }, false);
                                flipped = false;
                            }


                            extraPiece.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = dirs.Last() }, false);
                            extraPiece.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(dirs.Last()) }, false);
                            extraPiece.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(expandTo) }, false);
                            extraPiece.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = expandTo }, false);
                            extraPiece.Orientation = dirs.Last();
                            extraPiece.State = MAPPIECESTATE.PENDING;
                            path.Add(extraPiece);
                            map.SavePiece(extraPiece);

                        }
                        //ProcGenMKIII.Log("PathData", "CONSTRUCTOR", $"Ryser ryser bline.Coun[{bline.Count}]");

                        linePiece.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = Dungeon.Flip(expandTo) }, false);
                        linePiece.State = MAPPIECESTATE.PENDING;
                        path.Add(linePiece);
                        map.SavePiece(linePiece);
                    }
                    else
                    {
                        // 1 tile wide path
                        linePiece.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = expandTo }, false);
                        linePiece.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = Dungeon.Flip(expandTo) }, false);
                        linePiece.State = MAPPIECESTATE.PENDING;
                        path.Add(linePiece);
                        map.SavePiece(linePiece);
                    }
                } 
            

                // TURN
                if (rng.Next(100) < 80 && Count > 4 && Count < maxTotal - 2 && !isFinished && path.Count > 0)
                {
                    MAPDIRECTION turnDirection = left ? Dungeon.TwistLeft(dirs.Last()) : Dungeon.TwistRight(dirs.Last());
                    // SKip turn if blocked
                    bool skipTurn = false;
                    if (pSize == PATHSIZE.MEDIUM || pSize == PATHSIZE.LARGE && flipped)
                    {
                        MapPiece turnBlocker = path.Last().Neighbour(dirs.Last());
                        if(turnBlocker.State != MAPPIECESTATE.UNUSED)
                        {
                            skipTurn = true;
                        }
                    }
                    if (!skipTurn)
                    {
                        dirs.Add(turnDirection);
                        // cap off ends
                        if (pSize == PATHSIZE.MEDIUM || pSize == PATHSIZE.LARGE)
                        {
                            LastExpandedStep.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = dirs.Last() }, false);
                            LastExpandedStep.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = !left ? Dungeon.TwistLeft(turnDirection) : Dungeon.TwistRight(turnDirection) }, false);
                        }

                        turns.Add(left);
                        path.Last().Orientation = dirs.Last();

                        path.Last().AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = dirs.Last() }, false);
                        path.Last().AssignWall(new KeyData() { key = PIECEKEYS.W, dir = !left ? Dungeon.TwistLeft(turnDirection) : Dungeon.TwistRight(turnDirection) }, false);


                        // update expand side
                        expandTo = left ? Dungeon.TwistLeft(dirs.Last()) : Dungeon.TwistRight(dirs.Last());

                        bool thisTurn = left;
                        // pick which way to turn next
                        if (rng.Next(100) < 50)
                        {
                            left = false;
                        }
                        else
                        {
                            left = true;
                        }
                        if (dirs.Count > 2)
                        {
                            // Turns flipped
                            MAPDIRECTION current = dirs[dirs.Count - 2];
                            MAPDIRECTION last = dirs[dirs.Count - 3];
                            if (Dungeon.TwistLeft(last) == current)
                            {
                                if (!thisTurn)
                                {
                                    flipped = true;
                                }
                            }
                            else if (Dungeon.TwistRight(last) == current)
                            {
                                if (thisTurn)
                                {
                                    flipped = true;
                                }
                            }
                            //ProcGenMKIII.Log("PathData", "Constructor", $"FLIPP[{flipped}] TurningLeft[{thisTurn}] Last Turn was [{(Proc.TwistRight(last) == current ? "R" : "L")}]");

                        }
                    }
                    else
                    {
                        //ProcGenMKIII.Log("PathData", "Constructor", $"FLIPP[{flipped}] Skipped Turn!");
                    }
                }
                
                if(Count < 1) {  isFinished= true; }
                if (!isFinished )
                {
                    isFinished = Count >= maxTotal;
                    // get next step for next iteration
                    step = path.Last().Neighbour(path.Last().Orientation);
                }
                breaker--;
                if(breaker < 0)
                {
                    DungeonGenerator.Log("PathData", "CONSTRUCTOR", $"Path loop tripped breaker!");
                }
            }
            if (Count > 0)
            {
                BuildStartConnection();
                BuildEndCap(flipped);
            }

            //ProcGenMKIII.Log("PathData", "CONSTRUCTOR", $"Last piece Key[{path.Last().walls[(int)dirs.Last() - 1].key}]");
            //ProcGenMKIII.Log("PathData", "CONSTRUCTOR", ToString());
        }

        private void BuildStartConnection()
        {
            // Check if we starting from a room, if so open it up
            MapPiece startpieceConnection = FirstStep.Neighbour(Dungeon.Flip(FirstStep.Orientation));
            MapPiece startexpandpieceConnection = FirstExpandedStep.Neighbour(Dungeon.Flip(FirstStep.Orientation));

            if (startpieceConnection.SectionIndex > 0)
            {
                bool wasFirstTurnLeft = FirstExpandedStep == FirstStep.Neighbour(Dungeon.TwistLeft(FirstStep.Orientation));
                if (map.AddOpeningToSection(!wasFirstTurnLeft || startexpandpieceConnection == null ? startpieceConnection : startexpandpieceConnection, dirs.First(), Size == PATHSIZE.MEDIUM || Size == PATHSIZE.LARGE, false))
                {
                    if (Size == PATHSIZE.MEDIUM || Size == PATHSIZE.LARGE)
                    {
                        FirstStep.AssignWall(new KeyData() { key = wasFirstTurnLeft ? PIECEKEYS.WDW : PIECEKEYS.NONE, dir = Dungeon.Flip(FirstStep.Orientation) }, false);
                        //startpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = FirstStep.orientation });
                        if (FirstExpandedStep != null)
                        {
                            FirstExpandedStep.AssignWall(new KeyData() { key = !wasFirstTurnLeft ? PIECEKEYS.WDW : PIECEKEYS.NONE, dir = Dungeon.Flip(FirstStep.Orientation) }, false);
                            //startexpandpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = FirstStep.orientation });
                        }
                    }
                    else
                    {
                        FirstStep.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = Dungeon.Flip(FirstStep.Orientation) }, false);
                        //startpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = FirstStep.orientation });
                    }
                }
            }
            else
            {
                // Check if we starting from anything, if so open it up
                if (Size == PATHSIZE.MEDIUM || Size == PATHSIZE.LARGE)
                {
                    FirstStep.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(FirstStep.Orientation) }, false);
                    startpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = FirstStep.Orientation }, false);
                    if (FirstExpandedStep != null)
                    {
                        FirstExpandedStep.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(FirstStep.Orientation) }, false);
                        startexpandpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = FirstStep.Orientation }, false);
                    }
                }
                else
                {
                    FirstStep.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = Dungeon.Flip(FirstStep.Orientation) }, false);
                    startpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = FirstStep.Orientation }, false);
                }
            }
        }

        private void BuildEndCap(bool flipped)
        {
            // Check end of path for potential connections
            MAPDIRECTION endOrientation = path.Last().Orientation;
            MapPiece endpieceConnection = path.Last().Neighbour(endOrientation);
            MapPiece endexpandpieceConnection = LastExpandedStep.Neighbour(endOrientation);

            //ProcGenMKIII.Log("PathData", "BuildEndCap", $"Path looking to connect end. endpieceConnection[{endpieceConnection}][{endpieceConnection.State}]");

            bool waslastTurnLeft = LastExpandedStep == path.Last().Neighbour(Dungeon.TwistLeft(path.Last().Orientation));
            if ((!flipped || endexpandpieceConnection == null ? endpieceConnection : endexpandpieceConnection).SectionIndex > 0)
            {
                if (map.AddOpeningToSection(flipped || endexpandpieceConnection == null ? endpieceConnection : endexpandpieceConnection, Dungeon.Flip(endOrientation), Size == PATHSIZE.MEDIUM || Size == PATHSIZE.LARGE, false))
                {
                    if (endexpandpieceConnection != null)
                    {
                        if (flipped)
                        {
                            path.Last().AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = path.Last().Orientation }, false);
                            LastExpandedStep.AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = path.Last().Orientation }, false);
                        }
                        else
                        {
                            path.Last().AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = path.Last().Orientation }, false);
                            LastExpandedStep.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = path.Last().Orientation }, false);
                        }
                    }
                    else
                    {
                        path.Last().AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = endOrientation }, false);
                        //startpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = FirstStep.orientation });
                    }
                }
            }
            else if (endpieceConnection.State != MAPPIECESTATE.UNUSED)
            {
                // Check if we can connect the end, if so open it up
                if (endexpandpieceConnection != null)
                {
                    if (flipped || dirs.Count < 2)
                    {
                        if (WDWAllowed(path.Last(), flipped))
                        {
                            path.Last().AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = endOrientation }, false);
                            LastExpandedStep.AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = endOrientation }, false);
                            
                            endpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = Dungeon.Flip(endOrientation) }, false);
                            endexpandpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(endOrientation) }, false);

                        }
                        else
                        {
                            path.Last().AssignWall(new KeyData() { key = PIECEKEYS.W, dir = endOrientation }, false);
                            LastExpandedStep.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = endOrientation }, false);
                        }
                    }
                    else
                    {
                        
                        path.Last().AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = endOrientation }, false);
                        LastExpandedStep.AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = endOrientation }, false);
                        
                        endpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = Dungeon.Flip(endOrientation) }, false);
                        endexpandpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(endOrientation) }, false);

                    }
                    //endpieceConnection.SetError(true);
                    //endexpandpieceConnection.SetFaulty(true);
                }
                else
                {
                 
                    // Confirmed 
                    path.Last().AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = endOrientation }, false);
                    endpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = Dungeon.Flip(endOrientation) }, false);
                }
                
            }
            else
            {
                path.Last().AssignWall(new KeyData() { key = PIECEKEYS.W, dir = endOrientation }, false);
                if (LastExpandedStep != null)
                {
                    LastExpandedStep.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = endOrientation }, false);
                }
            }
        }

        private bool WDWAllowed(MapPiece piece, bool flipped)
        {
            MAPDIRECTION dirToCheck = flipped ? Dungeon.TwistRight(piece.Orientation) : Dungeon.TwistLeft(piece.Orientation);
            bool result = true;
            if (piece.Neighbour(piece.Orientation).WallKey(dirToCheck).key != PIECEKEYS.NONE)
            {
                result = false;
            };
            if (piece.Neighbour(piece.Orientation).Neighbour(piece.Orientation).WallKey(dirToCheck).key == PIECEKEYS.WDW)
            {
                result = false;
            };
            DungeonGenerator.Log("PathData", "WDWAllowed", $"piece[{piece}] result[{result}]");
            return result;
        }

        internal void SetPathInfo(PATHSIZE size, WATERAMOUNT w = WATERAMOUNT.NONE)
        {
            water = w;
            for (int i = 0; i < path.Count; i++)
            {
                path[i].water = water;
            }
        }

        private List<MapPiece> Walk(MapPiece step, MAPDIRECTION dir, int maxPathLength, MAPDIRECTION expandTo)
        {
            List<MapPiece> steps = new List<MapPiece>();
            //ProcGenMKIII.Log("PathData", "Walk", $"Walk[{steps}] dir[{dir}]");
            // walk the line
            for (int i = 1; i < maxPathLength; i++)
            {
                // End line if it makes the path hit its maxlength
                if (Count + steps.Count >= maxTotal) { return steps; }
              
                // if next step has data we stop path
                if (step.State != MAPPIECESTATE.UNUSED && !path.Contains(step))
                {
                    isFinished = true;
                    return steps;
                }

                // how distant along path is it?
                if (path.Contains(step))
                {
                    if (path.Count - path.IndexOf(step) > 2)
                    {
                        // to far back to be connected
                        isFinished = true;
                        return steps;
                    }
                }

                // If next adjacent step has data we stop path
                if (Size == PATHSIZE.MEDIUM || Size == PATHSIZE.LARGE)
                {
                    // Get adjacent piece to expand to
                    MapPiece expanded = step.Neighbour(expandTo);
                    // If the expanded already has data we stop path;
                    if (expanded.State != MAPPIECESTATE.UNUSED)
                    {
                        // how distant along path is it?
                        if (path.Contains(expanded))
                        {
                            if(path.Count - path.IndexOf(expanded) > 4)
                            {
                                // to far back to be connected
                                isFinished = true;
                                return steps;
                            }
                        }
                        else
                        {
                            isFinished = true;
                            return steps;
                        }
                    }
                }

                 
                step.Orientation = dir;
                step.State = MAPPIECESTATE.PENDING;
                steps.Add(step);
                // Get/Create next step in path
                step =  step.Neighbour(dir);
            }
            return steps;
        }
        public override string ToString()
        {
            return $"Length[{Length}]::DIRS[{string.Join('.', dirs)}]";
        }

        internal MapPiece GetRandomAlongPath(bool openPathToIt)
        {
            int pick = rng.Next(Count);
            MapPiece pathTile = path[pick];
            MAPDIRECTION dir = pathTile.Orientation;
            MapPiece piece = GetRandomAlongPath(out dir);
            //ProcGenMKIII.Log("PathData", "GetRandomAlongPath", "FAILED!");
            return piece;
        }
        internal MapPiece GetRandomAlongPath(out MAPDIRECTION dir)
        {
            int pick = rng.Next(Count);
            MapPiece pathTile = path[pick];
            dir = pathTile.Orientation;

            dir = rng.Next(100) < 50 ? Dungeon.TwistRight(dir) : Dungeon.TwistLeft(dir);

            MapPiece sideTile = pathTile.Neighbour(dir);
            
            if(sideTile != null && Path.Exists(p=>p.Coord == sideTile.Coord)) 
            {
                sideTile = sideTile.Neighbour(dir);
            }
            
            if(sideTile != null && sideTile.State != MAPPIECESTATE.UNUSED)
            {
                dir = Dungeon.Flip(dir);
                sideTile = pathTile.Neighbour(dir);
                if (Path.Exists(p => p.Coord == sideTile.Coord))
                {
                    sideTile = sideTile.Neighbour(dir);
                }
            }

            
            if (sideTile != null && sideTile.State == MAPPIECESTATE.UNUSED)
            {
                sideTile.Orientation = dir;
                return sideTile;
            }
            return null;
        }
    }// EOF CLASS
}
