using Godot;
namespace Munglo.MenuSystem;
[GlobalClass]
public partial class UIElementPageButton : Button
{
    [Export] private UIElementPager pager;
    [Export] private int pageIndex;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Pressed += WhenPressed;
    }

    private void WhenPressed()
    {
        pager.GoToPage(pageIndex);
        ReleaseFocus();
    }
}