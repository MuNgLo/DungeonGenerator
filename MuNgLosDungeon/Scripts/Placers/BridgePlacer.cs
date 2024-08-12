using Godot;
using Munglo.DungeonGenerator.PropGrid;
using System;
using System.Collections.Generic;

namespace Munglo.DungeonGenerator
{
    public class BridgePlacer : IPlacer
    {
        /// <summary>
        /// The parent map data this section belongs to
        /// </summary>
        private protected readonly MapData map;
        private AddonSettingsResource MasterConfig;

        private string resourceName = "BridgePlacer";
        public string ResourceName { get => resourceName; set => resourceName = value; }
        public bool isActive { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Chance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Min { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Max { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        private protected readonly ISection room;


        public BridgePlacer(ISection section, MapData mapData)
        {
            this.room = section;
            this.map = mapData;
            MasterConfig = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_addonconfig.tres") as AddonSettingsResource;
        }

        public void Place(ISection section)
        {
            foreach (int X in map.Pieces.Keys)
            {
                foreach (int Y in map.Pieces[X].Keys)
                {
                    foreach (int Z in map.Pieces[X][Y].Keys)
                    {
                        if (map.Pieces[X][Y][Z].Coord.y < MasterConfig.visibleFloorStart || map.Pieces[X][Y][Z].Coord.y > MasterConfig.visibleFloorEnd) { continue; }

                        FitBridge(map.Pieces[X][Y][Z]);
                    }
                }
            }
        }


        private void FitBridge(MapPiece piece)
        {
            if (piece.State != MAPPIECESTATE.PENDING ) { return; }
            if (!piece.isBridge) { return; }
            if (piece.SectionIndex < 0) { return; }

            MapPiece nbFor = map.GetExistingPiece(piece.Coord + piece.Orientation);
            MapPiece nbBack = map.GetExistingPiece(piece.Coord + Dungeon.Flip(piece.Orientation));
            MapPiece nbLeft = map.GetExistingPiece(piece.Coord + Dungeon.TwistLeft(piece.Orientation));
            MapPiece nbRight = map.GetExistingPiece(piece.Coord + Dungeon.TwistRight(piece.Orientation));

            // Center
            piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = piece.Orientation, variantID = 0 });

            CheckForOffsetPiece(piece, nbFor);

            CheckForFoundation(piece, nbFor, piece.Orientation);
            CheckForFoundation(piece, nbRight, Dungeon.TwistRight(piece.Orientation));
            CheckForFoundation(piece, nbBack, Dungeon.Flip(piece.Orientation));
            CheckForFoundation(piece, nbLeft, Dungeon.TwistLeft(piece.Orientation));

            CheckForConnection(piece, nbRight, Dungeon.TwistRight(piece.Orientation));
            CheckForConnection(piece, nbLeft, Dungeon.TwistLeft(piece.Orientation));

            CheckForRail(piece, nbFor, piece.Orientation);
            CheckForRail(piece, nbRight, Dungeon.TwistRight(piece.Orientation));
            CheckForRail(piece, nbBack, Dungeon.Flip(piece.Orientation));
            CheckForRail(piece, nbLeft, Dungeon.TwistLeft(piece.Orientation));

            CheckForOffsetRail(piece, nbFor, piece.Orientation);
            CheckForOffsetRail(piece, nbRight, Dungeon.TwistRight(piece.Orientation));
            CheckForOffsetRail(piece, nbBack, Dungeon.Flip(piece.Orientation));
            CheckForOffsetRail(piece, nbLeft, Dungeon.TwistLeft(piece.Orientation));

            AddPosts(piece);

            piece.State = MAPPIECESTATE.LOCKED;
            map.SavePiece(piece);
            return;
        }

        private void AddPosts(MapPiece piece)
        {
            MAPDIRECTION dir = piece.Orientation;

            if (map.GetExistingPiece((piece.Coord + Dungeon.TwistLeft(dir))) is not null && map.GetExistingPiece((piece.Coord + Dungeon.TwistLeft(dir))).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.TwistLeft(dir), variantID = 5 });
            }
            if (map.GetExistingPiece((piece.Coord + Dungeon.TwistRight(dir))) is not null && map.GetExistingPiece((piece.Coord + Dungeon.TwistRight(dir))).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 5 });
            }
            
            if (map.GetExistingPiece(piece.Coord + Dungeon.Flip(dir)) is not null 
                &&
                map.GetExistingPiece(piece.Coord + Dungeon.Flip(dir)).isBridge
                &&
                map.GetExistingPiece(piece.Coord + Dungeon.TwistLeft(dir)) is not null
                &&
                map.GetExistingPiece((piece.Coord + Dungeon.TwistLeft(dir))).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.Flip(dir), variantID = 5 });
            }

            if (map.GetExistingPiece(piece.Coord + Dungeon.Flip(dir)) is not null 
                && 
                map.GetExistingPiece(piece.Coord + Dungeon.Flip(dir)).isBridge 
                &&
                map.GetExistingPiece(piece.Coord + Dungeon.TwistRight(dir)) is not null
                &&
                map.GetExistingPiece((piece.Coord + Dungeon.TwistRight(dir))).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.TwistRight(dir), variantID = 5 });
            }


            // Foundation adds
            if (piece.WallKey(dir).key == PIECEKEYS.WD && map.GetExistingPiece(piece.Coord + Dungeon.TwistRight(dir)) is not null && !map.GetExistingPiece(piece.Coord + Dungeon.TwistRight(dir)).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 5 });
            }
            if (piece.WallKey(dir).key == PIECEKEYS.WD && map.GetExistingPiece(piece.Coord + Dungeon.TwistLeft(dir)) is not null && !map.GetExistingPiece(piece.Coord + Dungeon.TwistLeft(dir)).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.TwistLeft(dir), variantID = 5 });
            }

            if (piece.WallKey(Dungeon.Flip(dir)).key == PIECEKEYS.WD)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.TwistRight(dir), variantID = 5 });
            }
            if (piece.WallKey(Dungeon.Flip(dir)).key == PIECEKEYS.WD)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.Flip(dir), variantID = 5 });
            }


            if (piece.WallKey(Dungeon.TwistLeft(dir)).key == PIECEKEYS.WD && map.GetExistingPiece(piece.Coord + Dungeon.TwistLeft(dir)) is not null && !map.GetExistingPiece(piece.Coord + Dungeon.TwistLeft(dir)).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.TwistLeft(dir), variantID = 5 });
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.Flip(dir), variantID = 5 });
            }
            if (piece.WallKey(Dungeon.TwistRight(dir)).key == PIECEKEYS.WD && map.GetExistingPiece(piece.Coord + Dungeon.TwistRight(dir)) is not null && !map.GetExistingPiece(piece.Coord + Dungeon.TwistRight(dir)).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 5 });
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.TwistRight(dir), variantID = 5 });
            }
        }

        private void CheckForRail(MapPiece piece, MapPiece nb, MAPDIRECTION dir)
        {
            if (nb == null) { return; }

            if (!nb.isBridge && piece.WallKey(dir).key != PIECEKEYS.WD)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 3 });
            }
        }
        private void CheckForOffsetRail(MapPiece piece, MapPiece nb, MAPDIRECTION dir)
        {
            if (nb == null) { return; }
            if(map.GetExistingPiece(piece.Coord + Dungeon.TwistRight(dir)) is null) { return; }
            if(map.GetExistingPiece(nb.Coord + Dungeon.TwistRight(dir)) is null) { return; }

            MapPiece pieceRight = map.GetExistingPiece(piece.Coord + Dungeon.TwistRight(dir));
            MapPiece nbRight = map.GetExistingPiece(nb.Coord + Dungeon.TwistRight(dir));

            if (nb.isBridge && piece.WallKey(dir).key != PIECEKEYS.WD)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 4 });
            }
        }
        private void CheckForConnection(MapPiece piece, MapPiece nbRight, MAPDIRECTION dir)
        {
            if (nbRight == null) { return; }
            // Foundation bridge to connect to
            if (nbRight.isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 1 });
            }
        }

        private void CheckForFoundation(MapPiece piece, MapPiece nb, MAPDIRECTION dir)
        {
            if (nb == null) { return; }

            // Foundation placements
            if (piece.WallKey(dir).key == PIECEKEYS.WD)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 2 });
            }
        }

        private void CheckForOffsetPiece(MapPiece piece, MapPiece nbFor)
        {
            if (nbFor == null) { return; }

            // Forward
            if (nbFor.isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = piece.Orientation, variantID = 1 });
                
                // Railings on the offset section
                // Right Side
                //piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = piece.Orientation, variantID = 4 });
                // Left Side
                //nbFor.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.Flip(piece.Orientation), variantID = 4 });


            }
           
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

        public virtual void Place(ISection section, Node3D node)
        {
            throw new NotImplementedException();
        }


    }// EOF CLASS
}
