using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Munglo.DungeonGenerator
{
    public class BridgePlacer
    {
        private MapData map;
        private AddonSettings MasterConfig;

        public BridgePlacer(MapData mapData)
        {
            this.map = mapData;
            MasterConfig = ResourceLoader.Load("res://addons/MuNgLosDungeon/Config/def_addonconfig.tres") as AddonSettings;
        }

        public void Place()
        {
            foreach (int X in map.Pieces.Keys)
            {
                foreach (int Y in map.Pieces[X].Keys)
                {
                    foreach (int Z in map.Pieces[X][Y].Keys)
                    {
                        if (map.Pieces[X][Y][Z].Coord.y < MasterConfig.visibleFloorStart || map.Pieces[X][Y][Z].Coord.y > MasterConfig.visibleFloorEnd - 1) { continue; }

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

            MapPiece nbFor = piece.Neighbour(piece.Orientation);
            MapPiece nbBack = piece.Neighbour(Dungeon.Flip(piece.Orientation));
            MapPiece nbLeft = piece.Neighbour(Dungeon.TwistLeft(piece.Orientation));
            MapPiece nbRight = piece.Neighbour(Dungeon.TwistRight(piece.Orientation));

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

            if (piece.Neighbour(Dungeon.TwistLeft(dir)).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.TwistLeft(dir), variantID = 5 });
            }
            if (piece.Neighbour(Dungeon.TwistRight(dir)).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 5 });
            }
            if (piece.Neighbour(Dungeon.Flip(dir)).isBridge && piece.Neighbour(Dungeon.TwistLeft(dir)).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.Flip(dir), variantID = 5 });
            }
            if (piece.Neighbour(Dungeon.Flip(dir)).isBridge && piece.Neighbour(Dungeon.TwistRight(dir)).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.TwistRight(dir), variantID = 5 });
            }


            // Foundation adds
            if (piece.WallKey(dir).key == PIECEKEYS.WD && !piece.Neighbour(Dungeon.TwistRight(dir)).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 5 });
            }
            if (piece.WallKey(dir).key == PIECEKEYS.WD && !piece.Neighbour(Dungeon.TwistLeft(dir)).isBridge)
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


            if (piece.WallKey(Dungeon.TwistLeft(dir)).key == PIECEKEYS.WD && !piece.Neighbour(Dungeon.TwistLeft(dir)).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.TwistLeft(dir), variantID = 5 });
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.Flip(dir), variantID = 5 });
            }
            if (piece.WallKey(Dungeon.TwistRight(dir)).key == PIECEKEYS.WD && !piece.Neighbour(Dungeon.TwistRight(dir)).isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 5 });
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = Dungeon.TwistRight(dir), variantID = 5 });
            }
        }

        private void CheckForRail(MapPiece piece, MapPiece nb, MAPDIRECTION dir)
        {
            if (!nb.isBridge && piece.WallKey(dir).key != PIECEKEYS.WD)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 3 });
            }
        }
        private void CheckForOffsetRail(MapPiece piece, MapPiece nb, MAPDIRECTION dir)
        {
            MapPiece pieceRight = piece.Neighbour(Dungeon.TwistRight(dir));
            MapPiece nbRight = nb.Neighbour(Dungeon.TwistRight(dir));

            //if (pieceRight.isBridge && nbRight.isBridge) { return; }

            if (nb.isBridge && piece.WallKey(dir).key != PIECEKEYS.WD)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 4 });
            }
        }
        private void CheckForConnection(MapPiece piece, MapPiece nbRight, MAPDIRECTION dir)
        {
            // Foundation bridge to connect to
            if (nbRight.isBridge)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 1 });
            }
        }

        private void CheckForFoundation(MapPiece piece, MapPiece nb, MAPDIRECTION dir)
        {
            // Foundation placements
            if (piece.WallKey(dir).key == PIECEKEYS.WD)
            {
                piece.AddProp(new KeyData() { key = PIECEKEYS.BRIDGE, dir = dir, variantID = 2 });
            }
        }

        private void CheckForOffsetPiece(MapPiece piece, MapPiece nbFor)
        {
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
    }
}
