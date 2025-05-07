namespace Munglo.Commons;
public interface IInventoryItem
{
    public int CurrentInventoryIndex { get; }
    public int OwnerAIOID { get; set; }
    public string ItemName { get; }
    public void SetInventoryIndex(int givenIndex);
}