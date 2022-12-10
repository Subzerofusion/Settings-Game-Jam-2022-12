using Godot;
using System;
using System.Text;
using static Godot.GD;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

public class Root2D : CanvasLayer {
  // Declare member variables here. Examples:
  // private int a = 2;
  // private string b = "text";

  [Export]
  public NodePath _teInputPath;
  public TextEdit teInput;


  private string userInputMarker = ">>> ";
  private int userInputMarkerLength;

  [Export]
  public float _textSpeedFast = 0.001f;
  [Export]
  public float _textSpeedNormal = 0.05f;
  public float textSpeed;
  public bool completelySkipPrintTime = false;
  private Random random = new Random();


  private bool preventFocus = false;

  public string currentLine {
    get { return teInput.Text.Substr(teInput.Text.LastIndexOf('\n'), teInput.Text.Length); }
  }

  public string lastInput {
    get {
      string text = teInput.Text;
      int nlIndex = text.LastIndexOf('\n');
      int inputIndex = text.LastIndexOf(userInputMarker);
      if (inputIndex > nlIndex) {
        // no new input
        return null;
      } else {
        // input complete
        // Print(text.Substr(inputIndex + userInputMarkerLength, nlIndex));
        return text.Substr(inputIndex + userInputMarkerLength, nlIndex).Trim();
      }

    }
  }

  static string[] POSITIVE = new string[] { "ok", "okay", "yes", "y", "yea", "yeap", "yeet", "yeah", "affirmative", "amen", "fine", "good", "true", "all right", "aye", "beyond a doubt", "by all means", "certainly", "definitely", "even so", "exactly", "gladly", "good enough", "granted", "indubitably", "just so", "most assuredly", "naturally", "of course", "positively", "precisely", "sure thing", "surely", "undoubtedly", "unquestionably", "very well", "willingly", "without fail", "yep" };
  static string[] NEGATIVE = new string[] { "no", "nah", "nope", "decline", "negative", "negatory", "absolutely not", "nein", "veto", "niet" };
  static string[] POSNEGR = new string[] { "C'mon dude... It's just a yes or no question", "Why you gotta make things so complicated?", "Pls... It's not hard..." };

  static string[] GERMAN = new string[] { "nein", "danke", "ja", "bitte" };
  static string[] GERMANR = new string[] { "I don't speak German...", "Uh... Guten Tag?", "Spreken zie English?" };
  static string[] JAPANESE = new string[] { "hai", "iie", "daijoubu", "kawaii", "yamette" };
  static string[] JAPANESER = new string[] { "日本語を分からんよ", "This isn't a visual novel dude...", "Weeb...", "Kimoi..." };
  static string[] RUSSIAN = new string[] { "priviet", "blyatt", "blinn", "niet", "da", "cyka" };
  static string[] RUSSIANR = new string[] { "Я не знаю как говорить по Русски", "", "blinn", "niet", "da" };

  class Command {
    public string Help { get; set; }
    public bool Hidden { get; set; } = false;
    public Action<Root2D, string> Cmd;
  }

  private static Root2D ROOT;

  private static Dictionary<string, Command> COMMANDS = new Dictionary<string, Command>(){
    { "help", new Command{Help = "Shows this list!", Cmd = async(root, line) => {
      Dictionary<string, string> helps = COMMANDS.Where(x=> !x.Value.Hidden ).Select((x) => { return new {x.Key, x.Value.Help};}).ToDictionary(pair=> pair.Key, pair=> pair.Help);
      int longest = 0;
      foreach(string key in helps.Keys) {
        if(key.Length > longest) longest = key.Length;
      }

      List<string> lines = new List<string>();
      foreach(KeyValuePair<string, string> kvp in helps) {
        lines.Add($"{kvp.Key.PadRight(longest, '-')}--{kvp.Value}");
        lines.Add("");
      }
      await root.SayLines(lines.ToArray());
      }}},
    { "stat", new Command{Help = "Shows your current stats.", Cmd = async(root, line) => {
      await root.SayLines(
        $"Health: {root.Player.Health}",
        $"Mana:   {root.Player.Mana}",
        $"{(root.Player.Buffs.Count > 0 ? $"Is afflicted with: {String.Join(", ", root.Player.Buffs.Select(x=>x.Name))}" : "Has no ongoing effects")}"
      );
    }}},
    {"attack", new Command{Help = "Attacks enemy with chosen attack.", Cmd = async(root, line) => {

    }}}
  };

