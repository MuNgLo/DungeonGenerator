using System;
using System.Collections.Generic;
using System.Linq;
namespace Munglo.DungeonGenerator
{
    /// <summary>
    /// Single line of locations used to build paths
    /// </summary>
    internal class Line
    {
        private List<MapPiece> steps;
        private bool isBlocked = false;
        internal int Count => steps.Count;
        internal bool Blocked => isBlocked;
        internal int Floor => steps[0].Coord.y;
        internal Line(MapPiece startPiece, PRNGMarsenneTwister rng)
        {
            steps = new List<MapPiece>();
            steps.Add(startPiece);
            this.rng = rng;
        }
        internal MapPiece First => steps.First();
        internal MapPiece Last => steps.Last();
        internal List<MapPiece> Steps => steps;
        internal MAPDIRECTION Orientation { get => steps.Last().Orientation; set => steps.Last().Orientation = value; }
        private PRNGMarsenneTwister rng;

        internal void Add(MapPiece step)
        {
            steps.Add(step);
        }
        internal void AddFloorAndCeilingKeys(bool mainLine)
        {
            foreach (MapPiece step in steps)
            {
                if (!step.isBridge && step.SectionIndex < 0)
                {
                    step.keyFloor = new KeyData() { key = PIECEKEYS.F, dir = step.Orientation, variantID = 0 };
                    step.keyCeiling = new KeyData() { key = PIECEKEYS.C, dir = step.Orientation, variantID = 0 };
                }
            }
        }
        
        internal void AddTransitionDoors(bool mainLine)
        {
            foreach (MapPiece step in steps)
            {
                // back check
                if(
                    step.Neighbour(Dungeon.Flip(step.Orientation)).isBridge != step.isBridge
                    ||
                    step.Neighbour(Dungeon.Flip(step.Orientation)).SectionIndex != step.SectionIndex
                    )
                {
                    step.AssignWall(new KeyData() { key = mainLine ? PIECEKEYS.WD : PIECEKEYS.W, dir = Dungeon.Flip(step.Orientation) }, true);
                    step.Neighbour(Dungeon.Flip(step.Orientation)).AssignWall(new KeyData() { key = mainLine ? PIECEKEYS.WD : PIECEKEYS.W, dir = step.Orientation }, true);
                }
            }
        }
        

