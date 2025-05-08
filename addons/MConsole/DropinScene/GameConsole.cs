using Godot;
using System;
/// <summary>
/// This is the class that controls the visual UI elements for the console
/// Make sure you ONLY have one of these in the scene and that the console UI is on top.
/// There should be a ready to dropin scene in same folder.
/// Somewhere in Project you need to also create an isntance of the Manager class.
/// </summary>
public partial class GameConsole : Control
{
    private static GameConsole instance;
    public static EventHandler<string> OnConsoleInputChanged;
    public static EventHandler<string> OnConsoleInputSubmitted;
    [Export] private RichTextLabel outputArea;
    [Export] private LineEdit inputArea;
    [Export] private int maxLineCount = 10;
    /// <summary>
    /// Returns true if console is visible and input area has focus. Use this to limit input while console is open.
    /// </summary>
    public static bool Active => instance.Visible && instance.inputArea.HasFocus();
    public override void _EnterTree()
    {
        instance = this;
        inputArea.TextSubmitted += WhenInputSubmitted;
        inputArea.TextChanged += WhenInputChanged;
        inputArea.TextChangeRejected += WhenInputRejected;
        inputArea.KeepEditingOnTextSubmit = true;
        outputArea.Text = "Console - Welcome to the Despair of the wicked and forgotten. Stay a while. Stay Forever.";
        outputArea.Text += System.Environment.NewLine;
        Hide();
        // To show log messages from your own system do something like this
        //Core.OnLogMessagePushed += WhenLogMessagePushed;
    }

    private void WhenLogMessagePushed(object sender, string[] e)
    {
        OutputAddLines(e);
    }

    private void WhenInputRejected(string rejectedSubstring)
    {
        OutputAddLine($"GameConsole::WhenInputRejected() rejectedSubstring[{rejectedSubstring}]");
    }

    private void WhenInputChanged(string newText)
    {
        OnConsoleInputChanged?.Invoke(this, newText);
    }

    private void WhenInputSubmitted(string newText)
    {
        if (newText.Length < 2) { return; }
        OnConsoleInputSubmitted?.Invoke(this, newText);
        instance.inputArea.Clear();
        instance.inputArea.GrabFocus();
    }
    public static void AddLine(string e){ instance.OutputAddLine(e);}
    public static void AddLines(string[] e){ instance.OutputAddLines(e);}
    private void OutputAddLine(string e){
        OutputAddLines([e]);
    }
    private void OutputAddLines(string[] e)
    {
        outputArea.Text += String.Join(':', e) + System.Environment.NewLine;
        string[] parts = outputArea.Text.Split(System.Environment.NewLine);
        if (parts.Length > maxLineCount)
        {
            if (parts.Length > 1)
            {
                //GD.Print($"GameConsole::OutputAddLine() parts.Length[{parts.Length}] outputArea.GetLineCount()[{outputArea.GetLineCount()}]");
                for (int i = 0; i < parts.Length - maxLineCount - 1; i++)
                {
                    parts[i] = string.Empty;
                }
                string text = string.Empty;
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i] == string.Empty) { continue; }
                    text += parts[i] + System.Environment.NewLine;
                }
                outputArea.Text = text;
                outputArea.ScrollToLine(parts.Length);
            }
        }
    }

    public static bool Toggle()
    {
        if (instance.Visible)
        {
            instance.Hide();
            instance.inputArea.ReleaseFocus();
            return false;
        }
        instance.Show();
        instance.inputArea.Clear();
        // If mouse cursor is visislbe grab focus to the input area
        if(Input.MouseMode == Input.MouseModeEnum.Visible){
            instance.inputArea.GrabFocus();
        }
        return true;
    }

    internal static void SetTip(string tip)
    {
    }

    internal static void SetInputText(string v)
    {
        instance.inputArea.Text = v;
    }

    internal static void ClearInput()
    {
        instance.inputArea.Text = string.Empty;
    }
}// EOF CLASS