  public enum ResponseType {
    Bool, Str, Cmd
  }

  private Killable Enemy { get; set; } = null;
  private Killable Player { get; set; } = null;

  private async Task Intro() {
    if (!GDUtil.GameSave.FinishedIntro) {
      if (!GDUtil.GameSave.GaveUp) {
        await SayLines(
            "Welcome to the...",
            "",
            " Digitale",
            "  Impact",
            "  Combat",
            " Kolloseum",
            "",
            "Please imagine some fireworks...",
            "",
            "Anyway...",
            "",
            "Use your spacebar to speed up text.",
            "You can type \"help\" at any time for a list of commands!",
            "Your goal is to fight the other digital magi, and claim the\ntitle of Abstract Software Successor for yourself!!",
            "",
            "So are you ready? To become the ultimate ASS?"
          );
        if (await Prompt(ResponseType.Bool) == "true") {
          await SayLines("Excellent! Let us begin...", "", "", "Lettuce begin!", ".", "..", "...", "Lesbians!", ".", ".", ".", "", "Anyway...");
        } else {
          await SayLines("Yeah...", "Alright...");
          await this.Wait(2f);

          GDUtil.GameSave.GaveUp = true;
          GDUtil.Save();
          await CrushAndClose();
        }
      } else {
        await SayLines(
          "Look who's come crawling back...",
          "I'm not going to bother asking, you'd probably say no again",
          "Then I'd have to think of more clever text...",
          "I'll repeat the instructions again",
          "in case you weren't listening.",
          "",
          "Use your spacebar to speed up text.",
          "You can type \"help\" at any time for a list of commands!",
          "Your goal is to fight the other idiots I throw at you.",
          "",
          "Got it?"
        );
        if (await Prompt(ResponseType.Bool) == "true") {
          await Say("Good");
        } else {
          await Say("Too Bad");
        }
      }
      GDUtil.GameSave.FinishedIntro = true;
      GDUtil.Save();
    } else {
      if (GDUtil.GameSave.Name != null) {
        await SayLines($"Welcome back {GDUtil.GameSave.Name}");
      } else {
        await SayLines(
          "It seems like you've been here before."
        );

      }
      await SayLines(
        "Would you like me to repeat instructions?"
      );

      if (await Prompt(ResponseType.Bool) == "true") {
        await SayLines(
          "Use your spacebar to speed up text.",
          "You can type \"help\" at any time for a list of commands!",
          "Your goal is to fight the other idiots I throw at you."
        );
      } else {
        await Say("Cool, lets start.");
      }
    }
  }

  private async Task PlayerName() {
    if (GDUtil.GameSave.Name == null) {
      await SayLines(
        "It seems like I don't know your name yet...",
        "What's your name?"
      );
      string name = await Prompt(ResponseType.Str, unacceptedAnswers: new string[]{
        "aaron", "major triad", "majortriad",
        "blair", "inverseinductor", "inverse inductor", "inverse_inductor", "inverse-inductor", "merseylez",
        "chris", "greener", "me", "sack of cats", "sackofcats", "sackofdicks", "sackofcocks", "sackofass", "hplovecraft",
        "cam", "cameron", "altersquid", "squid",
        "eli", "faldor", "faldor 20", "faldor20",
        "justin", "nitro", "nitroghost", "nitro ghost", "nitro_ghost", "nitro-ghost",
        "mika", "mhear22", "montana",
        "oscar", "lyxaa", "lyxaaa",
        "orlando", "bellicapelli",
        "scott", "ben", "aberrantwolf", "aberrant wolf", "yuzu drink", "yuzudrink", "yuzu_drink", "yuzu-drink",
        "sugma", "ligma"
      },
      fail: new string[]{
        "Nice name... did your mum give it to you? Try again.",
        "Shit name. Try again.",
        "Be more creative dude..."
      });

      foreach (string word in GDUtil.CENSORLMAO) {
        name = name.Replace(word, "*");
      }

      GDUtil.GameSave.Name = name;
      await SayLines(
        "Cool, we can start....",
        $"\"{name}\"...",
        "if that's even your real name"
      );
    }
  }

