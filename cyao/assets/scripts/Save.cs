using Godot;
using static Newtonsoft.Json.JsonConvert;

public class GameSave {
  public bool GaveUp = false;
  public bool FinishedIntro = false;

  public string Name = null;

  public bool FinishedCombatTutorial = false;

  public Killable CurrentPlayer = null;
}