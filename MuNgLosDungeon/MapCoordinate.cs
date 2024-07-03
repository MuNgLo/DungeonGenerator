using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Munglo.DungeonGenerator
{
    public struct MapCoordinate
    {
        public int x;
        public int y;
        public int z;

        public MapCoordinate(int X, int Y, int Z)
        {
            x = X;
            y = Y;
            z = Z;
        }

        public bool Equals(MapCoordinate other)
        {
            return x == other.x && y == other.y && z == other.z;
        }
        public static bool operator ==(MapCoordinate a, MapCoordinate b)
        {
            return a.x == b.x && a.y == b.y && a.z == b.z;
        }

        public static bool operator !=(MapCoordinate a, MapCoordinate b)
        {
            return !(a == b);
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            return base.Equals(obj);
        }
        public override string ToString()
        {
            return $"[{x}.{y}.{z}]";
        }
        public override int GetHashCode()
        {
            return x * 1000 + y * 100000 + z * 1000000000;
        }
        public static MapCoordinate operator +(MapCoordinate a, MapCoordinate b) => new MapCoordinate(a.x + b.x, a.y + b.y, a.z + b.z);
        public static MapCoordinate operator +(MapCoordinate a, MAPDIRECTION b)
        {
            switch (b)
            {
                case MAPDIRECTION.NORTH:
                    return a + new MapCoordinate(0, 0, -1);
                case MAPDIRECTION.EAST:
                    return a + new MapCoordinate(1, 0, 0);
                case MAPDIRECTION.SOUTH:
                    return a + new MapCoordinate(0, 0, 1);
                case MAPDIRECTION.WEST:
                    return a + new MapCoordinate(-1, 0, 0);
                case MAPDIRECTION.UP:
                    return a + new MapCoordinate(0, 1, 0);
                case MAPDIRECTION.DOWN:
                    return a + new MapCoordinate(0, -1, 0);
            }
            return a;
        }

        public static MapCoordinate operator -(MapCoordinate a, MapCoordinate b) => new MapCoordinate(a.x - b.x, a.y - b.y, a.z - b.z);

        public static MapCoordinate operator *(MapCoordinate a, int v) => new MapCoordinate(a.x * v, a.y * v, a.z * v);
        #region Fixed and completed Commented up and done
        /// <summary>
        /// Returns the coordinates for the location above.
        /// </summary>
        public MapCoordinate StepUp => this + Up;
        /// <summary>
        /// Returns the coordinates for the location below.
        /// </summary>
        public MapCoordinate StepDown => this + Down;
        /// <summary>
        /// Returns the coordinates for the location to the north.
        /// </summary>
        public MapCoordinate StepNorth => this + North;
        /// <summary>
        /// Returns the coordinates for the location to the East.
        /// </summary>
        public MapCoordinate StepEast => this + East;
        /// <summary>
        /// Returns the coordinates for the location to the South.
        /// </summary>
        public MapCoordinate StepSouth => this + South;
        /// <summary>
        /// Returns the coordinates for the location to the West.
        /// </summary>
        public MapCoordinate StepWest => this + West;

        /// <summary>
        /// Zero coordinate.
        /// 0,0,0
        /// </summary>
        public static MapCoordinate Zero => new MapCoordinate(0, 0, 0);
        /// <summary>
        /// The shift value needed to shift one location up.
        /// 0,1,0
        /// </summary>
        public static MapCoordinate Up => new MapCoordinate(0, 1, 0);
        /// <summary>
        /// The shift value needed to shift one location Down.
        /// 0,-1,0
        /// </summary>
        public static MapCoordinate Down => new MapCoordinate(0, -1, 0);
        /// <summary>
        /// The shift value needed to shift one location North.
        /// 0,0,-1
        /// </summary>
        public static MapCoordinate North => new MapCoordinate(0, 0, -1);
        /// <summary>
        /// The shift value needed to shift one location East.
        /// 1,0,0
        /// </summary>
        public static MapCoordinate East => new MapCoordinate(1, 0, 0);
        /// <summary>
        /// The shift value needed to shift one location South.
        /// 0,0,1
        /// </summary>
        public static MapCoordinate South => new MapCoordinate(0, 0, 1);
        /// <summary>
        /// The shift value needed to shift one location West.
        /// -1,0,0
        /// </summary>
        public static MapCoordinate West => new MapCoordinate(-1, 0, 0);
        #endregion
    }// EOF CLASS

}
