using System;
using System.Collections.Generic;
using System.Linq;

public enum KTarget {
  Health, Mana, Resistance
}

public class KAction {
  public string Id { get; set; } = "";
  public string Name { get; set; } = "No Name";
  public int Cost { get; set; } = 0;
  public int Damage { get; set; } = 0;
  public string Description { get; set; } = "No Description";
  public Func<Root2D, KAction, Killable, Killable, string[]> Effect { get; set; } = null;
  public Func<Root2D, KAction, Killable, Killable, string[]> PerTurn { get; set; } = null;

  public string[] Execute(Root2D root2d, Killable user, Killable target) {
    List<string> executionLog = new List<string>();
    if (Cost > user.Mana) executionLog.Add($"{user.Name} doesn't have enough mana");
    executionLog.Add(UsePrint.Replace("{{user}}", user.Name).Replace("{{name}}", Name).Replace("{{target.Name}}", target.Name == user.Name ? "themselves" : target.Name));
    user.Mana -= Cost;
    int dmg = (int)(Damage * target.Resistance);
    target.StdDmg(Damage);
    if (dmg > 0) executionLog.Add($"it dealt {dmg} damage to {target.Name}");
    if (Effect != null) executionLog.AddRange(Effect(root2d, this, user, target));
    executionLog.Add("");
    return executionLog.ToArray();
  }

  string UsePrint = "{{user}} tried to use {{name}} on {{target.Name}}";

  public static KAction[] Attacks = new KAction[] {
    new KAction(){Id = "atk_fire_ball", Name = "Fire Ball",
      Description = "A basic incantation which hurtls a fireball at target",
      Damage = 10, Cost = 2},
    new KAction(){Id = "atk_lightning", Name = "Lightning Bolt",
      Description = "LIGHTNING BOLT! LIGHTNING BOLT!! LIGHTNING BOLT!!!",
      Damage = 40, Cost = 10},
    new KAction(){Id = "atk_punch", Name = "Punch",
      Description = "Hand goes into their face! (strictly better than Fire Ball but way less cool)",
      Damage = 10, Cost = 0},
    new KAction(){Id = "atk_stab", Name = "Stab them in the Throat",
      Description = "Stabs the victim in their throat (very effective)",
      Damage = 400, Cost = 0,
      Effect = (root2d, action, user, target) => {
        target.Effects.Add((user == root2d.Player ? Target.Player : Target.Enemy, target == root2d.Player ? Target.Player : Target.Enemy, Effects.First(x=>x.Id=="fx_stab")));
        return new string[]{$"{target.Name} is now bleeding to death" };
      }}
  };

  public static KAction[] Items = new KAction[] {
    new KAction(){ Id = "item_health_pot_1", Name = "Minor Health Potion",
    Description = "Restores 10 health to target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Health += 10;
      return new string[]{$"has restored 10 health to {target.Name}"};
      }
    },
    new KAction(){ Id = "item_health_pot_2", Name = "OK Health Potion",
    Description = "Restores 40 health to target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Health += 40;
      return new string[]{$"has restored 40 health to {target.Name}"};
      }
    },
    new KAction(){ Id = "item_health_pot_3", Name = "Amazing Health Potion",
    Description = "Restores 100 health to target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Health += 100;
      return new string[]{$"has restored 100 health to {target.Name}"};
      }
    },
    new KAction(){ Id = "item_resist_up", Name = "Liquid Shield",
    Description = "Permanently increases resistance",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Resistance *= 0.5f;
      return new string[]{$"{target.Name}'s resistances have been increased"};
      }
    },
    new KAction(){ Id = "item_throwing_knife", Name = "Throwing Knife",
    Description = "Knife which deals 20 damage when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      int dmg = target.StdDmg(20);
      return new string[]{$"{target.Name} has been hit for {dmg} damage"};
      }
    },
    new KAction(){ Id = "item_molotov", Name = "Molotov Cocktail",
      Description = "Lights target on fire",
      Effect = (root2d, action, user, target) => {
        user.Items.Remove(action);
        target.Effects.Add((user == root2d.Player ? Target.Player : Target.Enemy, target == root2d.Player ? Target.Player : Target.Enemy, Effects.First(x=>x.Id=="fx_molotovd")));
        return new string[]{$"{target.Name} is now on Fire"};
      },
    },
    new KAction(){ Id = "item_curse_amulet", Name = "Cursed Amulet of Decimation",
      Description = "Rare and dangerous amulet which decimates the victims health per turn when broken",
      Effect = (root2d, action, user, target) => {
        user.Items.Remove(action);
        target.Effects.Add((user == root2d.Player ? Target.Player : Target.Enemy, target == root2d.Player ? Target.Player : Target.Enemy,  Effects.First(x=>x.Id=="fx_curse")));
        return new string[]{$"{target.Name} is now cursed"};
      },
    },
    new KAction(){ Id = "item_stab_book", Name = "Knife and Book Titled \"Stabbing for Dummies\"",
      Description = "That's actually pretty funny...",
        Effect = (root2d, action, user, target) => {
          user.Items.Remove(action);
          target.Attacks.Add(KAction.Attacks.First(x=>x.Id == "atk_stab"));
          return new string[]{$"{target.Name} has learned how to stab!"};
          }
    }
  };

  public static KAction[] Effects = new KAction[] {
    new KAction() { Id = "fx_curse", Name = "Curse of Decimation",
      PerTurn = (root2d, action, source, holder) => {
        int dmg = holder.Health - holder.Health / 10;
        holder.TrueDmg(dmg);
        return new string[]{$"{holder.Name} has been cursed for {dmg} health"};
      }
    },
    new KAction() { Id = "fx_stab", Name = "Stab Wound",
      Description = "Unit is bleeding to death",
      PerTurn = (root2d, action, opponent, holder) => {
        int dmg = holder.StdDmg(new Random().Next(250, 360));
        return new string[]{$"{ holder.Name } lost {dmg} health from bleeding" };
      }
    },
    new KAction() { Id = "fx_molotovd", Name = "Molotov'd On",
      PerTurn = (root2d, action, opponent, holder) => {
        if(new Random().Next(1, 10) < 2) {
          holder.Effects.Remove(holder.Effects.First(x=>x.Item3.Id=="fx_molotovd"));
          return new string[]{$"{holder.Name} is no longer on fire"};
        }
        int dmg = new Random().Next(1, 20);
        holder.TrueDmg(dmg);
        return new string[]{$"{holder.Name} has burned for {dmg}"};
      }
    }
  };
};

