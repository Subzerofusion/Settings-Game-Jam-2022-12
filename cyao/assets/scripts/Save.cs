using Godot;
using static Newtonsoft.Json.JsonConvert;
using System.Collections;
using System.Collections.Generic;

public class GameSave {
  public bool GaveUp = false;
  public bool FinishedIntro = false;

  public string Name = null;

  public bool DiedInTutorial = false;

  public bool DiedToDummy = false;

  public bool QuitCosDied = false;

  public bool FinishedCombatTutorial = false;

  public Killable CurrentPlayer = null;

  public List<string> PreviousInputs = new List<string>();

  public List<string> UnfoughtEnemies = new List<string>();
}