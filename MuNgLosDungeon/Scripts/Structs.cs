using Godot;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
namespace Munglo.DungeonGenerator
{
    public struct KeyData
    {
        public PIECEKEYS key;
        public MAPDIRECTION dir;
        public int variantID;

        public override string ToString()
        {
            return $"key[{key}] dir[{dir}]";
        }
    }

    public struct RoomProp
    {
        public PIECEKEYS key;
        /// <summary>
        /// offset relative to room start location tile and the orientation
        /// </summary>
        public Vector3I Offset;
        public MAPDIRECTION dir;
        public int variantID;
        public RoomProp(PIECEKEYS key, Vector3I offset, MAPDIRECTION dir, int variantid = -1)
        {
            this.key = key ;
            this.Offset = offset;
            this.dir = dir ;
            this.variantID = variantid ;
        }
    }
}