public enum Target {
  Player, Enemy
}

public class Killable {
  public string Name { get; set; }
  public int MaxHealth { get; private set; }
  public int Health { get; set; }
  public bool IsDead { get { return Health <= 0; } }
  public int MaxMana { get; private set; }
  public int Mana { get; set; }
  public float Resistance { get; set; }
  public List<string> AttackIds { get; set; } = new List<string>(); // used only for serialisation in LateInit
  public List<KAction> Attacks { get; set; } = new List<KAction>();
  public List<string> ItemIds { get; set; } = new List<string>(); // used only for serialisation in LateInit
  public List<KAction> Items { get; set; } = new List<KAction>();

  // Target 1 is the source, Target 2 is the holder
  public List<(Target, Target, string)> EffectIds { get; set; } = new List<(Target, Target, string)>(); // used only for serialisation in LateInit

  // Target 1 is the source, Target 2 is the holder
  public List<(Target, Target, KAction)> Effects { get; set; } = new List<(Target, Target, KAction)>();
  public List<string> Engage { get; set; }
  public List<string> Idle { get; set; }
  public List<string> IdleWeak { get; set; }
  public List<string> IdleDieing { get; set; }
  public List<string> OnDeath { get; set; }

  public void LateInit() {
    MaxHealth = Health;
    MaxMana = Mana;
  }

  public void SyncIds() {
    AttackIds = Attacks.Select(x => x.Id).ToList();
    ItemIds = Items.Select(x => x.Id).ToList();
    EffectIds = Effects.Select(x => (x.Item1, x.Item2, x.Item3.Id)).ToList();
  }

  public void SyncActions() {
    Attacks = AttackIds.Select(x => KAction.Attacks.First(y => y.Id == x)).ToList();
    Items = ItemIds.Select(x => KAction.Items.First(y => y.Id == x)).ToList();
    Effects = EffectIds.Select(x => (x.Item1, x.Item2, KAction.Effects.First(y => y.Id == x.Item3))).ToList();
  }

  public delegate void OnDmgEventHandler();
  public event OnDmgEventHandler OnDmgEvent;

  public int StdDmg(int damage) {
    int dmg = (int)(damage * Resistance);
    this.Health -= dmg;
    OnDmgEvent?.Invoke();
    return dmg;
  }

  public void TrueDmg(int damage) {
    this.Health -= damage;
    OnDmgEvent?.Invoke();
  }

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
    if (Health / MaxHealth < 0.01f) {
      return "you don't sense their energy at all";
    } else if (Health / MaxHealth < 0.1f) {
      return "their energy seems to be there";
    } else if (Health / MaxHealth < 0.2f) {
      return "their energy eminates... but weakly";
    } else if (Health / MaxHealth < 0.5f) {
      return "their energy throbs and pulses";
    } else if (Health / MaxHealth < 0.9f) {
      return "is pulsating with energy";
    } else {
      return "is radiating life energy";
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