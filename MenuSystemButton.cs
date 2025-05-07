using Godot;
namespace Munglo.MenuSystem;
[GlobalClass]
public partial class MenuSystemButton : Button
{
    [Export] private string targetMenu;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Pressed += WhenPressed;
    }

    private void WhenPressed()
    {
        if(targetMenu.ToLower() == "back"){
            MenuSystem.GoToPreviousMenu();
            return;
        }
          if(targetMenu.ToLower() == "quit"){
            GetTree().Quit();
            return;
        }
        MenuSystem.GoToMenu(targetMenu);
        ReleaseFocus();
    }
}
