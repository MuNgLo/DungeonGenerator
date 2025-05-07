using System;
using System.Reflection;
using Godot;
using Munglo.SettingsModule;

namespace Munglo.MenuSystem;

public partial class UIKeybindPopup : Control
{
    private static UIKeybindPopup Instance;
    [Export] private RichTextLabel info;
    private string settingsName;
    private string fieldTarget;
    private bool asAlt;
    private bool keybindInProgress = false;
    public static bool Active => Instance.Visible;
    /// <summary>
    /// String array carries SettingsName, FieldName
    /// </summary>
    public static EventHandler<string[]> OnKeyBindUpdated;

    public override void _EnterTree()
    {
        Instance = this;
        Hide();
    }

    public override void _Input(InputEvent @event)
    {
        if (!keybindInProgress) { return; }
        if (@event is InputEventKey key)
        {
            if (key.Keycode == Key.Escape) { HidePopup(); }
            object Config = Settings.GetCachedSettings(settingsName);
            FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
            PlayerKeyBind bind = (PlayerKeyBind)Settings.GetFieldValue(settingsName, fieldTarget);
            if (asAlt)
            {
                bind.KeyAlt = key.Keycode;
                bind.MouseButtonAlt = MouseButton.None;
            }
            else
            {
                bind.Key = key.Keycode;
                bind.MouseButton = MouseButton.None;
            }
            Settings.SetFieldValue(settingsName, fieldTarget, bind, "");
            keybindInProgress = false;
            OnKeyBindUpdated?.Invoke(this, [settingsName, fieldTarget]);
            HidePopup();
            return;
        }

        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.None) { HidePopup(); }
            object Config = Settings.GetCachedSettings(settingsName);
            FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
            PlayerKeyBind bind = (PlayerKeyBind)Settings.GetFieldValue(settingsName, fieldTarget);
            if (asAlt)
            {
                bind.KeyAlt = Key.None;
                bind.MouseButtonAlt = mb.ButtonIndex;
            }
            else
            {
                bind.Key = Key.None;
                bind.MouseButton = mb.ButtonIndex;
            }
            Settings.SetFieldValue(settingsName, fieldTarget, bind, "");
            keybindInProgress = false;
            OnKeyBindUpdated?.Invoke(this, [settingsName, fieldTarget]);
            HidePopup();
            return;
        }

        if (@event is InputEventJoypadButton btn)
        {
            GD.Print($"UIKeybindPopup::_Input() was InputEventJoypadButton [{btn.ButtonIndex}]");
        }
        // Controller axis
        /*if (@event is InputEventJoypadMotion mot)
        {
            if (mot.Axis == JoyAxis.LeftX)
            {
                //InputEventJoypadMotion joyMEvent = InputMap.ActionGetEvents("Rabies").Where<InputEvent>(p=>p is InputEventJoypadMotion).First() as InputEventJoypadMotion;
                //outLabel.Text = joyMEvent.AxisValue.ToString("0.00");
                joyLeftX.SetValueNoSignal(mot.AxisValue);
            }
        }*/
    }

    /// <summary>
    /// Checks instance is set, Store the data needed in Instance and starts the KeyBind
    /// </summary>
    /// <param name="sName"></param>
    /// <param name="fName"></param>
    /// <param name="isAlt"></param>
    public static void StartKeyBind(string sName, string fName, bool isAlt = false)
    {
        if (Instance is null)
        {
            Core.LogError("UIKeybindPopup::StartKeyBind() Instance not set! Make sure there is a UI Popup in the scene.");
            return;
        }
        Instance.settingsName = sName;
        Instance.fieldTarget = fName;
        Instance.asAlt = isAlt;
        Instance.StartKeyBind();
    }
    public void StartKeyBind()
    {
        object Config = Settings.GetCachedSettings(settingsName);
        FieldInfo fieldInfo = Config.GetType().GetField(fieldTarget);
        string actionName = fieldInfo.Name;
        if (fieldInfo.GetCustomAttribute<MenuLabel>() is not null)
        {
            actionName = fieldInfo.GetCustomAttribute<MenuLabel>().Text;
        }
        info.Text = $"Please press the {(asAlt ? "[color=yellow]Alt[/color] " : "")}key to bind{System.Environment.NewLine}[color=yellow]{actionName}[/color]";
        Show();
        keybindInProgress = true;
    }
    /// <summary>
    /// Resets all info and hides the popup
    /// </summary>
    public static void HidePopup() { Instance.settingsName = string.Empty; Instance.fieldTarget = string.Empty; Instance.asAlt = false; Instance.Hide(); }
}// EOF CLASS