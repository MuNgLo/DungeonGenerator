using Godot;
namespace Munglo.Commons;

public interface IInteractable
{
    Vector3 GlobalPosition {get;}
    bool ISRigidBody { get; }
    bool CanSleep {get;set;}

    Vector3 ToGlobal(Vector3 v);
    Vector3 ToLocal(Vector3 v);
    Vector2 GetDampening();
    void WakeUp();
    void SetDampening(Vector2 damp);
    void Pull(Vector3 force, Vector3 point);
    void Stop();
    void Dampen(float dampening);
}//