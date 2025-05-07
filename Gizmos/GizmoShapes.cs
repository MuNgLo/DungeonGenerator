using Godot;

namespace Munglo.Commons.Gizmos;
/// <summary>
/// Static class that defines base shapes to use for the gizmos
/// </summary>
public enum GSHAPES { CUSTOM, SQUARE, STOP, DIAMOND, ARROW, CUBE, TRIANGLE, BRIDGE }
public static class GizmoShapes
{

    /// <summary>
    /// Uses the shape field to get the vec3 array that should be used
    /// </summary>
    /// <returns></returns>
    public static Vector3[] GetShape(GSHAPES shape)
    {
        switch (shape)
        {
            case GSHAPES.STOP:
                return Stop;
            case GSHAPES.SQUARE:
                return Square;
            case GSHAPES.DIAMOND:
                return Diamond;
            case GSHAPES.ARROW:
                return Arrow;
            case GSHAPES.CUBE:
                return Cube;
            case GSHAPES.TRIANGLE:
                return TriArrow;
            case GSHAPES.BRIDGE:
                return Bridge;
        }
        return new Vector3[0];
    }
    public static Vector3[] TriArrow = new Vector3[]{
        new Vector3(0.0f,0.0f,0.0f),
        new Vector3(-0.25f,0.0f,-0.25f),
        new Vector3(0.25f,0.0f,-0.25f),

        new Vector3(0.12f,0.0f,-0.25f),
        new Vector3(-0.12f,0.0f,-0.25f),
        new Vector3(0.12f,0.0f,-0.4f),

        new Vector3(-0.12f,0.0f,-0.25f),
        new Vector3(-0.12f,0.0f,-0.4f),
        new Vector3(0.12f,0.0f,-0.4f)
    };
    public static Vector3[] Arrow = new Vector3[]{
        new Vector3(0.0f,1.0f,0.0f), new Vector3(0.0f,0.0f,0.0f), new Vector3(0.04f,0.04f,0.0f),
        new Vector3(-0.04f,0.04f,0.0f), new Vector3(0.0f,0.0f,0.0f)
    };
    public static Vector3[] Square = new Vector3[]{
        new Vector3(-0.5f,0.0f,0.0f), new Vector3(0.0f,0.0f,0.5f), new Vector3(0.5f,0.0f,0.0f),
        new Vector3(0.0f,0.0f,-0.5f), new Vector3(-0.5f,0.0f,0.0f)
    };
    public static Vector3[] Diamond = new Vector3[]{
        new Vector3(-0.5f,0.0f,0.0f), new Vector3(0.0f,0.0f,0.5f), new Vector3(0.5f,0.0f,0.0f),
        new Vector3(0.0f,0.0f,-0.5f), new Vector3(-0.5f,0.0f,0.0f),new Vector3(0.0f,-0.5f,0.0f),
        new Vector3(0.5f,0.0f,0.0f), new Vector3(0.0f,0.5f,0.0f), new Vector3(0.0f,0.0f,-0.5f),
        new Vector3(0.0f,-0.5f,0.0f),new Vector3(0.0f,0.0f,0.5f),new Vector3(0.0f,0.5f,0.0f),
        new Vector3(-0.5f,0.0f,0.0f)
    };
    public static Vector3[] Stop = new Vector3[]{
        new Vector3(-0.5f,0.0f,-1.0f), new Vector3(0.5f,0.0f,-1.0f), new Vector3(1.0f,0.0f,-0.5f),
        new Vector3(1.0f,0.0f,0.5f), new Vector3(0.5f,0.0f,1.0f), new Vector3(-0.5f,0.0f,1.0f),
        new Vector3(-1.0f,0.0f,0.5f), new Vector3(-1.0f,0.0f,-0.5f), new Vector3(-0.5f,0.0f,-1.0f),
    };
    public static Vector3[] Cube = new Vector3[]{
		// Bottom
		new Vector3(-1.0f,-1.0f,1.0f),
        new Vector3(1.0f,-1.0f,1.0f),
        new Vector3(1.0f,-1.0f,-1.0f),
        new Vector3(-1.0f,-1.0f,-1.0f),
        new Vector3(-1.0f,-1.0f,1.0f),
		// Top
		new Vector3(-1.0f,1.0f,1.0f),
        new Vector3(1.0f,1.0f,1.0f), // 6th
		new Vector3(1.0f,1.0f,-1.0f),
        new Vector3(-1.0f,1.0f,-1.0f),
        new Vector3(-1.0f,1.0f,1.0f),
        new Vector3(-1.0f,-1.0f,-1.0f), // 10th
		new Vector3(-1.0f,1.0f,-1.0f),
        new Vector3(1.0f,-1.0f,-1.0f),
        new Vector3(1.0f,1.0f,-1.0f),
        new Vector3(1.0f,-1.0f,1.0f),
        new Vector3(1.0f,1.0f,1.0f), // 15th
		new Vector3(-1.0f,-1.0f,1.0f)
    };

