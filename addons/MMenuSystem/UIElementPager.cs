using Godot;

namespace Munglo.MenuSystem;

public partial class UIElementPager :Control{
    [Export] private Control[] pages;

    public void GoToPage(int idx){
        if(idx >= 0 && idx < pages.Length){
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].Visible = i == idx ? true : false;
            }            
        }
    }
}// EOF CLASS