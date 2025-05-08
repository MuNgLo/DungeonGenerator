using Godot;
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
        private MapData map;
        private List<MapPiece> steps;
        private bool isBlocked = false;
        internal int Count => steps.Count;
        internal bool Blocked => isBlocked;
        internal int Floor => steps[0].Coord.y;
        private protected readonly ISection section;
        private int SectionIndex => section.SectionIndex;

        internal Line(MapData map, ISection section, MapPiece startPiece, PRNGMarsenneTwister rng)
        {
            this.map = map;
            this.section = section;
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

        internal void Walk(int maxSteps, bool mainline)
        {
            if (steps.Count < 1) { return; }
            if (Last.isBridge) { WalkBridge(maxSteps, mainline); return; }
            WalkNormal(maxSteps, mainline);
        }
        internal void WalkNormal(int maxSteps, bool mainline)
        {
            MapPiece nextStep = Last.Neighbour(Last.Orientation, true);

            if (nextStep.SectionIndex >= 0)
            {
                if (nextStep.SectionIndex != Last.SectionIndex)
                {
                    // First step into other section
                    if (nextStep.Section.BridgeAllowed && maxSteps > steps.Count)
                    {
                        if (mainline)
                        {
                            ISection nextSection = map.Sections[nextStep.SectionIndex];
                            // WORKZ
                            int c1 = section.AddConnection(Last.Orientation, nextSection, Last.Coord, nextStep.Coord, true);
                            int c2 = nextSection.AddConnection(Dungeon.Flip(Last.Orientation), section, nextStep.Coord, Last.Coord, true);
                            map.Connections[c1].connectedToConnectionID = c2;
                            map.Connections[c2].connectedToConnectionID = c1;

                            // Rightside special connection
                            MapPiece nextStepRightNB = nextStep.Neighbour(Dungeon.TwistRight(Last.Orientation), false);
                            if (nextStepRightNB is not null && nextStepRightNB.SectionIndex == nextSection.SectionIndex && !nextStepRightNB.HasWall(Dungeon.TwistLeft(Last.Orientation)))
                            {
                                int cR1 = section.AddConnection(Dungeon.TwistRight(Last.Orientation), map.Sections[nextStepRightNB.SectionIndex], nextStep.Coord, nextStepRightNB.Coord, true);
                                int cR2 = map.Sections[nextStepRightNB.SectionIndex].AddConnection(Dungeon.TwistLeft(Last.Orientation), section, nextStepRightNB.Coord, nextStep.Coord, true);
                                map.Connections[cR1].connectedToConnectionID = cR2;
                                map.Connections[cR2].connectedToConnectionID = cR1;
                            }

                            // Leftside special connection
                            MapPiece nextStepLeftNB = nextStep.Neighbour(Dungeon.TwistLeft(Last.Orientation), false);
                            if (nextStepLeftNB is not null && nextStepLeftNB.SectionIndex == nextSection.SectionIndex && !nextStepLeftNB.HasWall(Dungeon.TwistRight(Last.Orientation)))
                            {
                                int cL1 = section.AddConnection(
                                    Dungeon.TwistLeft(Last.Orientation),
                                    map.Sections[nextStepLeftNB.SectionIndex],
                                    nextStep.Coord,
                                    nextStepLeftNB.Coord,
                                    false);
                                int cL2 = map.Sections[nextStepLeftNB.SectionIndex].AddConnection(Dungeon.TwistRight(Last.Orientation), section, nextStepLeftNB.Coord, nextStep.Coord, true);
                                map.Connections[cL1].connectedToConnectionID = cL2;
                                map.Connections[cL2].connectedToConnectionID = cL1;
                            }


                            //nextStep.isBridge = true;
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
                else if (nextStep.SectionIndex == SectionIndex)
                {
                    steps.RemoveAll(p => p.Coord == nextStep.Coord);
                    nextStep.Orientation = Last.Orientation;
                    nextStep.State = MAPPIECESTATE.PENDING;
                    steps.Add(nextStep);
                    return;
                }
                // Proceed to walk Line through other section
                nextStep.Orientation = Last.Orientation;
                nextStep.State = MAPPIECESTATE.PENDING;
                steps.Add(nextStep);
                return;
            }
            else if (Last.SectionIndex >= 0 && Last.SectionIndex != SectionIndex)
            {
                int nextSectionIndex = nextStep.SectionIndex;
                if (nextSectionIndex < 0) { nextSectionIndex = SectionIndex; }
                // WORKZ
                int c1b = section.AddConnection(Dungeon.Flip(Last.Orientation), map.Sections[Last.SectionIndex], nextStep.Coord, Last.Coord, true);
                int c2b = map.Sections[Last.SectionIndex].AddConnection(Last.Orientation, section, Last.Coord, nextStep.Coord, true);
                map.Connections[c1b].connectedToConnectionID = c2b;
                map.Connections[c2b].connectedToConnectionID = c1b;


            }
            nextStep.SectionIndex = SectionIndex;
            nextStep.Orientation = Last.Orientation;
            nextStep.State = MAPPIECESTATE.PENDING;
            steps.Add(nextStep);
            nextStep.Save();
        }
        /// <summary>
        ///  Previous step was a bridge so keep that in mind
        /// </summary>
        /// <param name="maxSteps"></param>
        /// <param name="mainline"></param>
        internal void WalkBridge(int maxSteps, bool mainline)
        {
            MapPiece nextStep = Last.Neighbour(Last.Orientation, true);

            if (nextStep.SectionIndex < 0)
            {
                // Blank
                section.AddConnection(Last.Orientation, map.Sections[nextStep.SectionIndex], Last.Coord, nextStep.Coord, true);
                nextStep.SectionIndex = SectionIndex;
                nextStep.Orientation = Last.Orientation;
                nextStep.State = MAPPIECESTATE.PENDING;
                steps.Add(nextStep);
                nextStep.Save();
                return;
            }
            else if (Last.SectionIndex == nextStep.SectionIndex)
            {
                // same section
                nextStep.isBridge = true;
                nextStep.Orientation = Last.Orientation;
                nextStep.State = MAPPIECESTATE.PENDING;
                steps.Add(nextStep);
                nextStep.Save();
                return;
            }
            else
            {
                // different section
                if (maxSteps < steps.Count || !nextStep.Section.BridgeAllowed)
                {
                    isBlocked = true;
                    return;
                }
                else
                {
                    section.AddConnection(Last.Orientation, map.Sections[nextStep.SectionIndex], Last.Coord, nextStep.Coord, true);
                    nextStep.isBridge = true;
                    nextStep.Orientation = Last.Orientation;
                    nextStep.State = MAPPIECESTATE.PENDING;
                    steps.Add(nextStep);
                    nextStep.Save();
                    return;
                }
            }
        }

        internal MapPiece[] GetTurners(int width, MAPDIRECTION dir, bool reversed = false)
        {
            List<MapPiece> turners = new List<MapPiece>();
            for (int i = steps.Count - 1; i > steps.Count - 1 - width; i--)
            {
                turners.Add(steps[i].Neighbour(dir, true));
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
            if (Count < 1)
            {
                return null;
            }


            if (leftside && rightside)
            {
                int pickIndex = rng.Next(Count * 2);
                if (pickIndex < Count)
                {
                    dir = Dungeon.TwistLeft(steps[pickIndex].Orientation);
                    return steps[pickIndex].Neighbour(dir, true);
                }
                dir = Dungeon.TwistRight(steps[pickIndex - Count].Orientation);
                return steps[pickIndex - Count].Neighbour(dir, true);
            }

            if (leftside && !rightside)
            {
                int pickIndex = rng.Next(Count);

                dir = Dungeon.TwistLeft(steps[pickIndex].Orientation);
                return steps[pickIndex].Neighbour(dir, true);
            }

            if (!leftside && rightside)
            {
                int pickIndex = rng.Next(Count);
                dir = Dungeon.TwistRight(steps[pickIndex].Orientation);
                return steps[pickIndex].Neighbour(dir, true);
            }

            return null;
        }

        internal void TrimToLength(int length)
        {
            while (Count > length)
            {
                steps.Last().State = MAPPIECESTATE.UNUSED;
                steps.RemoveAt(steps.Count - 1);
            }
        }

        internal void Remove(MapCoordinate coord)
        {
            steps.RemoveAll(p => p.Coord == coord);
        }
        internal void FilterBySectionID(int id)
        {
            steps.RemoveAll(p => p.SectionIndex == id);
        }
        internal void InsertAsFirst(MapPiece mapPiece)
        {
            List<MapPiece> newList = new() { mapPiece };
            newList.AddRange(steps);
            steps = newList;
        }
    }// EOF CLASS
}
