using System;
using System.Collections.Generic;
using Godot;
namespace Munglo.MenuSystem;
public partial class HUDSystem : Control
{
    private static HUDSystem Instance;
    private Dictionary<string, Control> HUDElements = new Dictionary<string, Control>();
    private bool isUp = false;
    [Export] private bool withCursor = false;
    static public bool IsUp => Instance != null ? Instance.isUp : false;

    [Export] private Control[] persistent;
    public override void _EnterTree()
    {
        Instance = this;
        foreach (Node element in GetChildren())
        {
            if (element is Control)
            {
                if (HUDElements.ContainsKey(element.Name))
                {
                    GD.Print($"HUD system: Trying to register already registered element {element.Name}");
                    return;
                }
                HUDElements[element.Name] = element as Control;
            }
        }
        HideAllElements();
    }
    public override void _Ready()
    {
        MenuSystem.OnMenuVisibilityChanged += WhenMenuVisChanged;
    }

    private void WhenMenuVisChanged(object sender, bool e)
    {
        if (isUp)
        {
            Visible = !e;
            if (withCursor)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
        }
    }

    public static void HideAll()
    {
        Instance.HideAllElements();
        Instance.ShowPersistent();
    }

    private void ShowPersistent()
    {
        foreach (Control node in persistent)
        {
            node.Show();
        }
    }

    private void HideAllElements()
    {
        foreach (string elementName in HUDElements.Keys)
        {
            HUDElements[elementName].Hide();
            HUDElements[elementName].ProcessMode = ProcessModeEnum.Disabled;
        }
        UpdateIsUp();
        withCursor = false;
    }

    private void UpdateIsUp()
    {
        foreach (KeyValuePair<string, Control> element in HUDElements)
        {
            if (element.Value.Visible)
            {
                isUp = true;
                if (withCursor)
                {
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                }
                else
                {
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                }
                return;
            }
        }
        isUp = false;
    }

    public static void ToggleElement(string elementName, bool syncMouse = false)
    {
        if (Instance.HUDElements.ContainsKey(elementName))
        {
            if (Instance.HUDElements[elementName].Visible)
            {
                Instance.HUDElements[elementName].Hide();
                Instance.HUDElements[elementName].ProcessMode = ProcessModeEnum.Disabled;
            }
            else
            {
                Instance.HUDElements[elementName].Show();
                Instance.HUDElements[elementName].ProcessMode = ProcessModeEnum.Inherit;
            }
            if (syncMouse)
            {
                if (Instance.HUDElements[elementName].Visible)
                {
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                    Instance.withCursor = true;
                }
                else
                {
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                    Instance.withCursor = false;
                }
            }
        }
        Instance.UpdateIsUp();
    }
    public static void ShowElement(string elementName, bool showCursor = false)
    {
        if (Instance.HUDElements.ContainsKey(elementName))
        {
            Instance.HUDElements[elementName].Show();
            Instance.HUDElements[elementName].ProcessMode = ProcessModeEnum.Inherit;
            if (showCursor)
            {


                Instance.withCursor = true;
            }
        }
        Instance.UpdateIsUp();
    }
    public static void HideElement(string elementName, bool hideCursor = false)
    {
        if (Instance.HUDElements.ContainsKey(elementName))
        {
            Instance.HUDElements[elementName].Hide();
            Instance.HUDElements[elementName].ProcessMode = ProcessModeEnum.Disabled;
            if (hideCursor)
            {
                Instance.withCursor = false;
            }
        }
        Instance.UpdateIsUp();
    }
    public static void ShowElements(string[] elementNames, bool showCursor = false)
    {
        for (int i = 0; i < elementNames.Length; i++)
        {
            ShowElement(elementNames[i], showCursor);
        }
        Instance.UpdateIsUp();
    }
    public static Control GetElementNode(string name)
    {
        if (Instance.HUDElements.ContainsKey(name))
        {
            return Instance.HUDElements[name];
        }
        return null;
    }

    internal static void ShowHUD()
    {
        HideAll();
        Instance.isUp = true;
    }
}// EOF CLASS