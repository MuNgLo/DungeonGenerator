using Godot;
namespace Munglo.DungeonGenerator;

public struct ConnectedLocation
{
    public int section;
    public int connectionID;
    public MapCoordinate coord;
    public double cost;
    public override string ToString()
    {
        return $"[S{section}]{coord}[cost:{cost}]";
    }
    public ConnectedLocation(int inSection, int connectionID, MapCoordinate location, double cost)
    {
        section = inSection; this.connectionID = connectionID; coord = location; this.cost = cost;
    }
}

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

public struct SectionProp
{
    public PIECEKEYS key;
    /// <summary>
    /// offset relative to room start location tile and the orientation
    /// </summary>
    public Vector3I position;
    public MAPDIRECTION dir;
    public int variantID;
    public SectionProp(KeyData kData, Vector3I pos)
    {
        this.key = kData.key;
        this.dir = kData.dir;
        this.position = pos;
        this.variantID = kData.variantID;
    }
    public SectionProp(PIECEKEYS key, Vector3I position, MAPDIRECTION dir, int variantid = -1)
    {
        this.key = key;
        this.position = position;
        this.dir = dir;
        this.variantID = variantid;
    }
    public SectionProp(PIECEKEYS key, Vector3 position, MAPDIRECTION dir, int variantid = -1)
    {
        this.key = key;
        this.position = (Vector3I)position;
        this.dir = dir;
        this.variantID = variantid;
    }
}

