using Godot;
using System;

namespace Munglo.DungeonGenerator
{
    /// <summary>
    /// Static Class that wraps static helper methods
    /// </summary>
    static internal class Dungeon
    {
        static public MapCoordinate[] NeighbourCoordinates(MapCoordinate coord)
        {
            return new MapCoordinate[] {
                coord + MAPDIRECTION.NORTH,
                coord + MAPDIRECTION.EAST,
                coord + MAPDIRECTION.SOUTH,
                coord + MAPDIRECTION.WEST,
                coord + MAPDIRECTION.NORTH + MAPDIRECTION.EAST,
                coord + MAPDIRECTION.SOUTH + MAPDIRECTION.EAST,
                coord + MAPDIRECTION.SOUTH + MAPDIRECTION.WEST,
                coord + MAPDIRECTION.NORTH + MAPDIRECTION.WEST
            };
        }

        static public MAPDIRECTION Flip(MAPDIRECTION direction)
        {
            return TwistLeft(TwistLeft(direction));
        }
        static public MAPDIRECTION TwistLeft(MAPDIRECTION direction)
        {
            switch (direction)
            {
                case MAPDIRECTION.NORTH:
                    return MAPDIRECTION.WEST;
                case MAPDIRECTION.WEST:
                    return MAPDIRECTION.SOUTH;
                case MAPDIRECTION.SOUTH:
                    return MAPDIRECTION.EAST;
                case MAPDIRECTION.EAST:
                    return MAPDIRECTION.NORTH;
            }
            return direction;
        }
        static public MAPDIRECTION TwistRight(MAPDIRECTION direction)
        {
            switch (direction)
            {
                case MAPDIRECTION.NORTH:
                    return MAPDIRECTION.EAST;
                case MAPDIRECTION.EAST:
                    return MAPDIRECTION.SOUTH;
                case MAPDIRECTION.SOUTH:
                    return MAPDIRECTION.WEST;
                case MAPDIRECTION.WEST:
                    return MAPDIRECTION.NORTH;
            }
            return direction;
        }
        internal static Vector3 GlobalPosition(MapPiece piece)
        {
            return GlobalPosition(piece.Coord);
        }
        internal static Vector3 GlobalPosition(MapCoordinate Coord)
        {
            return new Vector3(Coord.x * 6, Coord.y * 6, Coord.z * 6);
        }

        internal static Vector3 GlobalSnapPosition(Vector3 pos)
        {
            return GlobalPosition(GlobalSnapCoordinate((Vector3I)pos));
        }
        internal static MapCoordinate GlobalSnapCoordinate(Vector3I pos)
        {
            pos += new Vector3I(pos.X < 0 ? -3 : 3, 0,pos.Z < 0 ? -3 : 3);
            Vector3I c = pos == Vector3I.Zero ? Vector3I.Zero : (pos / 6);
            return new MapCoordinate(c.X, c.Y, c.Z);
        }
        internal static Vector3 GlobalRoomPropPosition(MapCoordinate Coord, Vector3I Location)
        {
            return new Vector3(Coord.x * 6, Coord.y * 6, Coord.z * 6) + Location + new Vector3(-3f,-1.0f,-3f);
        }

        internal static Vector3 ResolveRotation(MAPDIRECTION orientation)
        {
            Vector3 rot = Vector3.Zero;
            switch (orientation)
            {
                case MAPDIRECTION.NORTH:
                    rot.Y = 0;
                    break;
                case MAPDIRECTION.EAST:
                    rot.Y = -90;
                    break;
                case MAPDIRECTION.SOUTH:
                    rot.Y = -180;
                    break;
                case MAPDIRECTION.WEST:
                    rot.Y = -270;
                    break;
            }
            return rot;
        }
    }// EOF CLASS
}
