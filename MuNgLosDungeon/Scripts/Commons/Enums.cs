using System;
namespace Munglo.DungeonGenerator
{
    /// <summary>
    /// Unused has noting in it<br></br>
    /// Error something went wrong<br></br>
    /// Faulty don't fit right yet<br></br>
    /// Pending has things but is open to change<br></br>
    /// Locked can't be changed (Goasdal is to have all piece data locked by end of generation)
    /// </summary>
    public enum MAPPIECESTATE { UNUSED, ERROR, FAULTY, PENDING, LOCKED }
    /// <summary>
    /// Does the piece have any water
    /// </summary>
    public enum WATERAMOUNT { NONE, SMALL, MEDIUM, LARGE }
    /// <summary>
    /// If piece is part of a path, what size the path is
    /// </summary>
    public enum PATHSIZE { NONE, SMALL, MEDIUM, LARGE }
    /// <summary>
    /// When applicable this holds the direction that is important to the tile
    /// </summary>
    public enum MAPDIRECTION { ANY, NORTH, EAST, SOUTH, WEST, UP, DOWN }
    [Flags]
    public enum WALLS { N = 1, E = 2, S = 4, W = 8}
    public enum PIECEKEYS { NONE, ERROR, FAULTY, DEBUG, WFGREEN, WFRED, OCCUPIED, F, FWSS, C, CWS, W, WD,
        WDW, WCI, AS, ASIC, PLAYERSPAWN, ENEMYSPAWN, GOAL, DOUBLEDOOR, STAIR, STAIRSPIRAL, STAIRPLATFORM,
        BRIDGE, DEBUGSTAIR, DEBUGBRIDGE, DEBUGPATHEND, PROPMARKER }


    [Flags]
    public enum ROOMCONNECTIONRESPONCE { DOOR = 1, BALCONY = 2, BRIDGE = 4, STAIR = 8 }
}