  private async Task CombatTutorial() {
    bool runTutorial = false;

    if (!GDUtil.GameSave.FinishedCombatTutorial) {
      await SayLines(
        "So",
        "Moving on..."
      );
      runTutorial = true;
    } else {
      await SayLines("Would you like to play the combat tutorial again?");
      if (await Prompt(ResponseType.Bool) == "true") {
        await SayLines(
          "Alrighty then!"
        );
        runTutorial = true;
      }
    }

    if (runTutorial) {
      Player = new Killable() {
        Name = GDUtil.GameSave.Name,
        Health = 100,
        Mana = 100,
        Resistance = 0.1f,
        Attacks = new List<KAction>(){
          new KAction(){Name = "Fire Ball", Damage= 10, Cost = 2},
          new KAction(){Name = "Lightning Bolt", Damage= 40, Cost = 10},
        },
        Items = new List<KAction>() {
          KAction.Items.First(x=>x.Name == "Minor Health Potion"),
          new KAction(){
            Name = "Cursed Amulet of Decimation",
            Description = "Rare and dangerous amulet which decimates the victims health when broken"
          }
        }
      };

      Enemy = new Killable() {
        Name = "Tutorial Dummy",
        Health = 10000,
        Mana = 100,
        Resistance = 0.1f,
        Attacks = new List<KAction>(){
          new KAction(){Name = "Fire Ball", Damage= 10, Cost = 2},
          new KAction(){Name = "Lightning Bolt", Damage= 40, Cost = 10},
        },
        Items = new List<KAction>() { }
      };

      await SayLines(
        "You can type \"stat\" at any time to see your stats",
        "Try it now!"
      );
      string line = await Prompt(ResponseType.Str, acceptedAnswers: new string[] { "stat" }, fail: new string[] { "c'mon, you just gotta type \"stat\"...", "it's not hard...", "please..." });
      COMMANDS["stat"].Cmd(this, line);

    } else {
      await SayLines(
        "Cool!",
        "We're on the blood path now!"
      );
    }
  }

