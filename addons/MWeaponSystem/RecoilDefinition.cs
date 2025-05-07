using Godot;

namespace Munglo.WeaponsSystem;
public class RecoilDefinition
{
    public float X = 1.0f;
    public float Y = 1.0f;
    public float Stability = 3.0f;

    public Node3D viewPivvot;

    public void AddRecoil()
    {
        //viewPivvot.localRotation = Quaternion.Euler(Vector3.up * UnityEngine.Random.Range(-Y, Y)) *
        //    Quaternion.Euler(Vector3.right * UnityEngine.Random.Range(0, -X)) * viewPivvot.localRotation;
    }
    public void Stabilize()
    {
        //if (viewPivvot.localRotation != Quaternion.Euler(0.0f, 0.0f, 0.0f))
        //{
        //    viewPivvot.localRotation = Quaternion.RotateTowards(viewPivvot.localRotation, Quaternion.Euler(0.0f, 0.0f, 0.0f), Stability * Time.deltaTime);
        //}
    }
}// EOF CLASS