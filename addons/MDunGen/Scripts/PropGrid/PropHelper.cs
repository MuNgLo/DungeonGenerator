using Godot;
using Munglo.DungeonGenerator.Sections;

namespace Munglo.DungeonGenerator.PropGrid
{
    /// <summary>
    /// Contains static and functionally coded helper methods to manage prop insertion easier
    /// </summary>
    internal static class PropHelper
    {
        internal static void InsertLayer(RoomSection section, MapPiece rp, PIECEKEYS propKey, int layer, MAPDIRECTION dir, int distance = 1)
        {
            for (int x = 0; x < 6; x++)
            {
                if (x == 0 && rp.HasWestWall) { continue; }
                if (x >= 5 && rp.HasEastWall) { continue; }
                for (int z = 0; z < 6; z++)
                {
                    if (z == 0 && rp.HasNorthWall) { continue; }
                    if (z >= 5 && rp.HasSouthWall) { continue; }
                    if (x % distance == 0 && z % distance == 0)
                    {
                        section.AddProp(new SectionProp() { key = propKey, dir = dir, position = new Vector3I(x, layer, z) });
                    }
                }
            }
        }


        internal static void InsertGallery(RoomSection section, MapPiece rp, int layer, MAPDIRECTION dir, int width = 1)
        {
            // Gallery needs at least one wall so check that and error out
            if (!rp.HasNorthWall && !rp.HasEastWall && !rp.HasSouthWall && !rp.HasWestWall)
            {
                GD.PrintErr($"PropHelper::InsertGallery() Tried to add a gallery to {rp} but it has no walls!");
                return;
            }
            for (int x = 0; x < 6; x++)
            {
                if (x == 0 && rp.HasWestWall) { continue; }
                if (x >= 5 && rp.HasEastWall) { continue; }
                for (int z = 0; z < 6; z++)
                {
                    if (z == 0 && rp.HasNorthWall) { continue; }

                    if (rp.HasNorthWall)
                    {
                        if(z < width + 1)
                        {
                            //section.AddProp(new SectionProp() { key = PIECEKEYS.STAIRPLATFORM, dir = dir, position = new Vector3I(x, layer, z) });
                            continue;
                        }
                    }

                    if (rp.HasEastWall)
                    {
                        if (x > 5 - width - 1)
                        {
                            //section.AddProp(new SectionProp() { key = PIECEKEYS.STAIRPLATFORM, dir = dir, position = new Vector3I(x, layer, z) });
                            continue;
                        }
                    }

                    if (z >= 5 && rp.HasSouthWall) { continue; }
                    if (rp.HasSouthWall)
                    {
                        if (z >  5 - width - 1)
                        {
                            //section.AddProp(new SectionProp() { key = PIECEKEYS.STAIRPLATFORM, dir = dir, position = new Vector3I(x, layer, z) });
                            continue;
                        }
                    }

                    if (rp.HasWestWall)
                    {
                        if (x < width + 1)
                        {
                            //section.AddProp(new SectionProp() { key = PIECEKEYS.STAIRPLATFORM, dir = dir, position = new Vector3I(x, layer, z) });
                            continue;
                        }
                    }

                }
            }
        }
    }// EOF CLASS
}
