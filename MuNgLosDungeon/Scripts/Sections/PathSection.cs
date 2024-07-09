using Godot;
using System;
using System.Collections.Generic;


namespace Munglo.DungeonGenerator
{
    internal class PathSection : SectionBase
    {
        new public int PropCount => TotalPropCount(); // New needed?
        private int width = 1;
        override public int TileCount => AllPieces().Count; // Does need the new
        override public List<MapPiece> Pieces => AllPieces(); // Does need the new
        private Line[] lines;
        private Line LeftSide => lines[0];
        private Line RightSide => lines[lines.Length - 1];
        private bool isFinished = false;
        internal bool IsValid = false;
        internal int Floor => LeftSide.Floor;

        internal PathSection(MapData mapData, MapPiece step, int sectionID, int width, ulong[] seed, int maxTotal, int maxS, int minS)
       : base(seed, sectionID, "Corridor", "Corridor", mapData)
        {
            orientation = step.Orientation;
            coord = step.Coord;

            this.width = width;
            VerifyWidth(step);
            if (this.width < 1) { return; }
            SetupLines(step);
            int breaker = 100;
            int turntimer = rng.Next(minS, maxS);
            while (!isFinished)
            {
                turntimer--;
                if (turntimer < 1 && lines[0].Last.isBridge == false) { RollTurn(); turntimer = rng.Next(minS, maxS); }
                AddStep(maxTotal);
                if (PathBlocked()) { TrimLines(); isFinished = true; }
                breaker--;
                if (breaker < 0)
                {
                    isFinished = true;
                    DungeonGenerator.Log("Path", "CONSTRUCTOR", "Addstep loop hit BREAKER!");
                }
                if (LeftSide.Count >= maxTotal) { isFinished = true; }
            }

            // To short so we fail it
            if (TileCount < 1)
            {
                IsValid = false;
                return;
            }

            AddKeys();
            BuildStartConnection();
            BuildEndCap();
            //for (int i = 0; i < lines.Length; i++)
            //{
            //    pieces.AddRange(lines[i].Steps);
            //}

            IsValid = true;
        }


        public override int TotalPropCount()
        {
            int count = 0;
            foreach (var piece in AllPieces()) { count += piece.Props.Count; }
            return count;
        }

        private List<MapPiece> AllPieces()
        {
            List<MapPiece> a = new List<MapPiece>();
            for (int i = 0; i < lines.Length; i++)
            {
                a.AddRange(lines[i].Steps);
            }
            return a;
        }

        private int TotalCount()
        {
           int a = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                a += lines[i].Count;
            }
            return a;
        }

   

