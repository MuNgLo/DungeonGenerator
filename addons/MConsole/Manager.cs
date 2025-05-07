using System;
using System.Collections.Generic;

namespace ConsoleCommands;
/// <summary>
/// Main class of the consolesystem. Make an instance of this where you think it fits into the project.
/// Lets say ytou have a godclass static like Core. Then make an instance under it and expose it through a property like
/// Core.Commands
/// Then use "Core.Commands.RegisterCommand" to register in your commands
/// 
/// Use cmd_ as prefix for built in commands
/// </summary>
public class Manager
{
    private Dictionary<string, Command> commands;
    private int historyLength = 20;
    private int historyIndex = 0;
    private List<string> history;

    public Manager()
    {
        commands = new Dictionary<string, Command>();
        history = new List<string>();
        GameConsole.OnConsoleInputChanged += WhenConsoleInputChange;
        GameConsole.OnConsoleInputSubmitted += WhenConsoleInputSubmitted;
        AddDefaultCommands();
    }

    private void AddDefaultCommands()
    {
        // Source check a command
        RegisterCommand(new Command("ConsoleSystem Default")
        {
            Name = "cmd_source",
            Tip = "Check the source of a registered command",
            act = s => { return "Registered from: " + GetCommandSource(s[1]); },
            args = [typeof(string), typeof(string)]
        });
    }

    private string GetCommandSource(string cmdName)
    {
        if (commands.ContainsKey(cmdName))
        {
            return commands[cmdName].Source;
        }
        return $"The command \"{cmdName}\" isn't registered.";
    }

    /// <summary>
    /// When input is sent to console this is where it gets processed
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="incomming"></param>
    private void WhenConsoleInputSubmitted(object sender, string incomming)
    {
        // add to history
        history.Add(incomming);
        // Clamp history
        if (history.Count > 20) { history.RemoveAt(0); }
        // Set history back to present
        historyIndex = history.Count;
        // Split the incomming string
        string[] args = incomming.Trim().Split(' ');
        // Check if there is a command registered
        if (commands.ContainsKey(args[0]))
        {
            // Get the commmand from the dictionary
            Command cmd = commands[args[0]];

            if (args.Length == 1)
            {
                // Check if the command was given withouth any params when it should. If so give tip
                if (cmd.ArgCount > 0)
                {
                    GameConsole.AddLine(commands[args[0]].Tip);
                    return;

                }
                else
                {
                    // Run the command and push return string to console
                    GameConsole.AddLine(cmd.act.Invoke(args));
                    return;
                }

            }else if (args.Length == cmd.ArgCount + 1)
            {
                // Run the command and push return string to console
                GameConsole.AddLine(cmd.act.Invoke(args));
                return;

            }
            GameConsole.AddLine($"Command failed! CMD[{cmd.Name}] ArgCount[{cmd.ArgCount}] args.Length[{args.Length}]");
        }
    }
    /// <summary>
    /// Register a command to the console. Does a check for preexisting command
    /// </summary>
    /// <param name="cmd"></param>
    /// <returns></returns>
    public bool RegisterCommand(Command cmd)
    {
        if (commands.ContainsKey(cmd.Name))
        {
            GameConsole.AddLine($"Registering command \"{cmd.Name}\" failed. It already registered!");
            return false;
        }
        commands[cmd.Name] = cmd;
        return commands.ContainsKey(cmd.Name);
    }
    /// <summary>
    /// Walk history pointer and set input area of console to that stored command
    /// </summary>
    public void HistoryUp()
    {
        if (history.Count < 1) { return; }
        historyIndex = Math.Clamp(historyIndex - 1, 0, Math.Min(history.Count - 1, historyLength));
        GameConsole.SetInputText(history[historyIndex]);
    }
    /// <summary>
    /// Walk history pointer and set input area of console to that stored command
    /// </summary>
    public void HistoryDown()
    {
        if (history.Count < 1) { return; }
        if (history.Count - 1 == historyIndex)
        {
            historyIndex = history.Count; GameConsole.ClearInput();
            return;
        }
        historyIndex = Math.Clamp(historyIndex + 1, 0, Math.Min(history.Count - 1, historyLength));
        GameConsole.SetInputText(history[historyIndex]);
    }
    /// <summary>
    /// TODO use this for autocompletion!
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void WhenConsoleInputChange(object sender, string e)
    {
        if (commands.ContainsKey(e))
        {
            GameConsole.SetTip(commands[e].Tip);
        }
    }
}// EOF CLASS