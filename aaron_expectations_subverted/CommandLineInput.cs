using Godot;
using System.Linq;
using System;
using System.Text;

public class CommandLineInput : LineEdit
{

    // Please don't look at my code. 
    // No, it is not a fully functional text adventure engine. 
    // It's a hacked together piece of shit operated by a bunch of terrible switch statements.
    // I'm bad at scoping and planning how am i a senior dev :')

    public bool GrueRemoved = false;
    public bool GrueSafeRemoved = false;
    public bool PlayerDead = false;
    public Location[] Locations = new Location[]
    {
        new Location("Field of Grues", "You are standing in an open field. You hear the growls of many creatures all around you.\n\nThere is a small mailbox here.\n\nThere are paths in all directions."),
        new Location("Empty Field", "You are standing in an silent field. A grue stands before you, motionless.")
    };

    public int CurrentLocationIndex = 0;

    public enum CommandTestOption {
        StartsWith,
        EndsWith,
        Contains,
        Exact
    }

    public override void _Ready()
    {
        PlayerDead = false;
        GrueSafeRemoved = false;
        GrueRemoved = false;
        CurrentLocationIndex = 0;
        GetNode<RichTextLabel>("CommandHistory").Text = Locations[CurrentLocationIndex].LocationName + "\n\n" + Locations[CurrentLocationIndex].LocationLookText;
        this.Text = "";
        GD.Randomize();
        OS.SetWindowTitle("NORK v0.1.0");
    }

    public void OnCommandLineInputTextEntered(string input)
    {
        this.Text = string.Empty;
        AppendToCommandHistory("> " + input);
        switch(CurrentLocationIndex)
        {
            case 0:
                HomeCommands(input);
                break;
            case 1:
                GrueLocationCommands(input);
                break;
            default:
                GenericCommands(input);
                break;
        }
        if(PlayerDead)
        {
            AppendToCommandHistory("YOU ARE DEAD! Retry? [Y/N]");
        }
    }

    public void HomeCommands(string input)
    {
        switch (input)
        {
            case string s when !PlayerDead && IsValidCommand(new string[]{"go", "travel", "head", "move"}, s, CommandTestOption.StartsWith)
                            && IsValidCommand(new string[]{"north", "south", "east", "west", "up", "down", "left", "right"}, s, CommandTestOption.Contains):
                MovePlayer(s);
                break;
            case string s when !PlayerDead && IsValidCommand(new string[]{"open", "inspect", "interact", "check"}, s, CommandTestOption.Contains) 
                            && IsValidCommand(new string[]{"mailbox", "mail", "box"}, s, CommandTestOption.Contains):
                OpenMailbox();
                break;
            case string s when !PlayerDead && IsValidCommand(new string[]{"man rm"}, s, CommandTestOption.Exact):
                AppendToCommandHistory("rm - remove objects\nrm [OPTION]... OBJECT...");
                AppendToCommandHistory("Description\nThis manual page documents the GRU version of rm. rm removes each specified object.");
                AppendToCommandHistory("Options\n-e, --empty\n    Replaces the existing object with an empty replacement.");
                break;
            default:
                GenericCommands(input);
                break;
        }
    }

    public void GrueLocationCommands(string input)
    {
        switch(input)
        {
            case string s when !PlayerDead && IsValidCommand(new string[]{"go", "travel", "head", "move"}, s, CommandTestOption.StartsWith)
                            && IsValidCommand(new string[]{"north", "south", "east", "west", "up", "down", "left", "right"}, s, CommandTestOption.Contains):
                AppendToCommandHistory("As you being to move away, you notice that the field stretches on endlessly in all directions. The mailbox you saw earlier has mysteriously vanished");
                AppendToCommandHistory("Only you and the grue remain. You return to face the creature.");
                break;
            case string s when !PlayerDead && IsValidCommand(new string[]{"kill", "defeat", "attack", "damage"}, s, CommandTestOption.Contains):
                if(IsValidCommand(new string[]{"grue", "enemy", "monster"}, s, CommandTestOption.Contains))
                {
                    AppendToCommandHistory("You swing at the lifeless creature with your sword. It topples over with ease.");
                    AppendToCommandHistory("YOU WIN!");
                    this.Editable = false;
                }
                else if(IsValidCommand(new string[]{"me", "myself", "player", "self"}, s, CommandTestOption.Contains))
                {
                    AppendToCommandHistory("It dawns on you that nothing else remains in this world but you and this lifeless creature, trapped forever in an endless field of nothingness.");
                    AppendToCommandHistory("You point the tip of your sword at your stomach and, with one final breath, plunge it deep into your abdomen.");
                    AppendToCommandHistory("Darkness fills your vision, and your consciousness fades, leaving only an empty husk to forever stand in the field forever");
                    AppendToCommandHistory("You win?");
                    this.Editable = false;
                }
                else
                {
                    GenericCommands(input);
                }
                break;
        }
    }

