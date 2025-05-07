using Godot;
namespace Munglo.AI.Movement
{
    public static class Navigation
    {

        public static Vector3 RandomCirclePointInRange(Vector3 origin, float range)
        {
            Vector3 vec = Vector3.Zero;
            while (vec == Vector3.Zero)
            {
                vec = new Vector3((float)GD.RandRange(-1.0f, 1.0f), 0.0f, (float)GD.RandRange(-1.0f, 1.0f));
            }
            return origin + vec * range;
        }
    }//EOF CLASS
}