using System;
using System.Collections.Generic;
using System.Linq;

public enum KTarget {
  Health, Mana, Resistance
}

public class KAction {
  public string Id { get; set; } = "";
  public string Name { get; set; } = "No Name";
  public bool GoodForSelf { get; set; } = false;
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
    new KAction(){ Id = "atk_fire_ball", Name = "Fire Ball",
      Description = "A basic incantation which hurtls a fireball at target",
      Damage = 10, Cost = 2},
    new KAction(){ Id = "atk_lightning", Name = "Lightning Bolt",
      Description = "LIGHTNING BOLT! LIGHTNING BOLT!! LIGHTNING BOLT!!!",
      Damage = 41, Cost = 10},
    new KAction(){ Id = "atk_punch", Name = "Punch",
      Description = "Hand goes into their face! (strictly better than Fire Ball but way less cool)",
      Damage = 10, Cost = 0},
    new KAction(){ Id = "atk_stab", Name = "Stab them in the Throat",
      Description = "Stabs the victim in their throat (very effective)",
      Damage = 400, Cost = 0,
      Effect = (root2d, action, user, target) => {
        target.Effects.Add((user == root2d.Player ? Target.Player : Target.Enemy, target == root2d.Player ? Target.Player : Target.Enemy, Effects.First(x=>x.Id=="fx_stab")));
        return new string[]{$"{target.Name} is now bleeding to death" };
      }},
    new KAction(){ Id = "atk_pos_charge", Name = "Positive Slash",
      Description = "Stores a charge on the target, when opposite charge is applied, charges are consumed, dealing the damage again as true damage",
      Damage = 10, Cost = 0,
      Effect = (root2d, action, user, target) => {
        target.Effects.Add((user == root2d.Player ? Target.Player : Target.Enemy, target == root2d.Player ? Target.Player : Target.Enemy, Effects.First(x=>x.Id=="fx_pos")));
        int dmg  = target.Effects.RemoveAll(x=>x.Item3.Id == "fx_neg");
        target.TrueDmg(dmg * 10);
        var list = new List<string>();
        if(dmg > 0) {
          list.Add($"{dmg} negative charges have been triggered for {dmg*10} damage");
        }
        list.Add($"{target.Name} has had a positive charge applied to them.");
        return list.ToArray();
      }},
    new KAction(){ Id = "atk_neg_charge", Name = "Negative Stab",
      Description = "Stores a charge on the target, when opposite charge is applied, charges are consumed, dealing the damage again as true damage",
      Damage = 400, Cost = 0,
      Effect = (root2d, action, user, target) => {
        target.Effects.Add((user == root2d.Player ? Target.Player : Target.Enemy, target == root2d.Player ? Target.Player : Target.Enemy, Effects.First(x=>x.Id=="fx_neg")));
        int dmg  = target.Effects.RemoveAll(x=>x.Item3.Id == "fx_pos");
        target.TrueDmg(dmg * 10);
        var list = new List<string>();
        if(dmg > 0) {
          list.Add($"{dmg} positive charges have been triggered for {dmg*10} damage");
        }
        list.Add($"{target.Name} has had a negative charge applied to them.");
        return list.ToArray();
      }},
    new KAction(){ Id = "atk_exp_strike", Name = "Exponential Strike",
      Description = "Strikes for more damage per use",
      Damage = 1, Cost = 0,
      Effect = (root2d, action, user, target) => {
        action.Damage *= 2;
        return new string[]{"Exponential strike just got more powerful"};
      }},
    new KAction(){ Id = "atk_focus", Name = "Focused Blast",
      Description = "Focuses all mana into an attack",
      Damage = 0, Cost = 0,
      Effect = (root2d, action, user, target) => {
        int dmg = user.Mana;
        target.TrueDmg(user.Mana);
        user.Mana = 0;
        return new string[]{$"{user.Name} used all their mana to attack ${target.Name} for {dmg} damage"};
      }
    },
    new KAction(){ Id = "atk_knife_game", Name = "Knife Game",
      Description = "Attack starts at 0 damage but increases in damage per \"miss\".",
      Damage = 0, Cost = 5,
      Effect = (root2d, action, user, target) => {
        if(new Random().Next(0, 10) == 0) {
          int count = user.Effects.RemoveAll(x=>x.Item3.Id == "fx_knife_miss");
          action.Damage = (action.Damage + count) * 20;
          return new string[] { $"{user.Name} has hit {target.Name} for {target.StdDmg(action.Damage)} damage" };
        } else {
          user.Effects.Add((user == root2d.Player ? Target.Player : Target.Enemy, target == root2d.Player ? Target.Player : Target.Enemy, Effects.First(x=>x.Id=="fx_knife_miss")));
          return new string[] { $"{user.Name} has missed, strike damage has increased." };
        }
      }},
    new KAction(){ Id = "atk_deep", Name = "Tendrils of the Deep",
      Description = "Inflicts malevolent visions which deal damage over time",
      Damage = 0, Cost = 30,
        Effect = (root2d, action, user, target) => {
          target.Effects.Add((user == root2d.Player ? Target.Player : Target.Enemy, target == root2d.Player ? Target.Player : Target.Enemy, Effects.First(x=>x.Id=="fx_tendrils")));
          return new string[] { $"{target.Name} has been aflicted with malefic visions" };
        }},
    new KAction(){ Id = "atk_forage", Name = "Forage",
      Description = "Finds and uses a random item",
      Damage = 10, Cost = 30,
        Effect = (root2d, action, user, target) => {
          var actionLog = new List<string>();
          var item = KAction.Items[new Random().Next(0, KAction.Items.Count())];
          actionLog.Add($"{user.Name} has found ${item.Name}");
          if(item.GoodForSelf) {
            actionLog.AddRange(item.Execute(root2d, user, user));
          } else {
            actionLog.AddRange(item.Execute(root2d, user, target));
          }
          return actionLog.ToArray();
        }},
    new KAction(){ Id = "atk_toughen", Name = "Toughen",
      Description = "Permanently increases own resistances",
      Damage = 0, Cost = 30,
      Effect = (root2d, action, user, target) => {
        user.Resistance *= 0.2f;
        return new string[]{$"{user.Name} has become tougher"};
      }},
    new KAction(){ Id = "atk_gaslight", Name = "Gaslight",
      Description = "Use an opponents item on yourself as them",
      Effect = (root2d, action, user, target) => {
        var item = target.Items[new Random().Next(0, target.Items.Count())];
        return item.Execute(root2d, target, user);
      }},
    new KAction(){ Id = "atk_gatekeep", Name = "GateKeep",
      Description = "Increases resistances to max for 1 turn",
      Effect = (root2d, action, user, target) => {
        return new string[]{$"{target.Name} is being Gatekept"};
      }},
    new KAction(){ Id = "atk_girlboss", Name = "Girlboss",
      Description = "Uses one of the opponents attacks against them",
      Effect = (root2d, action, user, target) => {
        var attack = target.Attacks[new Random().Next(0, target.Items.Count())];
        return attack.Execute(root2d, user, target);
      }},
    new KAction(){ Id = "atk_manaburn", Name = "Mana Burn",
      Description = "Burns targets mana, transfers to self",
      Effect = (root2d, action, user, target) => {
        int burn = new Random().Next(20-300);
        target.Mana -= burn;
        user.Mana += burn;
        return new string[]{$"{user.Name} has burned {burn} Mana from {target.Name}"};
      }},
    new KAction(){ Id = "atk_oxidise", Name = "Oxidise",
      Description = "Reduces target resistance",
      Effect = (root2d, action, user, target) => {
        target.Resistance *= 2;
        return new string[]{$"{target.Name}'s resistances fall"};
      }},
    new KAction(){ Id = "atk_signal", Name = "Signal",
      Description = "Adds a one of 3 types of marks to the target that can be activated later",
      Effect = (root2d, action, user, target) => {
        target.Effects.Add((user == root2d.Player ? Target.Player : Target.Enemy, target == root2d.Player ? Target.Player : Target.Enemy, Effects.First(x=>x.Id==new string[]{"fx_health","fx_mana","fx_res"}[new Random().Next(0, 3)])));
        return new string[]{$"{target.Name}'s resistances fall"};
      }},
    new KAction(){ Id = "atk_delegate", Name = "Delegate",
      Description = "Adds a one of 3 types of marks to the target that can be activated later",
      Effect = (root2d, action, user, target) => {
        var effects = target.Effects.FindAll(x=>new string[]{"fx_health","fx_mana","fx_res"}.Contains(x.Item3.Id));
        List<string> actionList = new List<string>();
        foreach(var effect in effects) {
          actionList.AddRange(effect.Item3.Effect(root2d, effect.Item3, user, target));
        }
        return actionList.ToArray();
      }},
    new KAction(){ Id = "atk_leave", Name = "Leave",
      Description = "Unit has had enough and leaves combat",
      Effect = (root2d, action, user, target) => {
        user.Health = 0;
        return new string[]{$"{user.Name} has had enough. They're leaving."};
      }},
    new KAction(){ Id = "atk_rebuild", Name = "Rebuild",
      Description = "Swaps target health and mana",
      Effect = (root2d, action, user, target) => {
        var h = target.Health;
        var m = target.Mana;

        target.Health = m;
        target.Mana = h;

        return new string[]{$"{target.Name} target's mana and health have been swapped."};
      }},
    new KAction(){ Id = "atk_facts", Name = "Facts",
      Description = "If the target's health is greater than their mana, deal the difference in damage.",
      Effect = (root2d, action, user, target) => {
        var dmg = target.Health - target.Mana;
        target.TrueDmg(dmg);
        return new string[]{$"{target.Name} has been hit by the facts for {dmg} damage"};
      }},
    new KAction(){ Id = "atk_logic", Name = "Logic",
      Description = "If the target's mana is greater than their health, deal the difference in damage.",
      Effect = (root2d, action, user, target) => {
        var dmg = target.Mana - target.Health;
        target.TrueDmg(dmg);
        return new string[]{$"{target.Name} has been hit by the logic for {dmg} damage"};
      }},
    new KAction(){ Id = "atk_polarise", Name = "Polarise",
      Description = "Divide the target's health by its lowest non 1 factor.",
      Effect = (root2d, action, user, target) => {
        int lowestFactor = 1;
        for(; lowestFactor < target.Health; lowestFactor++) {
          if(target.Health % lowestFactor == 0) break;
        }
        int endHealth = target.Health / lowestFactor;

        var dmg = target.Health - endHealth;
        target.TrueDmg(dmg);

        return new string[]{$"{target.Name} has had their health divided by the lowest factor ({lowestFactor}) for {dmg} damage"};
      }},
    new KAction(){ Id = "atk_repair", Name = "Repair",
      Description = "Repairs own health back to full",
      Effect = (root2d, action, user, target) => {
        user.Health = user.MaxHealth;
        return new string[]{$"{user.Name} has repaired all damage taken"};
      }},
    new KAction(){ Id = "atk_slam", Name = "Slam",
      Description = "Deals damage to target based on user's health",
      Effect = (root2d, action, user, target) => {
        var dmg = target.StdDmg(user.Health / 2);
        return new string[]{$"{target.Name} been slammed for {dmg} damage"};
      }},
    new KAction(){ Id = "atk_7_female_wives", Name = "Seven Female Wives",
      Description = "Once user has acquired all Seven Female Wives, target is killed instantly",
      Effect = (root2d, action, user, target) => {
        user.Effects.Add((user == root2d.Player ? Target.Player : Target.Enemy, target == root2d.Player ? Target.Player : Target.Enemy, Effects.First(x=>x.Id=="fx_female_wife")));
        var wifeCount = user.Effects.Where(x=>x.Item3.Id == "fx_female_wife").Count();
        if(wifeCount == 1) {
          return new string[]{$"{user.Name} has acquired 1 Female Wife and is 6 away from victory."};
        } else if (wifeCount >= 7) {
          target.TrueDmg(target.Health);
          return new string[]{$"{user.Name} has acquired all 7 Female Wives"};
        } else {
          return new string[]{$"{user.Name} has acquired {wifeCount} Female Wives and is {7 - wifeCount} away from victory."};
        }
      }},
    new KAction(){ Id = "atk_clutter", Name = "Clutter",
      Description = "Spawns random items in user's inventory",
      Effect = (root2d, action, user, target) => {
        int itemCount = new Random().Next(0,5);
        for(int i = 0; i < itemCount; i++) {
          user.Items.Add(KAction.Items[new Random().Next(0, KAction.Items.Length)]);
        }
        return new string[]{$"{itemCount} new item{(itemCount == 1 ? "s" : "")} have spawned in {user.Name}'s inventory"};
      }},
    new KAction(){ Id = "atk_sweep", Name = "Swept Under the Rug",
      Description = "Puts entire user inventory in target's inventory",
      Effect = (root2d, action, user, target) => {
        int itemCount = user.Items.Count();
        target.Items.AddRange(user.Items);
        user.Items.Clear();
        return new string[]{$"{itemCount} item{(itemCount == 1 ? "s" : "")} have been transfered to {target.Name}'s inventory"};
      }},
    new KAction(){ Id = "akt_collapse", Name = "Collapse",
      Description = "Destroys target inventory, deals damage based on number of items destroyed",
      Effect = (root2d, action, user, target) => {
        int itemCount = target.Items.Count();
        target.Items.Clear();
        int dmg = target.TrueDmg(itemCount * new Random().Next(4,20));
        return new string[]{$"{itemCount} item{(itemCount == 1 ? "s" : "")} have been destroyed, dealing {dmg} damage to {target.Name}"};
      }},
    new KAction(){ Id = "atk_bongcloud", Name = "Bong Cloud",
      Description = "Afflicts target with status effect Bonged",
      Effect = (root2d, action, user, target) => {
        target.Effects.Add((user == root2d.Player ? Target.Player : Target.Enemy, target == root2d.Player ? Target.Player : Target.Enemy, Effects.First(x=>x.Id=="fx_bonged")));
        return new string[]{$"{target.Name} is now {target.Effects.Where(x=>x.Item3.Id == "fx_bonged")} Bonged"};
      }},
    new KAction(){ Id = "atk_blaze", Name = "Blaze",
      Description = "Consume target's Bonged effects to divide their health by how Bonged they are.",
      Effect = (root2d, action, user, target) => {
        int bongs = target.Effects.RemoveAll(x=>x.Item3.Id=="fx_bonged");

        int dmg = target.Health - target.Health / bongs;
        target.TrueDmg(dmg);

        return new string[]{$"{target.Name} was Blazed for {dmg} damage"};
      }},
    new KAction(){ Id = "atk_build", Name = "Build",
      Damage = 20, Cost = 20,
      Description = "Increases the damage dealt by deploy",
      Effect = (root2d, action, user, target) => {
        var attack = user.Attacks.First(x=>x.Id=="atk_deploy");
        attack.Damage += 20;
        return new string[]{$"{user.Name}'s Deploy has increased in damage"};
      }},
    new KAction() {Id = "atk_deploy", Name = "Deploy",
      Damage = 20, Cost = 20,
      Description = "Increases the damage dealt by build",
      Effect = (root2d, action, user, target) => {
        var attack = user.Attacks.First(x=>x.Id=="atk_build");
        attack.Damage += 20;
        return new string[] { $"{user.Name}'s Build has increased in damage" };
      }
    },
  };

  public static KAction[] ForbiddenItems = new KAction[] {
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

  public static KAction[] Items = new KAction[] {
    new KAction(){ Id = "item_health_pot_1", Name = "Minor Health Potion", GoodForSelf = true,
    Description = "Restores 10 health to target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Health += 10;
      return new string[]{$"has restored 10 health to {target.Name}"};
      }
    },
    new KAction(){ Id = "item_health_pot_2", Name = "OK Health Potion", GoodForSelf = true,
    Description = "Restores 40 health to target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Health += 40;
      return new string[]{$"has restored 40 health to {target.Name}"};
      }
    },
    new KAction(){ Id = "item_health_pot_3", Name = "Amazing Health Potion", GoodForSelf = true,
    Description = "Restores 100 health to target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Health += 100;
      return new string[]{$"has restored 100 health to {target.Name}"};
      }
    },
    new KAction(){ Id = "item_health_pot_4", Name = "Phenomenal Health Potion", GoodForSelf = true,
    Description = "Restores 500 health to target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Health += 500;
      return new string[]{$"has restored 500 health to {target.Name}"};
      }
    },
    new KAction(){ Id = "item_health_multiplier_1", Name = "Health Multiplier", GoodForSelf = true,
    Description = "Doubles health of target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Health *= 2;
      return new string[]{$"{target.Name}'s health has been doubled"};
      }
    },
    new KAction(){ Id = "item_health_multiplier_2", Name = "Greater Health Multiplier", GoodForSelf = true,
      Description = "Quadruples health of target when used.",
      Effect = (root2d, action, user, target) => {
        target.Health *= 4;
        return new string[]{$"{target.Name}'s health has been quadrupled"};
      }
    },
    new KAction(){ Id = "item_mana_pot_1", Name = "Minor Mana Potion", GoodForSelf = true,
    Description = "Restores 10 mana to target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Mana += 10;
      return new string[]{$"has restored 10 mana to {target.Name}"};
      }
    },
    new KAction(){ Id = "item_mana_pot_2", Name = "OK Mana Potion", GoodForSelf = true,
    Description = "Restores 40 mana to target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Mana += 40;
      return new string[]{$"has restored 40 mana to {target.Name}"};
      }
    },
    new KAction(){ Id = "item_mana_pot_3", Name = "Amazing Mana Potion", GoodForSelf = true,
    Description = "Restores 100 mana to target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Mana += 100;
      return new string[]{$"has restored 100 mana to {target.Name}"};
      }
    },
    new KAction(){ Id = "item_mana_pot_4", Name = "Phenomenal Mana Potion", GoodForSelf = true,
    Description = "Restores 500 mana to target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Mana += 500;
      return new string[]{$"has restored 500 mana to {target.Name}"};
      }
    },
    new KAction(){ Id = "item_mana_multiplier_1", Name = "Mana Multiplier", GoodForSelf = true,
    Description = "Doubles mana of target when used.",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Mana *= 2;
      return new string[]{$"{target.Name}'s mana has been doubled"};
      }
    },
    new KAction(){ Id = "item_mana_multiplier_2", Name = "Greater Mana Multiplier", GoodForSelf = true,
      Description = "Quadruples mana of target when used.",
      Effect = (root2d, action, user, target) => {
        target.Mana *= 4;
        return new string[]{$"{target.Name}'s mana has been quadrupled"};
      }
    },
    new KAction(){ Id = "item_health_markiplier", Name = "Health Markiplier", GoodForSelf = true,
    Description = "Hello everybody it's Markiplier",
    Effect = (root2d, action, user, target) => {
      return new string[]{$"{target.Name}: Hello everybody it's Markiplier"};
      }
    },
    new KAction(){ Id = "item_gc_coffin", Name = "From Software Gender Change Coffin", GoodForSelf = true,
    Description = "Swaps target's gender",
    Effect = (root2d, action, user, target) => {
      return new string[]{$"{target.Name}'s gender has been changed"};
      }
    },
    new KAction(){ Id = "item_resist_up_1", Name = "Minor Liquid Shield", GoodForSelf = true,
    Description = "Permanently minorly increases resistance",
    Effect = (root2d, action, user, target) => {
      user.Items.Remove(action);
      target.Resistance *= 0.8f;
      return new string[]{$"{target.Name}'s resistances have been increased"};
      }
    },
    new KAction(){ Id = "item_resist_up_2", Name = "Liquid Shield", GoodForSelf = true,
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
    },
    new KAction() { Id = "fx_pos", Name = "Positive Charge" },
    new KAction() { Id = "fx_neg", Name = "Negative Charge" },
    new KAction() { Id = "fx_knife_miss", Name = "Knife Miss" },
    new KAction() { Id = "fx_female_wife", Name = "Female Wife" },
    new KAction() { Id = "fx_tendrils", Name = "Malific Visions",
      PerTurn = (root2d, action, opponent, holder) => {
        int count = holder.Effects.Count(x=>x.Item3.Id == action.Id);
        holder.TrueDmg(count);
        return new string[]{$"Malefic Visions has dealt {count} damage."};
      }
    },
    new KAction() { Id = "fx_health", Name = "Signal Health",
      Effect = (root2d, action, opponent, holder) => {
        int count = new Random().Next(4, 20);
        int dmg = holder.StdDmg(count);
        return new string[]{$"Signal Health has dealt {dmg} damage."};
      }
    },
    new KAction() { Id = "fx_mana", Name = "Signal Mana",
      Effect = (root2d, action, opponent, holder) => {
        int count = new Random().Next(69, 420);
        holder.Mana -= count;
        int dmg = holder.StdDmg(count * 2);
        return new string[]{$"Signal Health has burned {count} mana for {dmg} damage."};
      }
    },
    new KAction() { Id = "fx_res", Name = "Signal Resistance",
      Effect = (root2d, action, opponent, holder) => {
        holder.Resistance *= 1.2f;
        return new string[]{$"Signal resistance reduces resistances {holder}'s resistance"};
      }
    },

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

  public int TrueDmg(int damage) {
    this.Health -= damage;
    OnDmgEvent?.Invoke();
    return damage;
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