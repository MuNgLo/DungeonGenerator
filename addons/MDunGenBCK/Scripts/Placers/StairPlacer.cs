using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Munglo.DungeonGenerator
{
    /// <summary>
    /// Instance this to create a staircase drop from given startpiece.
    /// It checks for possabilty in construction but doesnäät add the keys until the Build() is called
    /// </summary>
    internal class StairPlacer : IPlacer
    {
        private protected readonly ISection section;
        private readonly MapPiece parentPiece;
        private readonly bool canFit;

        private MapPiece locationPiece; // where the stair will be placed
        private MAPDIRECTION ogOrientation;
        private MAPDIRECTION orientation;
        private bool wide = false;

        internal bool isValid => canFit;
        int variationID = 0;

        private string resourceName = "BridgePlacer";
        public string ResourceName { get => resourceName; set => resourceName = value; }

        private List<MapPiece> Pieces => section.Pieces.Cast<MapPiece>().ToList();

        public bool isActive { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Chance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Min { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Max { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <summary>
        /// Instance this to create a staircase drop from given startpiece.
        /// Starting with neighbour in its direction. then attemps the other directions.
        /// It checks for possabilty in construction but doesn't add the keys until the Build() is called
        /// </summary>
        internal StairPlacer(ISection section, MapPiece parentPiece, MAPDIRECTION dir)
        {
            this.section = section;
            this.parentPiece = parentPiece;
            ogOrientation = dir;
            orientation = dir;

            wide = parentPiece.WallKey(dir).key == PIECEKEYS.WDW;

            //if (TestFit(ogOrientation)) { canFit = true; return; }
            if (TestFit(Dungeon.TwistRight(ogOrientation))) { canFit = true; variationID = 0; return; }
            if (TestFit(Dungeon.TwistLeft(ogOrientation))) { canFit = true; variationID = 1; return; }
            //if (TestFit(Dungeon.Flip(ogOrientation))) { canFit = true; return; }
            canFit = false;
        }

        private bool TestFit(MAPDIRECTION dir)
        {
            //RoomPiece stairStart = null;

            MapPiece r1 = Pieces.Find(p => p.Coord == parentPiece.Coord + dir);
            MapPiece r2 = Pieces.Find(p => p.Coord == parentPiece.Coord + dir + MapCoordinate.Down);
            if (r1 is null || r2 is null) { return false; }
            if (r1.hasStairs is true || r2.hasStairs is true) { return false; }



            // Fail if there is anytthing other ethen W or None to the left
            if ((r1.WallKey(Dungeon.TwistLeft(dir)).key != PIECEKEYS.W)
                    &&
                ((r1.WallKey(Dungeon.TwistLeft(dir)).key != PIECEKEYS.NONE)))
            { return false; }
            // Fail if there is anytthing other ethen W or None to the right
            if (
                (r1.WallKey(Dungeon.TwistRight(dir)).key != PIECEKEYS.W)
                &&
                ((r1.WallKey(Dungeon.TwistRight(dir)).key != PIECEKEYS.NONE))
                )
            { return false; }
            



            orientation = Dungeon.Flip(dir);
            locationPiece = r1;
            //GD.Print($"StairCase::TestFit() Stair Fit!");
            return true;
        }

        /// <summary>
        /// Inserts the stair keys into the roompieces if the staircase is still valid
        /// </summary>
        internal void Build()
        {
            if (!canFit) { return; }
            //GD.Print( $"StairCase::Build() Stair Built! ogOrientation[{ogOrientation}] orientation[{orientation}]");
            //Vector3I offset = new Vector3I(2, 1, 2);
            Vector3I offset = Vector3I.Zero;
            //Vector3I offset2 = new Vector3I(3, 1, 3);
            Vector3I offset2 = Vector3I.Zero;
            // Since offset is in worldspace we need to tweak it depending on OG Direction
            switch (ogOrientation)
            {
                case MAPDIRECTION.NORTH:
                    offset += new Vector3I(-1, 0, 1);
                    if(variationID == 1) { offset += new Vector3I(2, 0, 0); }
                    if (wide) { offset2 += new Vector3I(3, 0, 0); offset += new Vector3I(3, 0, 0); }
                    break;
                case MAPDIRECTION.EAST:
                    offset += new Vector3I(-1, 0, -1);
                    if (variationID == 1) { offset += new Vector3I(0, 0, 2); }
                    if (wide) { offset2 += new Vector3I(0, 0, 3); offset += new Vector3I(0, 0, 3); }
                    break;
                case MAPDIRECTION.SOUTH:
                    offset += new Vector3I(1, 0, -1);
                    if (variationID == 1) { offset += new Vector3I(-2, 0, 0); }
                    if (wide) { offset2 += new Vector3I(-3, 0, 0); offset += new Vector3I(-3, 0, 0); }
                    break;
                case MAPDIRECTION.WEST:
                    offset += new Vector3I(1, 0, 1);
                    if (variationID == 1) { offset += new Vector3I(0, 0, -2); }
                    if (wide) { offset2 += new Vector3I(0, 0, -3); offset += new Vector3I(0, 0, -3); }
                    break;
            }



            MapPiece r1 = Pieces.Find(p => p.Coord == locationPiece.Coord);
            MapPiece r2 = Pieces.Find(p=>p.Coord == r1.Coord + MapCoordinate.Down);
            
            r1.hasStairs = true;
            r2.hasStairs = true;
            

            section.AddProp(new SectionProp(PIECEKEYS.STAIR, Dungeon.GlobalPosition(r1) +  offset, orientation, variationID));
            if (!r2.hasFloor)
            {
                section.AddProp(new SectionProp(PIECEKEYS.STAIR, Dungeon.GlobalPosition(r2) + offset, Dungeon.Flip(orientation), variationID));
            }


            r1.Save();
            r2.Save();

            parentPiece.hasStairs = false;
            parentPiece.keyFloor = new KeyData() { key = PIECEKEYS.OCCUPIED, dir = ogOrientation, variantID = 0 };



            section.AddProp(new SectionProp(PIECEKEYS.BRIDGE, Dungeon.GlobalPosition(parentPiece) + offset2, ogOrientation, 6));
            parentPiece.Save();
        }


        public void DoForcedRolls(ISection section)
        {
            throw new NotImplementedException();
        }

        public virtual bool Fit(ISection section)
        {
            throw new NotImplementedException();
        }

        public virtual bool Fit(ISection section, Node3D node)
        {
            throw new NotImplementedException();
        }
        public bool PickRandomProp(out PackedScene asset, out int count)
        {
            throw new NotImplementedException();
        }
        public virtual void Place(ISection section)
        {
            throw new NotImplementedException();
        }
        public virtual void Place(ISection section, Node3D node)
        {
            throw new NotImplementedException();
        }

    }// EOF CLASS
}
