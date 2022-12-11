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


  [Export]
  public NodePath _crSkyrimPath;
  public ColorRect crSkyrim;


  private string userInputMarker = ">>> ";
  private int userInputMarkerLength;

  [Export]
  public float _textSpeedFast = 0.001f;
  [Export]
  public float _textSpeedNormal = 0.05f;
  public float textSpeed;
  public bool oneLineAtATime = false;
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
  static string[] RUSSIANR = new string[] { "Я не знаю как говорить по Русски", "blinn", "niet", "da" };

  class Command {
    public string Help { get; set; }
    public string Format { get; set; }
    public bool IsTurn { get; set; } = false;
    public bool Hidden { get; set; } = false;
    public Func<Root2D, string, Task> Cmd;
  }

  private static Dictionary<string, Command> COMMANDS = new Dictionary<string, Command>(){
    { "help", new Command {Help = "Shows this list!", Cmd = async(root, line) => {
      string[] split = line.Split(" ");
      if(split.Count() != 1) {
        await root.SayLines(
          "\"help\" has no arguments!",
          ""
        );
      }
      List<string> lines = new List<string>();
      foreach(KeyValuePair<string, Command> kvp in COMMANDS) {
        lines.Add($"{kvp.Key} {kvp.Value.Format}");
        lines.Add($"  {kvp.Value.Help}");
        lines.Add($"  {(kvp.Value.IsTurn ? "This command counts as a turn": "This command does not count as a turn")}");
        lines.Add("");
      }
      await root.SayLines(lines.ToArray());
      }}},
    { "stat", new Command {Help = "Shows your current stats.", Cmd = async(root, line) => {
      string[] split = line.Split(" ");
      if(split.Count() != 1) {
        await root.SayLines(
          "\"stat\" only has no arguments!",
          "use \"help\" for more info.",
          ""
        );
      }
      await root.SayPlayerStats();
    }}},
    { "inspect", new Command {Format = "[list:{attack | item | effects}] [index:{int}]", Help = "Inspects an item or attack by index", Cmd = async(root, line) => {
      string[] split = line.Split(" ");
      if(split.Count() != 3) {
        await root.SayLines(
          "\"inspect\" only has 2 arguments!",
          "use \"help\" for more info.",
          ""
        );
        return;
      }
      if(split[1] != "attack" && split[1] != "item" && split[1] != "effects") {
        await root.SayLines(
          $"There are no {split[1]} is not an inspectable property",
          ""
        );
        return;
      }
      List<KAction> actionSet;
      switch(split[1]) {
        case "attack": {
          actionSet = root.Player.Attacks;
          break;
        }
        case "item":{
          actionSet = root.Player.Items;
          break;
        }
        case "effect": {
          actionSet = root.Player.Effects.Select(x=>x.Item3).ToList();
          break;
        }
        default:{
          await root.SayLines(
            "unrecognised target list",
            "use \"help\" for more info.",
            ""
          );
          return;
        }
      }

      if(actionSet.Count() < 1) {
        await root.SayLines(
            $"There are no {split[1]}s to inspect",
            ""
          );
          return;
      }

      int index;
      if(!int.TryParse(split[2], out index)) {
        await root.SayLines(
          "specified index was not an integer",
          ""
        );
        return;
      }
      if(index < 0 || index >= actionSet.Count()) {
        await root.SayLines(
          $"index was not within range available {split[1]}{(actionSet.Count() != 1 ? "s" : "")}",
          "use \"stat\" to see how many options there are.",
          ""
        );
        return;
      }

      KAction action = actionSet[index];
      await root.SayLines(
        $"{action.Name}:",
        $"  {action.Description}",
        "",
        $"  Cost:        {action.Cost} mana",
        $"  Damage:      {action.Damage}"
      );

    }}},
    { "attack", new Command {Format = "[index:{int}]", IsTurn = true, Help = "Attacks enemy with chosen attack.", Cmd = async(root, line) => {
      string[] split = line.Split(" ");
      if(split.Count() != 2) {
        await root.SayLines(
          "\"attack\" only has 1 argument!",
          "use \"help\" for more info.",
          ""
        );
        return;
      }
      if(root.Enemy == null) {
        await root.SayLines("There're no enemies to attack", "");
        return;
      }

      int index;
      if(!int.TryParse(split[1], out index)) {
        await root.SayLines(
          "specified index was not an integer",
          ""
        );
        return;
      }
      List<KAction> actionSet = root.Player.Attacks;
      if(index < 0 || index >= actionSet.Count()) {
        await root.SayLines(
          $"index was not within range available {split[1]}{(actionSet.Count() != 1 ? "s" : "")}",
          "use \"stat\" to see how many options there are.",
          ""
        );
        return;
      }
      KAction action = actionSet[index];
      await root.SayLines(action.Execute(root, root.Player, root.Enemy));
    }}},
    { "use", new Command {Format = "[index:{int}] [target:{\"self\" | \"enemy\"}]", IsTurn = true, Help = "Uses an item on target.", Cmd = async(root, line) => {
      string[] split = line.Split(" ");
      if(split.Count() != 3) {
        await root.SayLines(
          "\"attack\" only has 2 arguments!",
          "use \"help\" for more info.",
          ""
        );
        return;
      }
      int index;
      if(!int.TryParse(split[1], out index)) {
        await root.SayLines(
          "specified index was not an integer",
          ""
        );
        return;
      }
      List<KAction> actionSet = root.Player.Items;
      if(index < 0 || index >= actionSet.Count()) {
        await root.SayLines(
          $"index was not within range available {split[1]}{(actionSet.Count() != 1 ? "s" : "")}",
          "use \"stat\" to see how many options there are.",
          ""
        );
        return;
      }
      KAction action = actionSet[index];

      Killable target;
      switch(split[2]) {
        case "self":{
          target = root.Player;
          break;
        }
        case "enemy":{
          if(root.Enemy == null) {
            await root.SayLines(
              $"there's no enemy to use that on!",
              ""
            );
          }
          target = root.Enemy;
          break;
        }
        default:{
          await root.SayLines(
            "unrecognised target",
            "target yourself with \"self\"",
            "or the enemy with \"enemy\"",
            "use \"help\" for more info.",
            ""
          );
          return;
        }
      }

      await root.SayLines(action.Execute(root, root.Player, target));
    }}},
    { "analyse", new Command {Format = "[stat:{ \"damage\" | \"health\" | \"resistance\" | \"mana\" | \"items\" }]", Help = "Analyses the enemy", Cmd = async(root, line) => {
      string[] split = line.Split(" ");
      if(split.Count() != 2) {
        await root.SayLines(
          "\"analyse\" only has 1 argument!",
          "use \"help\" for more info.",
          ""
        );
        return;
      }

      if(root.Enemy == null) {
        await root.SayLines(
          "There is no enemy to analyse",
          ""
        );
        return;
      }

      if(!new string[]{"damage", "health", "resistance", "mana", "items"}.Contains(split[1])) {
        await root.SayLines(
          $"{split[1]} is not a stat that can be analysed",
          ""
        );
        return;
      }

      switch(split[1]) {
        case "damage": {
          await root.Say(root.Enemy.DamageAsString(root.Player.Health));
          break;
        }
        case "health": {
          await root.Say(root.Enemy.HealthAsString());
          break;
        } case
        "resistance": {
          await root.Say(root.Enemy.ResistanceAsString());
          break;
        }
        case "mana": {
          await root.Say(root.Enemy.ManaAsString());
          break;
        }
        case "items": {
          await root.Say(root.Enemy.ItemsAsString());
          break;
        }
      }



      // await root.SayLines(lines.ToArray());
      }}},
  };

  public enum ResponseType {
    Bool, Str, Cmd
  }

  public Killable Enemy { get; set; } = null;
  public Killable Player { get; set; } = null;

  private async Task Intro() {
    if (GDUtil.GameSave.QuitCosDied) {
      GDUtil.GameSave.QuitCosDied = false;
      await this.Wait(2f);
      Task srf = SkyrimFade(3);
      await SayLines(
        "Ah",
        "you're awake I see!"
      );
      if (GDUtil.GameSave.DiedInTutorial) {
        GDUtil.GameSave.DiedInTutorial = false;
        await SayLines("Fucking idiot....");
      }
      if (GDUtil.GameSave.DiedToDummy) {
        GDUtil.GameSave.DiedToDummy = false;
        await SayLines("That guy's pretty scary", "I'd give up if I were you");
      }
      await srf;
    } else {
      await SkyrimFade(0);
    }

    if (!GDUtil.GameSave.FinishedIntro) {
      if (!GDUtil.GameSave.GaveUp) {
        await SayLines(
            "Welcome to the...",
            "",
            " Colosseum of",
            "   Untamed",
            "    Magic",
            "",
            "Please imagine some fireworks...",
            "",
            "Anyway...",
            "",
            "Use escape to cycle text speed.",
            "Your goal is to fight the other digital magi, and claim the\ntitle of Champion Usurper Man/Madame for yourself!!",
            "",
            "So are you ready?",
            "",
            "To be CUM?"
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
          "Use escape to cycle text speed.",
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
      if (GDUtil.GameSave.Name != null && GDUtil.GameSave.Name.Trim() != "") {
        await SayLines($"Welcome back {GDUtil.GameSave.Name}");
      }
      await SayLines(
        "Would you like me to repeat instructions?"
      );

      if (await Prompt(ResponseType.Bool) == "true") {
        await SayLines(
          "Use your spacebar to speed up text.",
          "Your goal is to fight the other idiots I throw at you."
        );
      } else {
        await Say("Cool, lets start.");
      }
    }
  }

  private async Task SkyrimFade(float seconds) {
    float start = (float)Time.GetTicksMsec() / 1000f;
    float elapsed = (float)Time.GetTicksMsec() / 1000f - start;
    while (elapsed < seconds) {
      elapsed = (float)Time.GetTicksMsec() / 1000f - start;
      crSkyrim.Color = new Color(0, 0, 0, 1 - elapsed / seconds);
      await this.NextFrame();
    }
    crSkyrim.Color = new Color(0, 0, 0, 0);
  }

  private async Task SkyrimUnfade(float seconds) {
    float start = (float)Time.GetTicksMsec() / 1000f;
    float elapsed = (float)Time.GetTicksMsec() / 1000f - start;
    while (elapsed < seconds) {
      elapsed = (float)Time.GetTicksMsec() / 1000f - start;
      crSkyrim.Color = new Color(0, 0, 0, elapsed / seconds);
      await this.NextFrame();
    }
    crSkyrim.Color = new Color(0, 0, 0, 0);
  }

  private async Task PlayerName() {
    if (GDUtil.GameSave.Name == null) {
      await SayLines(
        "It seems like I don't know your name yet...",
        "What's your name?"
      );
      string name = await Prompt(ResponseType.Str, unacceptedAnswers: new string[]{
        "aaron", "major triad", "majortriad",
        "avery", "ay", "neofisho",
        "blair", "inverseinductor", "inverse inductor", "inverse_inductor", "inverse-inductor", "merseylez",
        "chris", "greener", "me", "sack of cats", "sackofcats", "sackofdicks", "sackofcocks", "sackofass",
        "cam", "cameron", "altersquid", "squid",
        "eli", "faldor", "faldor 20", "faldor20",
        "ellie", "eleanor", "scribbel", "lungs",
        "jeremy", "jer", "alphastrata", "jerk", "alpha",
        "justin", "nitro", "nitroghost", "nitro ghost", "nitro_ghost", "nitro-ghost",
        "mika", "mhear22", "montana", "mika mika mii", "mikamikamii", "mikamii",
        "oscar", "lyxaa", "lyxaaa",
        "orlando", "bellicapelli", "belli",
        "scott", "ben", "sharper", "aberrantwolf", "aberrant wolf", "yuzu drink", "yuzudrink", "yuzu_drink", "yuzu-drink", "pahzy", "rynban", "pahzy rynban",
        "sugma", "ligma"
      },
      fail: new string[]{
        "Nice name... did your mum give it to you? Try again.",
        "Shit name. Try again.",
        "Stupid. Try again.",
        "Be more creative dude..."
      });

      foreach (string word in GDUtil.CENSORLMAO) {
        name = name.Replace(word, "*");
      }

      GDUtil.GameSave.Name = name;
      GDUtil.Save();
      await SayLines(
        "Cool, we can start....",
        $"\"{name}\"...",
        "if that's even your real name"
      );
    }
  }

  Killable defaultPlayer = new Killable() {
    Name = GDUtil.GameSave.Name,
    Health = 100,
    Mana = 100,
    Resistance = 0.9f,
    Attacks = new List<KAction>(){
        KAction.Attacks.First(x=> x.Id == "atk_fire_ball"),
        KAction.Attacks.First(x=> x.Id == "atk_lightning")
    },
    Items = new List<KAction>() {
      KAction.Items.First(x=> x.Id == "item_health_pot_1"),
      KAction.Items.First(x=> x.Id == "item_stab_book")
    }
  };

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
        Resistance = 0.9f,
        Attacks = new List<KAction>(){
          KAction.Attacks.First(x=> x.Id == "atk_fire_ball"),
          KAction.Attacks.First(x=> x.Id == "atk_lightning")
        },
        Items = new List<KAction>() {
          KAction.Items.First(x=> x.Id == "item_health_pot_1"),
          KAction.Items.First(x=> x.Id == "item_curse_amulet")
        }
      };

      Player.LateInit();
      Player.OnDmgEvent += () => { ScreenShake(0.25f, 8, 4, 0.01f); };

      await SayPlayerStats();
      await SayLines(
        "This is you!",
        "You can type \"stat\" at any time to see your stats",
        "Try it now!",
        ""
      );
      string line = await Prompt(ResponseType.Str, acceptedAnswers: new string[] { "stat" }, fail: new string[] { "c'mon, you just gotta type \"stat\"...", "it's not hard...", "please..." });
      await COMMANDS["stat"].Cmd(this, line);

      await SayLines(
        "Cool beans!",
        "Use \"help\" to see what else we can use...",
        ""
      );

      line = await Prompt(ResponseType.Str, acceptedAnswers: new string[] { "help" }, fail: new string[] { "no secrets, I won't let you pass until you use \"help\"" });
      await COMMANDS["help"].Cmd(this, line);

      await SayLines(
        "Have a look at your items and attacks",
        "with help from the \"help\" command",
        "type \"done\" when you want to continue",
        ""
      );

      int diePhase = 0;
      do {
        int itemCount = Player.Items.Count();
        line = await Prompt(ResponseType.Str);
        string name = line.Split(" ")[0];
        if (COMMANDS.ContainsKey(name)) {
          await COMMANDS[name].Cmd(this, line);
          foreach (var action in Player.Effects) {
            await SayLines(action.Item3.PerTurn(this, action.Item3, action.Item1 == Target.Player ? Player : Enemy, action.Item2 == Target.Player ? Player : Enemy));
          }
          if (Player.Items.Count() < itemCount) {
            await SayLines(
              "Why'd you use that? We're only in the tutorial!",
              ""
            );
          }
          if (Player.Effects.Count() > 0) {
            await SayLines(
              new String[] { "Oh good, and you're dieing now", "Well... Bye I guess?", "It's been fun?", "...", "...", "..." }[diePhase++],
              ""
            );
          }
          if (Player.IsDead) {
            await this.Wait(1f);
            GDUtil.GameSave.DiedInTutorial = true;
            GDUtil.GameSave.QuitCosDied = true;
            GDUtil.Save();
            await CrushAndClose();
          }
          // await COMMANDS["stat"].Cmd(this, "");
        } else if (line != "done") {
          await SayLines("Unrecognised Command", "");
        }

      } while (line != "done");

      Enemy = new Killable() {
        Name = "Tutorial Dummy",
        Health = 10000000,
        Mana = 10000000,
        Resistance = 0.1f,
        Attacks = new List<KAction>() {
          new KAction(){Name = "Punch", Damage = 10, Cost = 0},
          new KAction(){Name = "Murder", Damage = 100000, Cost = 0}
        },
        Items = new List<KAction>() {
          KAction.Items.First(x=>x.Id == "item_stab_book")
        },
        Engage = new List<string>() {
          "<Looks like it wants to kick the shit out of you!>"
        },
        Idle = new List<string>() {
          "<an indecipherable amalgamation of screams and cries of the damned>",
          "<writhes with the bodies it's filled with> (wait.. what?)",
          "<screams in pain (not because of your actions)>",
          "<repositions the corpses within>",
          ""
        },
        IdleWeak = new List<string>() {
          "<flipflops uncomfortably>",
          "<flails in anger>",
          "<stops moving... and then very suddenly starts moving again>",
          "<screams with anguish>"
        },
        IdleDieing = new List<string>() {
          "<writhes weakly>",
          "<there seems to be a movement within... but not much>",
          "<seems to be leaking putrid ooze>"
        },
        OnDeath = new List<string>() {
          "<bursts at the seams and the malevolent darkness within is pulled into the ground by dark claw-like hands and tendrils>"
        }
      };
      Enemy.LateInit();

      await SayLines($"{Enemy.Name}: {GetRandom(Enemy.Engage)}", "");
      await SayLines(
            "OH SHIT WADDUP!",
            "",
            "It looks like some idiot wants to fight!",
            "hit 'em with the \"analyse\"!",
            "type \"done\" when you're ready to move on",
            ""
          );

      OS.WindowResizable = true;

      do {
        line = await Prompt(ResponseType.Str);
        string[] split = line.Split(" ");
        string name = split[0];
        string tgt = null;
        if (split.Length > 1) {
          tgt = split[1];
        }
        if (COMMANDS.ContainsKey(name)) {
          if (!new string[] { "help", "analyse" }.Contains(name)) {
            await SayLines(
              GetRandom(new string[] {
                "Yeah, I know that's a command but lets stay on topic",
                "I'm only gonna let you use \"help\" and \"analyse\" for now...",
                "C'mon, just play a long please..." }),
              ""
            );
          } else {
            await COMMANDS[name].Cmd(this, line);
            if (name == "analyse") {
              if (tgt != null) {
                if (new string[] { "damage", "health", "resistance", "mana" }.Contains(tgt)) {
                  await SayLines(
                    GetRandom(new string[] {
                      "Oh...",
                      "Oh no...",
                      "Oh dear...",
                      "Jesus...",
                      "How... horrifying...",
                      "I swear this was meant to be a tutorial dude...",
                      "Is this real?",
                      "God..." }),
                    ""
                  );
                } else {
                  await SayLines(
                    GetRandom(new string[] {
                      "Yeah, alright, this one's alright" }
                    ),
                    ""
                  );
                }
              }
            }
          }
        } else if (line != "done") {
          await SayLines("Unrecognised Command", "");
        }
      } while (line != "done");

      await SayLines(
        "Well... I guess you just gotta fight gigachad right here...",
        ""
      );

      while (!Enemy.IsDead) {
        if (Enemy.Health > Enemy.MaxHealth * 0.5f) {
          await SayLines($"{Enemy.Name}: {GetRandom(Enemy.Idle)}");
        } else if (Enemy.Health > Enemy.MaxHealth * 0.25f) {
          await SayLines($"{Enemy.Name}: {GetRandom(Enemy.IdleWeak)}");
        } else {
          await SayLines($"{Enemy.Name}: {GetRandom(Enemy.IdleDieing)}");
        }
        line = await Prompt(ResponseType.Str);
        string name = line.Split(" ")[0];
        if (COMMANDS.ContainsKey(name)) {
          await COMMANDS[name].Cmd(this, line);

          if (COMMANDS[name].IsTurn) {
            foreach (var effect in Enemy.Effects) {
              // effect.Item1 will always be self
              // effect.Item2 will 

              await SayLines(effect.Item3.PerTurn(this, effect.Item3, effect.Item1 == Target.Player ? Player : Enemy, effect.Item2 == Target.Player ? Player : Enemy));
            }

            if (Enemy.IsDead) {
              break;
            }

            await SayLines(Enemy.Attacks[0].Execute(this, Enemy, Player));

            foreach (var effect in Player.Effects) {
              await SayLines(effect.Item3.PerTurn(this, effect.Item3, effect.Item1 == Target.Player ? Player : Enemy, effect.Item2 == Target.Player ? Player : Enemy));
            }
            if (Player.IsDead) {
              await SayLines("Looks like you're dead :(", "");
              await this.Wait(1f);
              GDUtil.GameSave.DiedToDummy = true;
              GDUtil.GameSave.QuitCosDied = true;
              GDUtil.Save();
              await CrushAndClose();
            }
          }
        }
      }


      await SayLines($"{Enemy.Name} has been defeated!", "", $"{Enemy.Name}: {GetRandom(Enemy.OnDeath)}");


      Player.Effects.Clear();
      await SayLines(
        "",
        "You've been cleared of any ongoing ailments",
        ""
      );

      await SayLines(
        "I guess that guy just knew how to punch",
        "Luckily you spawned in with that thing",
        "It looks like they dropped their items too!",
        ""
      );

      int? selection = null;
      while (selection == null) {
        await SayLines("Choose an item to take using its number", "");
        int i = 0;
        await SayLines(Enemy.Items.Select(x => $"{i}: {x.Name}").ToArray());
        await SayLines("");
        if (int.TryParse(await Prompt(ResponseType.Str), out int s)) {
          if (s < Enemy.Items.Count() && s >= 0) {
            selection = s;
          } else {
            await SayLines("They didn't have that many items...", "");
          }
        }
      }
      KAction item = Enemy.Items[(int)selection];
      Player.Items.Add(item);

      await SayLines(
        $"You've acquired {item.Name}",
        ""
      );
    }

    await SayLines(
      "Cool!",
      "We're on the blood path now!",
      "Lets get murderin'!"
    );
    GDUtil.GameSave.FinishedCombatTutorial = true;
    GDUtil.Save();
    OS.WindowResizable = true;
  }

  // Called when the node enters the scene tree for the first time.
  public override async void _Ready() {
    GDUtil.Load();
    OS.WindowResizable = false;
    OS.WindowSize = new Vector2(640, 480);

    teInput = GetNode<TextEdit>(_teInputPath);
    crSkyrim = GetNode<ColorRect>(_crSkyrimPath);
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

  int currentTextSpeed = 0;

  int? historyIndex = null;
  public override void _Input(InputEvent inputEvent) {
    base._Input(inputEvent);

    if (inputEvent is InputEventMouseButton eventButton) {
      if (eventButton.Pressed) {
        switch (eventButton.ButtonIndex) {
          case (int)ButtonList.WheelUp: {
              teInput.ScrollVertical -= 1;
              break;
            }
          case (int)ButtonList.WheelDown: {
              teInput.ScrollVertical += 1;
              break;
            }
        }
      }
    }

    if (inputEvent is InputEventKey eventKey) {
      if (eventKey.Pressed) {
        // Print(eventKey.Unicode + " " + eventKey.Scancode + " " + eventKey.AsText());
        switch (eventKey.Scancode) {
          case (int)KeyList.Backspace: {
              if (teInput.HasFocus() && teInput.CursorGetColumn() <= userInputMarkerLength) {
                teInput.InsertTextAtCursor(" ");
              }
              break;
            }
          case (int)KeyList.Escape: {
              currentTextSpeed = (currentTextSpeed + 1) % 3;
              switch (currentTextSpeed) {
                case 0:
                  completelySkipPrintTime = false;
                  oneLineAtATime = false;
                  textSpeed = _textSpeedNormal;
                  break;
                case 1:
                  completelySkipPrintTime = false;
                  oneLineAtATime = false;
                  textSpeed = _textSpeedFast;
                  break;
                case 2:
                  oneLineAtATime = true;
                  completelySkipPrintTime = false;
                  break;
                case 3:
                  oneLineAtATime = false;
                  completelySkipPrintTime = true;
                  break;
              }
              break;
            }
          case (int)KeyList.Enter: {
              break;
            }
          case (int)KeyList.Up: {

              // start the count if not started yet
              if (historyIndex == null) {
                historyIndex = 0;
              } else { // increment if we have
                historyIndex++;
              }
              // bind it to size of array
              historyIndex = Math.Min((int)historyIndex, GDUtil.GameSave.PreviousInputs.Count());
              string line = GDUtil.GameSave.PreviousInputs[(int)historyIndex];

              teInput.Select(int.MaxValue, userInputMarkerLength, int.MaxValue, int.MaxValue);
              teInput.InsertTextAtCursor(line);
              break;
            }
          case (int)KeyList.Down: {
              // start the count if not started yet
              if (historyIndex == null) {
                historyIndex = 0;
              } else { // increment if we have
                historyIndex--;
              }
              // bind it to size of array
              historyIndex = Math.Max((int)historyIndex, 0);
              string line = GDUtil.GameSave.PreviousInputs[(int)historyIndex];

              teInput.Select(int.MaxValue, userInputMarkerLength, int.MaxValue, int.MaxValue);
              teInput.InsertTextAtCursor(line);
              break;
            }
          case (int)KeyList.F1: {
              ScreenShake(0.2f, 4, 2, 0.01f);
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
    teInput.CursorSetLine(int.MaxValue);

    if (teInput.CursorGetColumn() < userInputMarkerLength) {
      teInput.CursorSetColumn(userInputMarkerLength);
    }

    SaveCursorPos();
    // Print(cursorPos[0] + " " + cursorPos[1]);
  }

  public void OnTEInputTextChanged() {
    // lines = teInput.Text.Split("\n");

    // String.Join("\n")

    // text = teInput.Text;
    // Print(teInput.Text);

  }

  public async Task SayPlayerStats() {
    await SayLines(
        $"{Player.Name}:",
        $"  Health: {Player.Health}",
        $"  Mana:   {Player.Mana}",
        "");

    if (Player.Attacks.Count() == 0) {
      await Say($"  Has No attacks:");
    } else {
      int i = 0;
      await Say($"  Attacks:");
      await SayLines(Player.Attacks.Select(x => $"    {i++}. {x.Name}").ToArray());
    }
    await SayLines("");

    if (Player.Items.Count() == 0) {
      await Say($"  Has No items:");
    } else {
      int i = 0;
      await Say($"  Items:");
      await SayLines(Player.Items.Select(x => $"    {i++}. {x.Name}").ToArray());
    }
    await SayLines("");

    if (Player.Effects.Count() == 0) {
      await Say($"  Has no ongoing effects:");
    } else {
      int i = 0;
      await Say($"  Effects:");
      await SayLines(Player.Attacks.Select(x => $"    {i++}. {x.Name}").ToArray());
    }
    await SayLines("");
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
      if (!completelySkipPrintTime && !oneLineAtATime) await this.Wait((float)random.NextDouble() * textSpeed);
    }
    teInput.InsertTextAtCursor("\n");
    if (manageFocus) {
      preventFocus = false;
      teInput.GrabFocus();
    }
  }

  private string GetRandom(List<string> selection) {
    return GetRandom(selection.ToArray());
  }

  private string GetRandom(string[] selection) {
    if (selection.Length == 0) return null;
    return selection[new Random().Next(0, selection.Length)];
  }

  async private Task ScreenShake(float duration, float howShaky, float howBroken, float howWonk) {
    float startTime = Time.GetTicksMsec() / 1000f;
    float elapsed = Time.GetTicksMsec() / 1000f - startTime;

    Vector2 winPos = OS.WindowPosition;
    Vector2 winSize = OS.WindowSize;
    teInput.SetRotation(teInput.GetRotation() + (float)(random.NextDouble() - 0.5f) * 2 * howWonk);

    while (elapsed < duration) {
      elapsed = Time.GetTicksMsec() / 1000f - startTime;
      OS.WindowPosition = new Vector2(winPos.x + (float)(random.NextDouble() - 0.5f) * 2 * howShaky, winPos.y + (float)(random.NextDouble() - 0.5f) * 2 * howShaky);
      OS.WindowSize = new Vector2(winSize.x + (float)(random.NextDouble() - 0.5f) * 2 * howBroken, winSize.y + (float)(random.NextDouble() - 0.5f) * 2 * howBroken);
    }

    OS.WindowPosition = winPos;
  }

  async private Task CrushAndClose() {
    OS.WindowResizable = false;
    SkyrimUnfade(0.2f);
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
  int historyLength = 200;
  public async Task<string> Prompt(ResponseType type, string prompt = null, string[] acceptedAnswers = null, string[] unacceptedAnswers = null, string[] fail = null) {

    if (Player != null) await Say($"Health: {Player.Health} Mana: {Player.Mana}{(Enemy != null ? " vs " + Enemy.Name : "")}");
    if (prompt != null) await Say(prompt);

    string result = null;
    do {
      teInput.CursorToEnd();
      teInput.InsertTextAtCursor(userInputMarker);

      while (lastInput == null) {
        await this.NextFrame();
      }
      result = lastInput;
      GDUtil.GameSave.PreviousInputs.Insert(0, result);
      if (GDUtil.GameSave.PreviousInputs.Count() > historyLength) {
        GDUtil.GameSave.PreviousInputs.RemoveRange(historyLength, GDUtil.GameSave.PreviousInputs.Count() - historyLength);
      }
      GDUtil.Save();
      historyIndex = null;

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
