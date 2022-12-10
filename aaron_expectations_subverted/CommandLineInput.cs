using Godot;
using System.Linq;
using System;
using System.Text;

public class CommandLineInput : LineEdit
{

    public bool GrueRemoved = false;

    public enum CommandTestOption {
        StartsWith,
        EndsWith,
        Contains,
        Exact
    }

    public override void _Ready()
    {
        this.Text = string.Empty;
        GD.Randomize();
        OS.SetWindowTitle("NORK v0.1.0");
    }

    public void OnCommandLineInputTextEntered(string input)
    {
        this.Text = string.Empty;
        AppendToCommandHistory("> " + input);
        switch (input)
        {
            case string s when IsValidCommand(new string[]{"go", "travel", "head", "move"}, s, CommandTestOption.StartsWith)
                            && IsValidCommand(new string[]{"north", "south", "east", "west", "up", "down", "left", "right"}, s, CommandTestOption.Contains):
                MovePlayer(s);
                break;
            case string s when IsValidCommand(new string[]{"open", "inspect", "interact", "check"}, s, CommandTestOption.Contains) 
                            && IsValidCommand(new string[]{"mailbox", "mail"}, s, CommandTestOption.Contains):
                OpenMailbox();
                break;
            case string s when IsValidCommand(new string[]{"fuck", "shit", "penis", "pussy", "cunt", "asshole", "fag", "faggot", "piss"}, s, CommandTestOption.Contains):
                AppendToCommandHistory(RandomResponse(new string[]{"You kiss your mother with that mouth?", "Charming", "If you don't have anything nice to say, you shouldn't say anything at all", "Nice potty mouth"}));
                break;
            case string s when IsValidCommand(new string[]{"rm", "del"}, s, CommandTestOption.Contains):
                DeleteObject(s);
                break;
            default:
                AppendToCommandHistory(RandomResponse(new string[]{"Sorry, I don't understand.", "What was that?", "Could you repeat that?"}));
                break;
        }
    }

    public string RandomResponse(string[] possibleResponses)
    {
        return possibleResponses[GD.Randi() % possibleResponses.Length];
    }

    public bool IsValidCommand(string[] validCommands, string testString, CommandTestOption commandTestOption)
    {
        var wordsInString = testString.Split(' ').Select(m => m.ToLower());
        switch(commandTestOption)
        {
            case CommandTestOption.Contains:
                return validCommands.Any(m => wordsInString.Contains(m.ToLower()));
            case CommandTestOption.StartsWith:
                return validCommands.Any(m => wordsInString.First() == m.ToLower());
            case CommandTestOption.EndsWith:
                return validCommands.Any(m => wordsInString.Last() == m.ToLower());
            case CommandTestOption.Exact:
                return validCommands.Any(m => testString.ToLower() == m.ToLower());
            default:
                throw new IndexOutOfRangeException("Not sure how you got here but you tried to perform an unsupported comparison");
        }
    }

    public bool IsOpenMailBoxCommand(string s)
    {
        return (s.ToLower().Contains("open")
            || s.ToLower().Contains("inspect")
            || s.ToLower().Contains("interact")
            || s.ToLower().Contains("check"))
            && (s.ToLower().Contains("mailbox")
            || s.ToLower().Contains("mail"));
    }

    public void MovePlayer(string input)
    {
        var direction = TrimStart(TrimStart(TrimStart(input.ToLower(), "go"), "travel"), "head").Trim();
        AppendToCommandHistory($"You move {direction}.");
        if(GrueRemoved)
        {
            Random rnd = new Random((int)GD.Randi());
            byte[] randomBytes = new byte[10];
            rnd.NextBytes(randomBytes);
            var randString = Encoding.ASCII.GetString(randomBytes);
            AppendToCommandHistory($"You were eaten by a {randString}");
            FakeCrash();
        }
        else
        {
            AppendToCommandHistory("You were eaten by a grue.");
        }
    }

    public void DeleteObject(string input)
    {
        var objectToDelete = TrimStart(TrimStart(input.ToLower(), "sudo").Trim(), "rm").Trim().Capitalize();
        if(!input.ToLower().StartsWith("sudo"))
        {
            AppendToCommandHistory($"rm: cannot remove '{objectToDelete}': Permission Denied");
        }
        else
        {
            AppendToCommandHistory($"{objectToDelete} has been removed.");
            GrueRemoved = true;
        }
    }

    public void FakeCrash()
    {
        GetNode<ColorRect>("../FakeCrashOverlay").Visible = true;
        OS.SetWindowTitle("NORK v0.1.0 (Not Responding)");
    }

    public void OpenMailbox()
    {
        AppendToCommandHistory("You open the mail box. Nothing is inside.");
    }

    public void AppendToCommandHistory(string commandText)
    {
        var commandHistory = GetNode<RichTextLabel>("CommandHistory");
        commandHistory.AddText("\n\n" + commandText);
    }

    public static string TrimStart(string target, string trimString)
    {
        if (string.IsNullOrEmpty(trimString)) return target;

        string result = target;
        while (result.StartsWith(trimString))
        {
            result = result.Substring(trimString.Length);
        }

        return result;
    }
}