    public void GenericCommands(string input)
    {
        switch(input)
        {
            case string s when !PlayerDead && IsValidCommand(new string[]{"look"}, s, CommandTestOption.Contains):
                AppendToCommandHistory(Locations[CurrentLocationIndex].LocationName);
                AppendToCommandHistory(Locations[CurrentLocationIndex].LocationLookText);
                break;
            case string s when !PlayerDead && IsValidCommand(new string[]{"rm"}, s, CommandTestOption.Contains):
                if(IsValidCommand(new string[]{"-e", "--empty"}, s, CommandTestOption.Contains))
                {
                    DeleteObjectSafe(s);    
                }
                else
                {
                    DeleteObject(s);
                }
                break;
            case string s when PlayerDead && IsValidCommand(new string[]{"y", "yes"}, s, CommandTestOption.Exact):
                _Ready();
                break;
            case string s when PlayerDead && IsValidCommand(new string[]{"n", "no"}, s, CommandTestOption.Exact):
                GetTree().Quit();
                break;
            case string s when !PlayerDead && IsValidCommand(new string[]{"hello", "hi", "hola", "ohayo", "konnichiwa", "good morning", "good day", "greetings", "morning", "good evening", "evening", "gday", "g'day"}, s, CommandTestOption.Exact):
                AppendToCommandHistory(RandomResponse(new string[]{"Hello!", "Nice to meet you!", "Welcome!", "How are you?"}));
                break;
            case string s when IsValidCommand(new string[]{"cls", "clear"}, s, CommandTestOption.Exact):
                GetNode<RichTextLabel>("CommandHistory").Text = string.Empty;
                break;
            case string s when !PlayerDead && IsValidCommand(new string[]{"kill", "defeat", "attack", "damage"}, s, CommandTestOption.Contains):
                if(IsValidCommand(new string[]{"me", "myself", "player", "self"}, s, CommandTestOption.Contains))
                {
                    AppendToCommandHistory("As you enthusiastically stab your sword into your stomach, you wonder what could have possibly possessed you do this. Your consciousness fades before you can figure it out.");
                    PlayerDead = true;
                }
                else
                {
                    AppendToCommandHistory("No target in range.");
                }
                break;
            case string s when !PlayerDead && IsValidCommand(new string[]{"fuck", "shit", "penis", "pussy", "cunt", "asshole", "piss"}, s, CommandTestOption.Contains):
                AppendToCommandHistory(RandomResponse(new string[]{"You kiss your mother with that mouth?", "Charming", "If you don't have anything nice to say, you shouldn't say anything at all", "Nice potty mouth"}));
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

    public void MovePlayer(string input)
    {
        var direction = TrimStart(TrimStart(TrimStart(input.ToLower(), "go"), "travel"), "head").Trim();
        AppendToCommandHistory($"You move {direction}.");
        if(GrueRemoved && !GrueSafeRemoved)
        {
            Random rnd = new Random((int)GD.Randi());
            byte[] randomBytes = new byte[10];
            rnd.NextBytes(randomBytes);
            var randString = Encoding.ASCII.GetString(randomBytes);
            AppendToCommandHistory($"You were eaten by a {randString}");
            FakeCrash();
        }
        else if(GrueRemoved && GrueSafeRemoved)
        {
            CurrentLocationIndex = 1;
            AppendToCommandHistory("A grue is standing here lifelessly. You flinch at the first sight of the creature, but notice it makes no attempt to eat you.");
        }
        else
        {
            PlayerDead = true;
            AppendToCommandHistory("You were eaten by a grue.");
        }
    }

    public void DeleteObject(string input)
    {
        var objectToDelete = input.Split(' ').Last();
        if(!input.ToLower().StartsWith("sudo"))
        {
            AppendToCommandHistory($"rm: cannot remove '{objectToDelete}': Permission Denied");
        }
        else
        {
            if(objectToDelete == "grue")
            {
                AppendToCommandHistory($"{objectToDelete} has been removed.");
                GrueRemoved = true;
            }
            else
            {
                AppendToCommandHistory($"ERROR: Could not remove object '{objectToDelete}' - Not found.");
            }
        }
    }

    public void DeleteObjectSafe(string input)
    {
        var objectToDelete = input.Split(' ').Last();
        if(!input.ToLower().StartsWith("sudo"))
        {
            AppendToCommandHistory($"rm: cannot remove '{objectToDelete}': Permission Denied");
        }
        else
        {
            if(objectToDelete == "grue")
            {
                AppendToCommandHistory($"{objectToDelete} has been removed and replaced with and empty object.");
                GrueRemoved = true;
                GrueSafeRemoved = true;
            }
            else
            {
                AppendToCommandHistory($"ERROR: Could not remove object '{objectToDelete}' - Not found.");
            }
        }
    }
    


    public void FakeCrash()
    {
        GetNode<ColorRect>("../FakeCrashOverlay").Visible = true;
        OS.SetWindowTitle("NORK v0.1.0 (Not Responding)");
        GetNode<Timer>("../CrashTimer").Start();
    }

    public void OpenMailbox()
    {
        AppendToCommandHistory("Opening the mailbox reveals a leaflet. It reads:"
            + "\n\nWelcome to Nork!"
            + "\n\nThis is a game of adventure, danger, and low cunning. In it you will explore some of the most amazing territory ever seen by mortal man. Hardened adventurers have run screaming from the terrors contained within.EOLEOL>man rm�����");
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

public class Location
{
    public Location(){}
    public Location(string locationName, string locationLookText)
    {
        LocationName = locationName;
        LocationLookText = locationLookText;
    }
    public string LocationName {get;set;}
    public string LocationLookText{get;set;}
}

public class Item
{
    public Item(){}
    public Item(string itemName, string itemDescription)
    {
        ItemName = itemName;
        ItemDescription = itemDescription;
    }
    public string ItemName {get;set;}
    public string ItemDescription{get;set;}
}