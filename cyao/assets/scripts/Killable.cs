using System;
using System.Collections.Generic;
using System.Linq;

public enum KTarget {
  Health, Mana, Resistance
}

public class KAction {
  public string Name { get; set; } = "No Name";
  public int Cost { get; set; } = 0;
  public int Damage { get; set; } = 0;
  public string Description { get; set; } = "No Description";
  public Func<KAction, Killable, Killable, string[]> Effect { get; set; } = null;
  public Func<KAction, Killable, Killable, string[]> PerTurn { get; set; } = null;

  public string[] Execute(Killable user, Killable target) {
    List<string> executionLog = new List<string>();
    if (Cost > user.Mana) executionLog.Add($"{user.Name} doesn't have enough mana");
    executionLog.Add(UsePrint.Replace("{{user}}", user.Name).Replace("{{name}}", Name).Replace("{{target}}", target.Name == user.Name ? "themselves" : target.Name));
    user.Mana -= Cost;
    int dmg = (int)(Damage * target.Resistance);
    target.Health -= dmg;
    if (dmg > 0) executionLog.Add($"it dealt {dmg} damage to {target.Name}");
    if (Effect != null) executionLog.AddRange(Effect(this, user, target));
    return executionLog.ToArray();
  }

  string UsePrint = "{{user}} tried to use {{name}} on {{target}}";

  public static KAction[] Attacks = new KAction[] {

  };

  public static KAction[] Items = new KAction[] {
      new KAction(){
      Name = "Minor Health Potion",
      Effect = (action, user, target) => {
        user.Items.Remove(action);
        target.Health += 10;
        return new string[]{$"has restored 10 health to {target}"};
        }
      },
      new KAction(){
      Name = "OK Health Potion",
      Effect = (action, user, target) => {
        user.Items.Remove(action);
        target.Health += 40;
        return new string[]{$"has restored 40 health to {target}"};
        }
      },
      new KAction(){
      Name = "Minor Health Potion",
      Effect = (action, user, target) => {
        user.Items.Remove(action);
        target.Health += 100;
        return new string[]{$"has restored 100 health to {target}"};
        }
      },
      new KAction(){
      Name = "Liquid Shield",
      Effect = (action, user, target) => {
        user.Items.Remove(action);
        target.Resistance *= 0.5f;
        return new string[]{$"{target}'s resistances have been increased"};
        }
      },
      new KAction(){
      Name = "Throwing Knife",
      Effect = (action, user, target) => {
        user.Items.Remove(action);
        int dmg = (int)(20 * target.Resistance);
        target.Health -= dmg;
        return new string[]{$"{target} has been hit for {dmg} damage"};
        }
      },
      new KAction(){
        Name = "Jar of Fire",
        Effect = (action, user, target) => {
          target.Effects.Add(new KAction() {
            Name = "Fire",
            PerTurn = (a, u, t) => {
              if(new Random().Next(1, 10) < 2) {
                target.Effects.Remove(a);
                return new string[]{$"{target.Name} is no longer on fire"};
              }
              int dmg = new Random().Next(1, 20);
              target.Health -= dmg;
              return new string[]{$"{target.Name} has burned for ${dmg}"};
            }
          });
          return new string[]{$"{target.Name} is now on Fire"};
        },
        Description = "Lights target on fire"
      }
  };
}

public class Killable {
  public string Name { get; set; }
  public int MaxHealth { get; set; }
  public int Health { get; set; }
  public bool IsDead { get { return Health < 0; } }
  public int MaxMana { get; set; }
  public int Mana { get; set; }
  public float Resistance { get; set; }
  public List<KAction> Attacks { get; set; } = new List<KAction>();
  public List<KAction> Items { get; set; } = new List<KAction>();
  public List<KAction> Effects { get; set; } = new List<KAction>();
  public List<string> Engage { get; set; }
  public List<string> Idle { get; set; }
  public List<string> IdleWeak { get; set; }
  public List<string> OnDeath { get; set; }

  public string DamageAsString(int comparisonHealth) {
    double val = Attacks.Select(x => x.Damage).Average() / comparisonHealth;
    if (val > 0.5f)
      return "is pretty fucking scary";
    else if (val > 0.2f) {
      return "is mildly threatening";
    } else if (val > 0.1f) {
      return "is vaguely threatening";
    } else if (val > 0.05f) {
      return "is probably not that threatening";
    } else {
      return "imagine that senator armstrong meme";
    }
  }

  public string ResistanceAsString() {
    if (Resistance > 0.5f) {
      return "low resistance";
    } else {
      return "high resistance";
    }
  }

  public string HealthAsString() {
    if (Mana / MaxMana < 0.01f) {
      return "you don't sense their aura at all";
    } else if (Mana / MaxMana < 0.1f) {
      return "their aura seems to be there";
    } else if (Mana / MaxMana < 0.2f) {
      return "their aura glows, but weakly";
    } else if (Mana / MaxMana < 0.5f) {
      return "their aura gently pulses";
    } else if (Mana / MaxMana < 0.9f) {
      return "is pulsating with energy";
    } else {
      return "is eminating a dangerous aura";
    }
  }

  public string ManaAsString() {
    if (Mana / MaxMana < 0.01f) {
      return "you don't sense their aura at all";
    } else if (Mana / MaxMana < 0.1f) {
      return "their aura seems to be there";
    } else if (Mana / MaxMana < 0.2f) {
      return "their aura glows, but weakly";
    } else if (Mana / MaxMana < 0.5f) {
      return "their aura gently pulses";
    } else if (Mana / MaxMana < 0.9f) {
      return "is pulsating with energy";
    } else {
      return "is eminating a dangerous aura";
    }
  }

  public string ItemsAsString() {
    return Items.Count().ToString();
  }
}