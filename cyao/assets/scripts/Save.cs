using Godot;
using static Newtonsoft.Json.JsonConvert;
using System.Collections;
using System.Collections.Generic;

public class GameSave {
  public bool GaveUp = false;
  public bool FinishedIntro = false;

  public string Name = null;
  public int MaxHealth = 1;
  public int Health = 1;
  public int MaxMana = 1;
  public int Mana = 1;
  public float Resistance = 1;
  public List<string> AttackIds = null;
  public List<string> ItemIds = null;

  public bool PlayerSaved = false;

  public bool DiedInTutorial = false;

  public bool DiedToDummy = false;

  public bool QuitCosDied = false;

  public bool WonGame = false;

  public bool FinishedCombatTutorial = false;

  public List<string> PreviousInputs = new List<string>();

  public List<string> UnfoughtEnemies = new List<string>();
}