using Godot;
/// <summary>
/// Uses an array of vector3 to draw line segments.
/// Has predefined shapes, offset,scale and colour that can be changed
/// Set shape custom and assign your own vector3 array for your own shapes.
/// </summary>
[GlobalClass, Tool]
public partial class PathGizmo : MeshInstance3D
{
	#region PRIVATE FIELDS
	private GSHAPES shape = GSHAPES.SQUARE;
	private float pathScale = 1.0f;
	private Color color = Colors.Blue;
	private bool isLocal = true;
	private Vector3 offset = Vector3.Zero;
	private Vector3[] pathpoints = diamond;
	private ImmediateMesh iMesh => this.Mesh as ImmediateMesh;
	private StandardMaterial3D mat = new StandardMaterial3D();
	#endregion

	#region PUBLIC and EXPORTED PROPERTIES [all sets also fires UpdateGizmo()]
	[Export]
	public GSHAPES Shape { get => shape; set { shape = value; UpdateGizmo(); } }
	[Export(PropertyHint.ColorNoAlpha)]
	public Color GizmoColor
	{
		get => color;
		set
		{
			color = value;
			if (mat.Emission != color)
			{
				mat.Emission = color;
			}
		}
	}
	[Export]
	public float PathScale { get => pathScale; set { pathScale = value; UpdateGizmo(); } }
	// draw path in the gizmo's local space or global
	[Export] public bool IsLocal { get => isLocal; set { isLocal = value; UpdateGizmo(); } }
	[Export]
	public Vector3 Offset { get => offset; set { offset = value; UpdateGizmo(); } }
	[ExportCategory("Points for path editable when set to custom")]
	[Export]
	public Vector3[] Pathpoints { get => pathpoints; set { pathpoints = value; UpdateGizmo(); } }
	#endregion
	// Called when the node enters the scene tree for the first time.
	// Note that this means that scene might need to be reloaded for it to work right in editor
	public override void _Ready()
	{
		VisibilityChanged += WhenVisibiltyChanged;
		UpdateGizmo();
	}
	// Runs a full update of the gizmo
	public void UpdateGizmo()
	{
		if (mat is null) { mat = MaterialOverride as StandardMaterial3D; }
		mat.DisableReceiveShadows = true;
		mat.EmissionEnabled = true;
		mat.Emission = color;
		mat.EmissionEnergyMultiplier = 5.0f;
		pathpoints = ResolveShape();
		int segCount = Mathf.FloorToInt(pathpoints.Length - 1);
		// Make new iMesh so even when copying nodes in scene it will have a unique mesh
		Mesh = new ImmediateMesh();
		// Clean up old mesh
		iMesh.ClearSurfaces();
		// Skip if no path
		if (segCount < 1) { return; }
		// Begin draw
		iMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
		// Build verts
		Vector3 frameOffset = offset - (isLocal ? Vector3.Zero : GlobalPosition);
		for (int i = 0; i < segCount; i++)
		{
			// Prepare attributes for add_vertex.
			iMesh.SurfaceSetNormal(Vector3.Back);
			iMesh.SurfaceSetUV(Vector2.Down);
			// Call last for each vertex, adds the above attributes.
			iMesh.SurfaceAddVertex(pathpoints[i] * pathScale + frameOffset);
			iMesh.SurfaceAddVertex(pathpoints[i + 1] * pathScale + frameOffset);
		}
		// End drawing
		iMesh.SurfaceEnd();
		mat.Emission = color;
		MaterialOverride = mat;
	}
	// When gizmo is hidden it is completely turned off
	private void WhenVisibiltyChanged()
	{
		if (Visible) { ProcessMode = ProcessModeEnum.Inherit; return; }
		ProcessMode = ProcessModeEnum.Disabled;
	}
	/// <summary>
	/// Uses the shape field to get the vec3 array that should be used
	/// </summary>
	/// <returns></returns>
	private Vector3[] ResolveShape()
	{
		switch (shape)
		{
			case GSHAPES.STOP:
				return stop;
			case GSHAPES.SQUARE:
				return square;
			case GSHAPES.DIAMOND:
				return diamond;
			case GSHAPES.ARROW:
				return arrow;
			case GSHAPES.CUBE:
				return cube;
		}
		return pathpoints;
	}
	#region PREDEFINED SHAPES
	// SHAPES
	public enum GSHAPES { CUSTOM, SQUARE, STOP, DIAMOND, ARROW, CUBE }
	private static Vector3[] arrow = new Vector3[]{
		new Vector3(0.0f,1.0f,0.0f), new Vector3(0.0f,0.0f,0.0f), new Vector3(0.04f,0.04f,0.0f),
		new Vector3(-0.04f,0.04f,0.0f), new Vector3(0.0f,0.0f,0.0f)
	};
	private static Vector3[] square = new Vector3[]{
		new Vector3(-0.5f,0.0f,0.0f), new Vector3(0.0f,0.0f,0.5f), new Vector3(0.5f,0.0f,0.0f),
		new Vector3(0.0f,0.0f,-0.5f), new Vector3(-0.5f,0.0f,0.0f)
	};
	private static Vector3[] diamond = new Vector3[]{
		new Vector3(-0.5f,0.0f,0.0f), new Vector3(0.0f,0.0f,0.5f), new Vector3(0.5f,0.0f,0.0f),
		new Vector3(0.0f,0.0f,-0.5f), new Vector3(-0.5f,0.0f,0.0f),new Vector3(0.0f,-0.5f,0.0f),
		new Vector3(0.5f,0.0f,0.0f), new Vector3(0.0f,0.5f,0.0f), new Vector3(0.0f,0.0f,-0.5f),
		new Vector3(0.0f,-0.5f,0.0f),new Vector3(0.0f,0.0f,0.5f),new Vector3(0.0f,0.5f,0.0f),
		new Vector3(-0.5f,0.0f,0.0f)
	};
	private static Vector3[] stop = new Vector3[]{
		new Vector3(-0.5f,0.0f,-1.0f), new Vector3(0.5f,0.0f,-1.0f), new Vector3(1.0f,0.0f,-0.5f),
		new Vector3(1.0f,0.0f,0.5f), new Vector3(0.5f,0.0f,1.0f), new Vector3(-0.5f,0.0f,1.0f),
		new Vector3(-1.0f,0.0f,0.5f), new Vector3(-1.0f,0.0f,-0.5f), new Vector3(-0.5f,0.0f,-1.0f),
	};
	private static Vector3[] cube = new Vector3[]{
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
	#endregion
}// EOF CLASS