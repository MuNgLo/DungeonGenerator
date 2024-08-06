using Godot;
using System;
using System.Collections.Generic;
using static System.Collections.Specialized.BitVector32;


namespace Munglo.DungeonGenerator.Sections
{
    public class PathSection : SectionBase
    {
        override public int TileCount => AllPieces().Count;
        override public List<MapPiece> Pieces => AllPieces();
        private Line[] lines;
        private Line LeftSide => lines[0];
        private Line RightSide => lines[lines.Length - 1];
        private bool isFinished = false;
        internal bool IsValid = false;
        internal int Floor => LeftSide.Floor;

        // Corridor things
        [ExportGroup("Corridors")]
        [Export] public int corMaxTotal = 20;
        [Export] public int corMaxStraight = 5;
        [Export] public int corMinStraight = 2;


        public PathSection(SectionbBuildArguments args) : base(args, false)
        {
        }

        public override void Build()
        {
            MapPiece step = map.GetPiece(coord);
            //VerifyWidth(step);
            SetupLines(step);
            int breaker = corMaxTotal + 5;
            int turntimer = rng.Next(corMinStraight, corMaxStraight);
            while (!isFinished)
            {
                turntimer--;
                if (turntimer < 1 && lines[0].Last.isBridge == false) { RollTurn(); turntimer = rng.Next(corMinStraight, corMaxStraight); }
                AddStep(corMaxTotal);
                if (PathBlocked()) { TrimLines(); isFinished = true; }
                breaker--;
                if (breaker < 0)
                {
                    isFinished = true;
                    DungeonGenerator.Log("Path", "CONSTRUCTOR", "Addstep loop hit BREAKER!");
                }
                if (LeftSide.Count >= corMaxTotal) { isFinished = true; }
            }

            // To short so we fail it
            if (TileCount < 1)
            {
                IsValid = false;
                return;
            }

            SealSection();
            BuildStartConnection();
            BuildEndCap();

            IsValid = true;
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
        public override void SealSection(int wallVariant = 0, int floorVariant = 0, int ceilingVariant = 0)
        {
            base.SealSection(1);
        }
        /// <summary>
        /// remember to check state before using returned piece
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        internal MapPiece GetRandomAlongPath(out MAPDIRECTION dir)
        {
            if (sectionDefinition.sizeWidthMax < 2)
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

            if(startpieceConnection.SectionIndex >= 0)
            {
                AddConnection(startpieceConnection.SectionIndex, Dungeon.Flip(LeftSide.First.Orientation), LeftSide.First.Coord, true);
            }
        }
        private void BuildEndCap()
        {
            MapPiece endpieceConnection = LeftSide.Last.Neighbour(LeftSide.Last.Orientation);
            LeftSide.Last.AddDebug(new KeyData() { key = PIECEKEYS.DEBUGPATHEND, dir = LeftSide.Last.Orientation });
            RightSide.Last.AddDebug(new KeyData() { key = PIECEKEYS.DEBUGPATHEND, dir = RightSide.Last.Orientation });

            if (sectionDefinition.sizeWidthMax < 2)
            {
                CapLineEndsWithWalls();
                if (endpieceConnection.State == MAPPIECESTATE.PENDING && endpieceConnection.SectionIndex >= 0 && endpieceConnection.SectionIndex != sectionIndex)
                {
                    AddConnection(endpieceConnection.SectionIndex, LeftSide.Last.Orientation, LeftSide.Last.Coord, true);
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
                if(!endpieceConnection.isEmpty && endpieceConnection2.isEmpty && endpieceConnection.SectionIndex >= 0 && endpieceConnection.SectionIndex != sectionIndex) 
                {
                    // Add a single door to connect corridors on left side
                    AddConnection(endpieceConnection.SectionIndex, LeftSide.Last.Orientation, LeftSide.Last.Coord, true);
                }
                else if(endpieceConnection.isEmpty && !endpieceConnection2.isEmpty && endpieceConnection2.SectionIndex >= 0 && endpieceConnection2.SectionIndex != sectionIndex)
                {
                    // Add a single door to connect corridors on right side
                    AddConnection(endpieceConnection2.SectionIndex, RightSide.Last.Orientation, RightSide.Last.Coord, true);
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
            MapPiece[] turners = RightSide.GetTurners(sizeX, newDir);
            //ProcGenMKIII.Log("Path", "TurnRight", $"turners.Length[{turners.Length}]");
            if (TurnNotBlocked(turners))
            {
                for (int i = 0; i < turners.Length; i++)
                {
                    turners[i].SectionIndex = sectionIndex;
                    turners[i].Orientation= newDir;
                    turners[i].State = MAPPIECESTATE.PENDING;
                    lines[i].Add(turners[i]);
                    turners[i].Save();
                }
            }
        }
        private void TurnLeft()
        {
            MAPDIRECTION newDir = Dungeon.TwistLeft(lines[0].Last.Orientation);
            MapPiece[] turners = LeftSide.GetTurners(sizeX, newDir, true);
            //ProcGenMKIII.Log("Path", "TurnLeft", $"turners.Length[{turners.Length}]");

            if (TurnNotBlocked(turners))
            {
                for (int i = 0; i < turners.Length; i++)
                {
                    turners[i].SectionIndex = sectionIndex;
                    turners[i].Orientation = newDir;
                    turners[i].State = MAPPIECESTATE.PENDING;
                    lines[i].Add(turners[i]);
                    turners[i].Save();
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
            //GD.Print($"PathSection::SetupLines() SectionName[{SectionName}]  sectionStyle[{SectionStyle}] sizeX[{sizeX}]");
            lines = new Line[sizeX];
            step.SectionIndex = sectionIndex;
            step.State = MAPPIECESTATE.PENDING;

            lines[0] = new Line(this, step, rng);
            if (lines.Length > 1)
            {
                MAPDIRECTION expandTo = Dungeon.TwistRight(step.Orientation);
                for (int i = 1; i < lines.Length; i++)
                {
                    lines[i] = new Line(this, lines[i - 1].Last.Neighbour(expandTo), rng);
                    lines[i].Last.Orientation = step.Orientation;
                    lines[i].Last.State = MAPPIECESTATE.PENDING;
                }
            }
        }
        private void VerifyWidth(MapPiece step)
        {
            int cleared = 0;
            MAPDIRECTION dir = Dungeon.TwistRight(step.Orientation);
            for (int i = 0; i < sizeX; i++)
            {
                if (step.State != MAPPIECESTATE.UNUSED)
                {
                    sizeX = cleared;
                    return;
                }
                cleared++;
                step = step.Neighbour(dir);
            }
            sizeX = cleared;
        }
    }// EOF CLASS
}
