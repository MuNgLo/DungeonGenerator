using Godot;
public interface ICarryReference
{
    public void SetOriginalObject(Node ogObject);
    public void UpdateBoundsFromOriginal();
}
