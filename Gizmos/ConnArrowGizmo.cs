using Godot;

namespace Munglo.Commons.Gizmos;
/// <summary>
/// Uses an array of vector3 to draw line segments.
/// Has predefined shapes, offset,scale and colour that can be changed
/// Set shape custom and assign your own vector3 array for your own shapes.
/// </summary>
[GlobalClass, Tool]
public partial class ConnArrowGizmo : MeshInstance3D
{
	#region PRIVATE FIELDS
	private GSHAPES shape = GSHAPES.SQUARE;
	private float pathScale = 1.0f;
	private float energy = 1.0f;
	private Color color = Colors.Blue;
	private bool isLocal = true;
	private Vector3 offset = Vector3.Zero;
	private Vector3[] verts = GizmoShapes.Diamond;
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
		[Export]
	public float Energy { get => energy; set { energy = value; UpdateGizmo(); } }
	// draw path in the gizmo's local space or global
	[Export] public bool IsLocal { get => isLocal; set { isLocal = value; UpdateGizmo(); } }
	[Export]
	public Vector3 Offset { get => offset; set { offset = value; UpdateGizmo(); } }
	[ExportCategory("Points for path editable when set to custom")]
	[Export]
	public Vector3[] Pathpoints { get => verts; set { verts = value; UpdateGizmo(); } }
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
		mat.AlbedoColor = color;
		mat.EmissionEnergyMultiplier = energy;
		verts = GizmoShapes.GetShape(shape);
		// Make new iMesh so even when copying nodes in scene it will have a unique mesh
		Mesh = new ImmediateMesh();
		// Clean up old mesh
		iMesh.ClearSurfaces();
		// Skip if no path
		// Begin draw
		iMesh.SurfaceBegin(Mesh.PrimitiveType.Triangles);
		// Build verts
		Vector3 frameOffset = offset - (isLocal ? Vector3.Zero : GlobalPosition);
		for (int i = 0; i < verts.Length; i++)
		{
			// Prepare attributes for add_vertex.
			iMesh.SurfaceSetNormal(Vector3.Up);
			iMesh.SurfaceSetUV(Vector2.Down);
			// Call last for each vertex, adds the above attributes.
			iMesh.SurfaceAddVertex(verts[i] * pathScale + frameOffset);
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

}// EOF CLASS