        internal void AddWallsLeft()
        {
            foreach (MapPiece step in steps)
            {
                if (step.SectionIndex < 1)
                {
                    if (step.WallKey(Dungeon.TwistLeft(step.Orientation)).key != PIECEKEYS.OCCUPIED)
                    {
                        step.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = Dungeon.TwistLeft(step.Orientation), variantID = 1 }, false);
                    }
                    else
                    {
                        // Revert occupied set from turns back to none
                        step.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.TwistLeft(step.Orientation) }, false);
                    }
                }
            }
        }
        internal void AddWallsRight()
        {
            foreach (MapPiece step in steps)
            {
                if (step.SectionIndex < 1)
                {
                    if (step.WallKey(Dungeon.TwistRight(step.Orientation)).key != PIECEKEYS.OCCUPIED)
                    {
                        step.AssignWall(new KeyData() { key = PIECEKEYS.W, dir = Dungeon.TwistRight(step.Orientation), variantID = 1 }, false);
                    }
                    else
                    {
                        // Revert occupied set from turns back to none
                        step.AssignWall(new KeyData() { key = PIECEKEYS.NONE, dir = Dungeon.TwistRight(step.Orientation) }, false);
                    }
                }
            }
        }
        /// <summary>
        /// removes steps in line from end
        /// </summary>
        /// <param name="amount"></param>
        internal void RemoveFromEnd(int amount)
        {
            for(int i = 0; i < amount; i++)
            {
                if(steps.Count > 1)
                {
                    steps.RemoveAt(steps.Count - 1);
                }
            }
        }

        internal void Walk(int maxSteps, bool mainline)
        {
            if (steps.Count < 1) { return; }
            if (Last.isBridge) { WalkBridge(maxSteps, mainline); return; }
            WalkNormal(maxSteps, mainline);
        }
        internal void WalkNormal(int maxSteps, bool mainline)
        {
            MapPiece nextStep = Last.Neighbour(Last.Orientation);
            if (nextStep == null) { DungeonGenerator.Log("Line", "WalkNormal", $"steps.Count[{steps.Count}] nextStep is NULL[{nextStep == null}]"); }

            if (nextStep.State != MAPPIECESTATE.UNUSED && nextStep.SectionIndex < 1)  
            {
                isBlocked= true;
                return;
            }
            else if(nextStep.SectionIndex > 0)
            {
                // First step into room
                if (nextStep.Section.BridgeAllowed && maxSteps > steps.Count)
                {
                    if (mainline)
                    {
                        nextStep.isBridge = true;
                        nextStep.Orientation = Last.Orientation;
                        nextStep.State = MAPPIECESTATE.PENDING;
                        steps.Add(nextStep);
                        return;
                    }
                }
                else
                {
                    isBlocked = true;
                    return;
                }
            }
            nextStep.Orientation = Last.Orientation;
            nextStep.State = MAPPIECESTATE.PENDING;
            steps.Add(nextStep);
        }
        internal void WalkBridge(int maxSteps, bool mainline)
        {
            MapPiece nextStep = Last.Neighbour(Last.Orientation);
            if (nextStep == null) { DungeonGenerator.Log("Line", "WalkNormal", $"steps.Count[{steps.Count}] nextStep is NULL[{nextStep == null}]"); }

            if (nextStep.State != MAPPIECESTATE.UNUSED && nextStep.SectionIndex < 1)
            {
                isBlocked = true;
                return;
            }
            else if (nextStep.SectionIndex > 0)
            {
                if (!Last.isBridge)
                {
                    isBlocked = true;
                    return;
                }
                if (Last.SectionIndex != nextStep.SectionIndex && nextStep.Section.BridgeAllowed && (maxSteps < steps.Count))
                {
                    isBlocked = true;
                    return;
                }
                else
                {
                    nextStep.isBridge = true;
                }
            }
            nextStep.Orientation = Last.Orientation;
            nextStep.State = MAPPIECESTATE.PENDING;
            steps.Add(nextStep);
        }

        internal MapPiece[] GetTurners(int width, MAPDIRECTION dir, bool reversed=false)
        {
            List<MapPiece> turners = new List<MapPiece>();
            for (int i = steps.Count -1; i > steps.Count - 1 - width; i--)
            {
                turners.Add(steps[i].Neighbour(dir));
            }
            if (reversed) { turners.Reverse(); }
            return turners.ToArray();
        }
        /// <summary>
        /// Returns a neighbour piece along the path
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="leftside"></param>
        /// <param name="rightside"></param>
        /// <returns></returns>
        internal MapPiece GetRandomAlongPath(out MAPDIRECTION dir, bool leftside = true, bool rightside = true)
        {
            dir = MAPDIRECTION.ANY;
            if(Count < 1) { dir = MAPDIRECTION.ANY; return null; }


            if (leftside && rightside)
            {
                int pickIndex = rng.Next(Count * 2);
                if (pickIndex < Count)
                {
                    dir = Dungeon.TwistLeft(steps[pickIndex].Orientation);
                    return steps[pickIndex].Neighbour(dir);
                }
                dir = Dungeon.TwistRight(steps[pickIndex - Count].Orientation);
                return steps[pickIndex - Count].Neighbour(dir);
            }

            if (leftside && !rightside)
            {
                int pickIndex = rng.Next(Count);

                dir = Dungeon.TwistLeft(steps[pickIndex].Orientation);
                return steps[pickIndex].Neighbour(dir);
            }

            if (!leftside && rightside)
            {
                int pickIndex = rng.Next(Count);
                dir = Dungeon.TwistRight(steps[pickIndex].Orientation);
                return steps[pickIndex].Neighbour(dir);
            }
            
            return null;
        }

        internal void TrimToLength(int length)
        {
            while(Count > length)
            {
                steps.Last().State = MAPPIECESTATE.UNUSED;
                steps.RemoveAt(steps.Count - 1);
            }
        }

     
    }// EOF CLASS
}
