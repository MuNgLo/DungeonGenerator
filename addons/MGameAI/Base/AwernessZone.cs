using System.Collections.Generic;
using Godot;

namespace Munglo.AI.Base
{
    [GlobalClass]
    public partial class AwernessZone : Node3D
    {
        [Export] private bool debug = false;
        [Export] private Mesh ogMesh;
        [Export] private MeshInstance3D mFilter;
        [Export] private CollisionShape3D mColl;
     
        private List<int> LeftSide;
        private List<int> RightSide;
        private List<Edge> Cage;
        private List<Vector3> ogPoints;
        
        private AIUnit unit;
        public void Init(AIUnit aiUnit)
        {
            if (ogMesh is null)
            {
                GD.PrintErr("Need an original Mesh assigned!");
                return;
            }
            unit = aiUnit;

            //mFilter = GetComponent<MeshFilter>();
            //mColl = GetComponent<MeshCollider>();
            ogPoints = new List<Vector3>();
            //mFilter.mesh = Instantiate(ogMesh);
            //mColl.sharedMesh = null;
            //mColl.sharedMesh = mFilter.sharedMesh;
            //SaveOgPoints();
            SyncMesh();
        }
        public void Resync()
        {
            SyncMesh();
        }

        /// <summary>
        /// TODO for now this always return TRUE
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool IsPointInsideZone(Vector3 point)
        {
            return true;
        }
        private void SyncMesh()
        {
            /*Mesh mesh = mFilter.sharedMesh;
            if (!unit)
            {
                Init(transform.parent.GetComponent<AIUnit>());
                return;
            }
            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                if (!LeftSide.Exists(p => p == i) && !RightSide.Exists(p => p == i))
                {
                    verts[i].x = ogPoints[i].x * unit.ZoneConfig.width;
                    verts[i].z = ogPoints[i].z * unit.ZoneConfig.Range;
                    verts[i].y = ogPoints[i].y > 0.3f ? verts[i].y = unit.ZoneConfig.Height : verts[i].y = unit.ZoneConfig.heightOffset;
                }
            }
            foreach (int index in LeftSide)
            {
                if (index >= verts.Length || index < 0) { continue; }
                verts[index].x = unit.ZoneConfig.baseWidth * 0.5f * -1.0f;
                verts[index].z = ogPoints[index].z + unit.ZoneConfig.rangeOffset;
                verts[index].y = ogPoints[index].y > 0.3f ? verts[index].y = unit.ZoneConfig.Height : verts[index].y = unit.ZoneConfig.heightOffset;
            }
            foreach (int index in RightSide)
            {
                if (index >= verts.Length || index < 0) { continue; }
                verts[index].x = unit.ZoneConfig.baseWidth * 0.5f;
                verts[index].z = ogPoints[index].z + unit.ZoneConfig.rangeOffset;
                verts[index].y = ogPoints[index].y > 0.3f ? verts[index].y = unit.ZoneConfig.Height : verts[index].y = unit.ZoneConfig.heightOffset;
            }
            mesh.SetVertices(verts);
            mFilter.mesh = mesh;
            mesh.RecalculateNormals();
            mColl.sharedMesh = null;
            mColl.sharedMesh = mesh;
            */
        }
        
        /*private void SaveOgPoints()
        {
            Godot.Collections.Array meshArr = ogMesh.SurfaceGetArrays(0);
            Vector3[] verts = meshArr.
            foreach (Vector3 vector in verts)
            {
                ogPoints.Add(vector);
            }
        }*/
        
    }// EOF CLASS
}