    public static Vector3[] Bridge = new Vector3[]{

            // First side
            new Vector3(-1.0f,0.0f,2.0f),
            new Vector3(-1.0f,0.0f,1.0f),
            new Vector3(-1.0f,0.0f,0.0f),
            new Vector3(-1.0f,0.0f,-1.0f),
            new Vector3(-1.0f,0.0f,-2.0f),
            new Vector3(-1.0f,-0.304f,-2.0f),
            new Vector3(-1.0f,-0.175f,-1.0f),
            new Vector3(-1.0f,-0.159f,0.0f),
            new Vector3(-1.0f,-0.175f,1.0f),
            new Vector3(-1.0f,-0.304f,2.0f),
            new Vector3(-1.0f,0.0f,2.0f),
            new Vector3(0.0f,float.PositiveInfinity,0.0f),
            new Vector3(-1.0f,-0.175f, 1.0f),
            new Vector3(-1.0f,0.0f, 1.0f),
            new Vector3(0.0f,float.PositiveInfinity,0.0f),
            new Vector3(-1.0f,-0.175f, -1.0f),
            new Vector3(-1.0f,0.0f, -1.0f),
            new Vector3(0.0f,float.PositiveInfinity,0.0f),
            new Vector3(-1.0f,-0.159f, 0.0f),
            new Vector3(-1.0f,0.0f, 0.0f),
            // Not First side
            new Vector3(0.0f,float.PositiveInfinity,0.0f),
            new Vector3(1.0f,0.0f,2.0f),
            new Vector3(1.0f,0.0f,1.0f),
            new Vector3(1.0f,0.0f,0.0f),
            new Vector3(1.0f,0.0f,-1.0f),
            new Vector3(1.0f,0.0f,-2.0f),
            new Vector3(1.0f,-0.304f,-2.0f),
            new Vector3(1.0f,-0.175f,-1.0f),
            new Vector3(1.0f,-0.159f,0.0f),
            new Vector3(1.0f,-0.175f,1.0f),
            new Vector3(1.0f,-0.304f,2.0f),
            new Vector3(1.0f,0.0f,2.0f),
            new Vector3(0.0f,float.PositiveInfinity,0.0f),
            new Vector3(1.0f,-0.175f, 1.0f),
            new Vector3(1.0f,0.0f, 1.0f),
            new Vector3(0.0f,float.PositiveInfinity,0.0f),
            new Vector3(1.0f,-0.175f, -1.0f),
            new Vector3(1.0f,0.0f, -1.0f),
            new Vector3(0.0f,float.PositiveInfinity,0.0f),
            new Vector3(1.0f,-0.159f, 0.0f),
            new Vector3(1.0f,0.0f, 0.0f),
            // Across
            new Vector3(0.0f,float.PositiveInfinity,0.0f),
            new Vector3(1.0f,0.0f,2.0f),
            new Vector3(-1.0f,0.0f,2.0f),
            new Vector3(1.0f,0.0f,1.0f),
            new Vector3(-1.0f,0.0f,1.0f),
            new Vector3(1.0f,0.0f,0.0f),
            new Vector3(-1.0f,0.0f,0.0f),
            new Vector3(1.0f,0.0f,-1.0f),
            new Vector3(-1.0f,0.0f,-1.0f),
            new Vector3(1.0f,0.0f,-2.0f),
            new Vector3(-1.0f,0.0f,-2.0f)
    };


}// EOF CLASS