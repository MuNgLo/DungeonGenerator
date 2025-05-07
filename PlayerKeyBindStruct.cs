using System.Text.Json.Serialization;

namespace Munglo.SettingsModule;

[System.Serializable]
public enum INPUTDEVICE { KEYBOARD = 0, MOUSE = 1, GAMEPAD = 2 }
[System.Serializable]
public class PlayerKeyBind
{
    [JsonInclude] public INPUTDEVICE Device;
    [JsonInclude] public INPUTDEVICE AltDevice;
    [JsonInclude] public Godot.Key Key;
    [JsonInclude] public Godot.Key KeyAlt;
    [JsonInclude] public Godot.MouseButton MouseButton;
    [JsonInclude] public Godot.MouseButton MouseButtonAlt;

}