  // Called when the node enters the scene tree for the first time.
  public override async void _Ready() {
    ROOT = this;
    GDUtil.Load();

    teInput = GetNode<TextEdit>(_teInputPath);
    userInputMarkerLength = userInputMarker.Length;

    textSpeed = _textSpeedNormal;
    await Intro();
    await PlayerName();
    await CombatTutorial();
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(float delta) {

  }

  int[] cursorPos = { 0, 0 };
  private void SaveCursorPos() {
    cursorPos[0] = teInput.CursorGetColumn();
    cursorPos[1] = teInput.CursorGetLine();
  }

  private void LoadCursorPos() {
    teInput.CursorSetColumn(cursorPos[0]);
    teInput.CursorSetLine(cursorPos[1]);
  }

  public override void _Input(InputEvent inputEvent) {
    base._Input(inputEvent);

    if (inputEvent is InputEventKey eventKey) {
      if (eventKey.Pressed) {
        // Print(eventKey.Unicode + " " + eventKey.Scancode + " " + eventKey.AsText());
        switch (eventKey.Scancode) {
          case (int)KeyList.Space: {
              Print("changing speeed");
              textSpeed = _textSpeedFast;
              break;
            }
          case (int)KeyList.Escape: {
              completelySkipPrintTime = true;
              break;
            }
          case (int)KeyList.Enter: {
              break;
            }
        }

        if (eventKey.Scancode == (int)KeyList.Backspace) {


        }
      }
    }
  }

  public void OnTEInputFocusExited() {
  }

  public void OnTEInputFocusEntered() {
    if (preventFocus) teInput.ReleaseFocus();
  }

  public void OnTEInputCursorChanged() {
    // if (teInput.CursorGetLine() < lines.Length - 1) {
    //   LoadCursorPos();
    // }

    // if (teInput.CursorGetColumn() < userInputMarkerLength) {
    //   teInput.CursorSetColumn(userInputMarkerLength - 1);
    // }

    // SaveCursorPos();
    // Print(cursorPos[0] + " " + cursorPos[1]);
  }

  public void OnTEInputTextChanged() {
    // lines = teInput.Text.Split("\n");

    // String.Join("\n")

    // text = teInput.Text;
    // Print(teInput.Text);

  }

  public async Task SayLines(params string[] texts) {
    preventFocus = true;
    teInput.ReleaseFocus();

    foreach (string text in texts) {
      await Say(text, false);
      if (!completelySkipPrintTime) await this.Wait(textSpeed * 2);
    }

    preventFocus = false;
    teInput.GrabFocus();
    textSpeed = _textSpeedNormal;
    completelySkipPrintTime = false;
  }

  public async Task Say(string text, bool manageFocus = true) {
    teInput.CursorToEnd();

    if (manageFocus) {
      preventFocus = true;
      teInput.ReleaseFocus();
    }
    for (int i = 0; i < text.Length; i++) {
      teInput.CursorToEnd();
      teInput.InsertTextAtCursor(text[i].ToString());
      if (!completelySkipPrintTime) await this.Wait((float)random.NextDouble() * textSpeed);
    }
    teInput.InsertTextAtCursor("\n");
    if (manageFocus) {
      preventFocus = false;
      teInput.GrabFocus();
      completelySkipPrintTime = false;
    }
  }

  // public async Task<string> WaitForNewLine() {
  //   Print("starting wait " + lines.Length);
  //   int prevLen = lines.Length;
  //   while (prevLen == lines.Length) {
  //     Print(prevLen + " " + lines.Length);
  //     await this.NextFrame();
  //   }
  //   Print("Done waiting");
  //   for (int i = 0; i < lines.Length; i++) {
  //     Print($"{i}: {lines[i]}");

  //   }
  //   Print(currentLine.Remove(0, userInputMarkerLength));
  //   Print("Done waiting");
  //   return currentLine;
  // }

  private string GetRandom(string[] selection) {
    if (selection.Length == 0) return null;
    return selection[new Random().Next(0, selection.Length)];
  }

  async private Task CrushAndClose() {
    Vector2 size = OS.WindowSize;
    Vector2 pos = OS.WindowPosition;
    ulong start = Time.GetTicksMsec();
    float elapsed = 0;
    while (elapsed < Mathf.Pi) {
      elapsed = (Time.GetTicksMsec() - (float)start) * 0.005f;
      OS.WindowSize = new Vector2(size.x, size.y * ((Mathf.Cos(elapsed) + 1) / 2));
      OS.WindowPosition = new Vector2(pos.x, pos.y + (size.y - OS.WindowSize.y) / 2);
      await this.NextFrame();
    }
    GetTree().Quit();
  }

  public async Task<string> Prompt(ResponseType type, string prompt = null, string[] acceptedAnswers = null, string[] unacceptedAnswers = null, string[] fail = null) {
    if (prompt != null) await Say(prompt);

    string result = null;
    do {
      teInput.CursorToEnd();
      teInput.InsertTextAtCursor(userInputMarker);

      while (lastInput == null) {
        await this.NextFrame();
      }
      result = lastInput;

      if (type != ResponseType.Str) {
        if (GERMAN.Contains(result)) {
          await Say(GetRandom(GERMANR));
          result = null;
        }
        if (JAPANESE.Contains(result)) {
          await Say(GetRandom(JAPANESER));
          result = null;
        }
        if (RUSSIAN.Contains(result)) {
          await Say(GetRandom(RUSSIANR));
          result = null;
        }
        if (result == null) continue;
      }

      switch (type) {
        case ResponseType.Bool: {
            if (POSITIVE.Contains(result.ToLower())) {
              result = "true";
            } else if (NEGATIVE.Contains(result.ToLower())) {
              result = "false";
            } else {
              result = null;
              await Say(fail != null ? GetRandom(fail) : GetRandom(POSNEGR));
            }
            break;
          }
        case ResponseType.Cmd: {
            if (COMMANDS.ContainsKey(result.ToLower().Split(' ')[0])) {

            } else {
              result = null;
              await Say(fail != null ? GetRandom(fail) : "I didn't get that :(, maybe try \"help\"?");
            }
            break;
          }
        case ResponseType.Str: {
            if (unacceptedAnswers != null && unacceptedAnswers.Contains(result.ToLower())) {
              result = null;
              await Say(fail != null ? GetRandom(fail) : "I didn't get that :(, try again.");
            }
            if (acceptedAnswers != null) {
              if (!acceptedAnswers.Contains(result.ToLower())) {
                result = null;
                await Say(fail != null ? GetRandom(fail) : "I didn't get that :(, try again.");
              }
            }
            break;
          }
      }
    } while (result == null);
    return result;
  }
}
