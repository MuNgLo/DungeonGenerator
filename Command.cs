using System;

namespace ConsoleCommands;

public class Command
{
    /// <summary>
    /// Pass in the path/class where the command is getting registered from
    /// </summary>
    /// <param name="sourceNote"></param>
    /// <param name="argumentCount">0 for no arguments, -1 for unlimited</param>
    public Command(string sourceNote, int argumentCount=0){
        Source = sourceNote;
        argCount = argumentCount;
    }
    /// <summary>
    /// Dev note to know where the command was registered from
    /// </summary>
    internal string Source;
    /// <summary>
    /// Name of the command and also the command
    /// </summary>
    internal string Name;
    /// <summary>
    /// Description of command when given no parameters
    /// </summary>
    internal string Tip;
    /// <summary>
    /// Defines the Types the parameters needs to be for the command
    /// </summary>
    internal Type[] args;
    /// <summary>
    /// The action the command runs. It returns what should be output in the console
    /// </summary>
    internal Func<string[], string> act;
    private int argCount;
    internal int ArgCount => argCount;
}// EOF CLASS