        private void TrimLines()
        {
            int length = LeftSide.Count;
            for (int i = 0; i < lines.Length; i++)
            {
                length = Math.Min(length, lines[i].Count);
            }
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].TrimToLength(length);
            }
        }

        internal MapPiece GetRandomAlongPathNoCorner(out MAPDIRECTION dir)
        {
            MapPiece result = null;
            dir = MAPDIRECTION.ANY;
            // Check for corner and go recursive 10x before failing
            int breaker = 10;
            while (breaker > 0)
            {
                breaker--;
                result = GetRandomAlongPath(out dir);

                if(result is not null && !result.IsCorner(dir)) { return result; }

                if (breaker < 1) { break; }
            }
            return null;
        }

        /// <summary>
        /// remember to check state before using returned piece
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        internal MapPiece GetRandomAlongPath(out MAPDIRECTION dir)
        {
            if (width < 2)
            {
                return LeftSide.GetRandomAlongPath(out dir);
            }
            // Pick from wider paths
            int max = LeftSide.Count + RightSide.Count;
            int pick = rng.Next(max);
            if (pick < LeftSide.Count)
            {
                return LeftSide.GetRandomAlongPath(out dir, true, false);

            }
            else
            {
                return RightSide.GetRandomAlongPath(out dir, false, true);
            }
        }
        private void BuildStartConnection()
        {
            MapPiece startpieceConnection = LeftSide.First.Neighbour(Dungeon.Flip(LeftSide.First.Orientation));

            if (width < 2)
            {
                if (startpieceConnection.SectionIndex > -1)
                {
                    // ask room if connecting with a door is acceptable
                    if (map.AddOpeningToSection(startpieceConnection, LeftSide.First.Orientation, false, true))
                    {
                        LeftSide.First.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = Dungeon.Flip(LeftSide.First.Orientation) }, true);
                        return;
                    }
                }
                else
                {
                    if(rng.Next(0, 100) < 25)
                    {
                        startpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = LeftSide.First.Orientation }, true);
                        LeftSide.First.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = Dungeon.Flip(LeftSide.First.Orientation) }, true);
                    }
                    else
                    {
                        startpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = LeftSide.First.Orientation }, true);
                        LeftSide.First.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(LeftSide.First.Orientation) }, true);
                    }
                    return;
                }
            }
            else
            {
                // Wide Corridor start connection
                if (startpieceConnection.SectionIndex > -1)
                {
                    // Started from a room
                    if (map.AddOpeningToSection(startpieceConnection, LeftSide.First.Orientation, true, true))
                    {
                        LeftSide.First.AssignWall(new KeyData() { key = PIECEKEYS.OCCUPIED, dir = Dungeon.Flip(LeftSide.First.Orientation) }, false); // No lock override needed
                        RightSide.First.AssignWall(new KeyData() { key = PIECEKEYS.WDW, dir = Dungeon.Flip(RightSide.First.Orientation) }, false); // No lock override needed
                        return;
                    }
                }
                else
                {
                    MapPiece startpieceConnection2 = RightSide.First.Neighbour(Dungeon.Flip(RightSide.First.Orientation));
                    // Check seconday start piecve and adjust if needed
                    if (startpieceConnection2.isEmpty)
                    {
                        startpieceConnection.AssignWall(new KeyData(){ key = PIECEKEYS.NONE, dir = Dungeon.TwistRight(LeftSide.First.Orientation) }, true);
                        startpieceConnection2.Orientation = startpieceConnection.Orientation;
                        startpieceConnection2.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = Dungeon.TwistRight(LeftSide.First.Orientation) }, false);
                        startpieceConnection2.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = Dungeon.Flip(LeftSide.First.Orientation) }, false);
                        startpieceConnection2.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = startpieceConnection2.Orientation };
                    }

                    // Started from anything else
                    if (rng.Next(0, 100) < 222)
                    {
                        map.AddDoorWide(LeftSide.First, false);
                    }
                    else
                    {
                        startpieceConnection.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = LeftSide.First.Orientation }, true);
                        startpieceConnection2.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = RightSide.First.Orientation }, true);
                        LeftSide.First.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(LeftSide.First.Orientation) }, true);
                        RightSide.First.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(RightSide.First.Orientation) }, true);
                    }
                    return;
                }
            }
        }
        private void BuildEndCap()
        {
            MapPiece endpieceConnection = LeftSide.Last.Neighbour(LeftSide.Last.Orientation);
            LeftSide.Last.AddDebug(new KeyData() { key = PIECEKEYS.DEBUGPATHEND, dir = LeftSide.Last.Orientation });
            RightSide.Last.AddDebug(new KeyData() { key = PIECEKEYS.DEBUGPATHEND, dir = RightSide.Last.Orientation });

            if (width < 2)
            {
                CapLineEndsWithWalls();
                if (endpieceConnection.State == MAPPIECESTATE.PENDING)
                {
                    map.AddDoor(LeftSide.Last, LeftSide.Last.Neighbour(LeftSide.Last.Orientation), false);
                }
                return;
            }
            MapPiece endpieceConnection2 = RightSide.Last.Neighbour(RightSide.Last.Orientation);

            if (endpieceConnection.State == MAPPIECESTATE.PENDING && endpieceConnection2.State == MAPPIECESTATE.PENDING)
            {
                // Avoid messing with start connection if path is only 1 tile long
                if (LeftSide.Count > 1)
                {
                    if (LeftSide.Last.isBridge)
                    {
                        LeftSide.Last.AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = LeftSide.Last.Orientation }, true);
                    }
                    else
                    {
                        LeftSide.Last.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = LeftSide.Last.Orientation }, true);
                    }
                    if (!RightSide.Last.isEmpty)
                    {
                        RightSide.Last.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(RightSide.Last.Orientation) }, true);
                    }
                }
                if (LeftSide.Last.isBridge)
                {
                    LeftSide.Last.Neighbour(LeftSide.Last.Orientation).AssignWall(new KeyData() { key = PIECEKEYS.WD, dir = Dungeon.Flip(LeftSide.Last.Orientation) }, true);
                }
                else
                {
                    LeftSide.Last.Neighbour(LeftSide.Last.Orientation).AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(LeftSide.Last.Orientation) }, true);
                    RightSide.Last.Neighbour(LeftSide.Last.Orientation).AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.Flip(RightSide.Last.Orientation) }, true);
                }
            }
            else
            {
                CapLineEndsWithWalls();
                if(!endpieceConnection.isEmpty && endpieceConnection2.isEmpty && endpieceConnection.SectionIndex < 0) 
                {
                    // Add a single door to connect corridors on left side
                    map.AddDoor(LeftSide.Last, endpieceConnection, false);
                }
                else if(endpieceConnection.isEmpty && !endpieceConnection2.isEmpty && endpieceConnection2.SectionIndex < 0)
                {
                    // Add a single door to connect corridors on right side
                    map.AddDoor(RightSide.Last, endpieceConnection2, false);
                }
            }
        }
        private void CapLineEndsWithWalls()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                MAPDIRECTION dir = lines[i].Last.Orientation;
                MapPiece next = lines[i].Last.Neighbour(dir);
                if (next.isEmpty && next.SectionIndex < 0)
                {
                    lines[i].Last.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = lines[i].Last.Orientation }, true);
                }
                if (next.HasWall(Dungeon.Flip(dir)))
                {
                    lines[i].Last.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = lines[i].Last.Orientation }, true);
                }
            }
        }
        private void RollTurn()
        {
            if (rng.Next(100) < 40)
            {
                bool right = true;
                if (rng.Next(100) < 50)
                {
                    right = false;
                }
                if (right) { TurnRight(); } else { TurnLeft(); }
            }
        }
        private void TurnRight()
        {
            MAPDIRECTION newDir = Dungeon.TwistRight(lines[0].Last.Orientation);
            MapPiece[] turners = RightSide.GetTurners(width, newDir);
            //ProcGenMKIII.Log("Path", "TurnRight", $"turners.Length[{turners.Length}]");
            if (TurnNotBlocked(turners))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i].Last.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = lines[i].Last.Orientation }, false);
                }
                for (int i = 0; i < turners.Length; i++)
                {
                    turners[i].AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = newDir }, false);
                    turners[i].Orientation= newDir;
                    turners[i].State = MAPPIECESTATE.PENDING;
                    lines[i].Add(turners[i]);
                    turners[i].Neighbour(Dungeon.Flip(newDir)).AssignWall(new KeyData() { key = PIECEKEYS.OCCUPIED, dir = newDir }, false);
                }
            }
        }
        private void TurnLeft()
        {
            MAPDIRECTION newDir = Dungeon.TwistLeft(lines[0].Last.Orientation);
            MapPiece[] turners = LeftSide.GetTurners(width, newDir, true);
            //ProcGenMKIII.Log("Path", "TurnLeft", $"turners.Length[{turners.Length}]");

            if (TurnNotBlocked(turners))
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    lines[i].Last.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = lines[i].Last.Orientation }, false);
                }
                for (int i = 0; i < turners.Length; i++)
                {
                    turners[i].AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = newDir }, false);
                    turners[i].Orientation = newDir;
                    turners[i].State = MAPPIECESTATE.PENDING;
                    lines[i].Add(turners[i]);
                    turners[i].Neighbour(Dungeon.Flip(newDir)).AssignWall(new KeyData() { key = PIECEKEYS.OCCUPIED, dir = newDir }, false);
                }
            }
        }
        private bool TurnNotBlocked(MapPiece[] turners)
        {
            for (int i = 0; i < turners.Length; i++)
            {
                if (turners[i].State != MAPPIECESTATE.UNUSED)
                {
                    return false;
                }
            }
            return true;
        }

      
        private void AddKeys()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                // Main Line
                if(i == 0) { 
                    lines[i].AddWallsLeft(); 
                }
                lines[i].AddTransitionDoors(i == 0);
                if(i == lines.Length - 1) { lines[i].AddWallsRight(); }
                lines[i].AddFloorAndCeilingKeys(i == 0);
            }
        }

        private bool PathBlocked()
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Blocked) { return true; }
            }
            return false;
        }

        private void AddStep(int maxSteps)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].Walk(maxSteps, i==0);
            }
        }

        private void SetupLines(MapPiece step)
        {
            lines = new Line[width];
            step.State = MAPPIECESTATE.PENDING;
            lines[0] = new Line(step, rng);
            if (lines.Length > 1)
            {
                MAPDIRECTION expandTo = Dungeon.TwistRight(step.Orientation);
                for (int i = 1; i < lines.Length; i++)
                {
                    lines[i] = new Line(lines[i - 1].Last.Neighbour(expandTo), rng);
                    lines[i].Last.Orientation = step.Orientation;
                    lines[i].Last.State = MAPPIECESTATE.PENDING;
                }
            }
        }
        private void VerifyWidth(MapPiece step)
        {
            int cleared = 0;
            MAPDIRECTION dir = Dungeon.TwistRight(step.Orientation);
            for (int i = 0; i < width; i++)
            {
                if (step.State != MAPPIECESTATE.UNUSED)
                {
                    width = cleared;
                    return;
                }
                cleared++;
                step = step.Neighbour(dir);
            }
            width = cleared;
        }

        public override void Build()
        {
            throw new NotImplementedException();
        }

        public override bool AddOpening(MapCoordinate coord, MAPDIRECTION dir, bool wide, bool overrideLocked)
        {
            throw new NotImplementedException();
        }
    }// EOF CLASS
}
