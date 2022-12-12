using System;
using System.Collections.Generic;
using Godot;
using static Newtonsoft.Json.JsonConvert;

static class TextEditExtension {
  public static void CursorToEnd(this TextEdit textEdit) {
    textEdit.CursorSetLine(Int32.MaxValue);
    textEdit.CursorSetColumn(Int32.MaxValue);
  }
}

static class ListExtension {
  private static Random rng = new Random();

  public static void Shuffle<T>(this IList<T> list) {
    int n = list.Count;
    while (n > 1) {
      n--;
      int k = rng.Next(n + 1);
      T value = list[k];
      list[k] = list[n];
      list[n] = value;
    }
  }
}

static class GDUtil {
  public static string[] CENSORLMAO = new string[] { "69", "666", "1cup", "2girls", "2girls1cup", "4r5e", "5h1t", "abortion", "ahole", "aids", "anal", "anal sex", "analsex", "angrydragon", "angrydragons", "angrypenguin", "angrypenguins", "angrypirate", "angrypirates", "anus", "apeshit", "ar5e", "arrse", "arse", "arsehole", "artard", "askhole", "ass", "ass 2 ass", "ass hole", "ass kisser", "ass licker", "ass lover", "ass man", "ass master", "ass pirate", "ass rapage", "ass rape", "ass raper", "ass to ass", "ass wipe", "assbag", "assbandit", "assbanger", "assberger", "assburger", "assclown", "asscock", "asses", "assface", "assfuck", "assfucker", "assfukker", "asshat", "asshead", "asshole", "asshopper", "assjacker", "asslicker", "assmunch", "asswhole", "asswipe", "aunt flo", "b000bs", "b00bs", "b17ch", "b1tch", "bag", "ballbag", "ballsack", "bampot", "bang", "bastard", "basterd", "bastich", "bean count", "beaner", "beastial", "beastiality", "beat it", "beat off", "beaver", "beavers", "beeyotch", "betch", "beyotch", "bfe", "bi sexual", "bi sexuals", "biatch", "bigmuffpi", "biotch", "bisexual", "bisexuality", "bisexuals", "bitch", "bitched", "bitches", "bitchin", "bitching", "bizatch", "blackie", "blackies", "block", "bloody hell", "blow", "blow job", "blow wad", "blowjob", "boff", "boffing", "boffs", "boink", "boinking", "boinks", "boiolas", "bollick", "bollock", "bondage", "boner", "boners", "bong", "boob", "boobies", "boobs", "booty", "boy2boy", "boy4boy", "boyforboy", "boyonboy", "boys2boys", "boys4boys", "boysforboys", "boysonboys", "boytoboy", "brothel", "brothels", "brotherfucker", "buceta", "bugger", "bugger ", "buggered", "buggery", "bukake", "bullshit", "bumblefuck", "bumfuck", "bung", "bunghole", "bush", "bushpig", "but", "but plug", "butplug", "butsecks", "butsekks", "butseks", "butsex", "butt", "buttfuck", "buttfucka", "buttfucker", "butthole", "buttmuch", "buttmunch", "buttplug", "buttsecks", "buttsekks", "buttseks", "buttsex", "buttweed", "c0ck", "c0cksucker", "cabron", "camel toe", "camel toes", "cameltoe", "canabis", "cannabis", "carpet muncher", "castrate", "castrates", "castration", "cawk", "chank", "cheesedick", "chick2chick", "chick4chick", "chickforchick", "chickonchick", "chicks2chicks", "chicks4chicks", "chicksforchicks", "chicksonchicks", "chickstochicks", "chicktochick", "chinc", "chink", "chinks", "choad", "choads", "chode", "cipa", "circlejerk", "circlejerks", "cl1t", "cleavelandsteemer", "cleveland", "clevelandsteamer", "clevelandsteemer", "clit", "clitoris", "clitoris     ", "clits", "clusterfuck", "cock", "cock block", "cock suck", "cockblock", "cockface", "cockfucker", "cockfucklutated", "cockhead", "cockmaster", "cockmunch", "cockmuncher", "cockpenis", "cockring", "cocks", "cocksuck", "cocksucker", "cocksuka", "cocksukka", "cok", "cokmuncher", "coksucka", "comestain", "condom", "condoms", "coochie", "coon", "coons", "cooter", "copulated", "copulates", "copulating", "copulation", "corn", "corn_hole", "cornhole", "cornholes", "cr4p", "crap", "crapping", "craps", "cream", "creampie", "crotch", "crotches", "cum", "cumming", "cums", "cumshot", "cumstain", "cumtart", "cunnilingus", "cunt", "cuntbag", "cunthole", "cuntilingis", "cuntilingus", "cunts", "cuntulingis", "cuntulingus", "d1ck", "dabitch", "dago", "dammit", "damn", "damned", "dance", "darkie", "darkies", "darky", "deep", "deepthroat", "defecate", "defecates", "defecating", "defecation", "deggo", "diaf", "diarea", "diarhea", "diarrhea", "dick", "dickhead", "dickhole", "dickring", "dicks", "dicksucker", "dicksuckers", "dicksucking", "dicksucks", "dickwad", "dickweed", "dickwod", "dik", "dike", "dikes", "dildo", "dildoe", "dildoes", "dildos", "dilligaf", "dingleberry", "dipshit", "dirsa", "dlck", "dog", "doggin", "doggystyle", "dogshit", "domination", "dominatrix", "donkey", "donkeyribber", "dook", "doosh", "dork", "dorks", "douche", "douchebag", "douchebags", "douchejob", "douchejobs", "douches", "douchewaffle", "duche", "dumass", "dumb", "dumb fuck", "dumbass", "dumbfuc", "dumbfuck", "dumbshit", "dumdfuk", "dumfuck", "dumshit", "dyke", "dykes", "ead", "eat me", "ejaculat", "ejaculate", "ejaculated", "ejaculates", "ejaculation", "ejakulat", "ejakulate", "enema", "enemas", "enima", "enimas", "epeen", "epenis", "erect", "erection", "erekshun", "erotic", "eroticism", "f0x0r", "f0xx0r", "fack", "facker", "facking", "fag", "fagbag", "faggit", "faggitt", "faggot", "faggots", "faghag", "fags", "fagtard", "fannyflaps", "fannyfucker", "fart", "farting", "farts", "fatass", "fck", "fcker", "fckers", "fcking", "fcks", "fcuk", "fcuker", "fcuking", "feck", "fecker", "felch", "felched", "felcher", "felches", "felching", "fellate", "fellatio", "feltch", "feltched", "feltcher", "feltches", "feltching", "fetish", "fetishes", "finger", "fingering", "fisting", "flog", "flogging", "flogs", "fook", "fooker", "foreskin", "forked", "fornicate", "fornicates", "fornicating", "fornication", "frigging", "frottage", "fubar", "fuck", "fucka", "fuckas", "fuckass", "fucked", "fuckedup", "fucker", "fuckers", "fuckface", "fuckfaces", "fuckhead", "fuckhole", "fuckhouse", "fuckin", "fucking", "fuckingshitmotherfucker", "fucknugget", "fucks", "fucktard", "fuckwad", "fuckwhit", "fuckwit", "fuckyou", "fucndork", "fudgepacker", "fudgepacking", "fugly", "fuk", "fuken", "fuker", "fukker", "fukkin", "fukwhit", "fukwit", "fuq", "fuqed", "fuqing", "fuqs", "fux", "fux0r", "fuxx0r", "gang bang", "gangbang", "gangbanger", "gangbangers", "gangbanging", "gangbangs", "ganja", "gay", "gaydar", "gays", "gaytard", "gaywad", "genital", "genitalia", "genitals", "gerbiling", "ghey", "girl2girl", "girl4girl", "girlforgirl", "girlongirl", "girls2girls", "girls4girls", "girlsforgirls", "girlsongirls", "girlstogirls", "girltogirl", "gloryhole", "goatse", "gobshit", "gobshite", "gobtheknob", "goddam", "goddammit", "goddamn", "goddamned", "goddamnit", "gooch", "gook", "gooks", "groe", "gspot", "gtfo", "gubb", "gummer", "guppy", "guy2guy", "guy4guy", "guyforguy", "guyonguy", "guys2guys", "guys4guys", "guysforguys", "guysonguys", "guystoguys", "guytoguy", "gyfs", "hair pie", "hairpie", "hairpies", "hairy bush", "hand", "handjob", "harbl", "hard on", "hardon", "hardons", "hell", "hentai", "heroin", "herpes", "herps", "hick", "hiv", "ho", "hoare", "hoe", "hoebag", "hoer", "hoes", "hole", "homo", "homos", "homosexual", "homosexuality", "homosexuals", "honkee", "honkey", "honkie", "honkies", "honky", "hoochie", "hooker", "hookers", "hore", "horney", "hornie", "horny", "horseshit", "hosebeast", "hot", "hotcarl", "hotkarl", "hummer", "hump", "humping", "humps", "hymen", "i like ass", "i love ass", "i love tit", "i love tits", "iluvsnatch", "incest", "ip freely", "itard", "jack", "jack off", "jackass", "jackingoff", "jackoff", "jap", "japs", "jerk off", "jerkoff", "jesusfreak", "jewbag", "jewboy", "jiga", "jigaboo", "jigga", "jiggaboo", "jis", "jism", "jiz", "jizm", "jizz", "job", "junglebunny", "jysm", "kawk", "khunt", "kike", "kikes", "kinky", "kkk", "klit", "knob", "knobjocky", "knobjokey", "knockers", "kooch", "koolie", "koolielicker", "koolies", "kootch", "kukluxklan", "kunt", "kyke", "l3itch", "labia", "lap", "lapdance", "lelo", "lemonparty", "lesbian", "lesbians", "lesbifriends", "lesbo", "lesbos", "leyed", "lez", "lezzie", "lichercunt", "lick ass", "lick myass", "lick tit", "lickbeaver", "lickcarpet", "lickdick", "lickherass", "lickherpie", "lickmy ass", "lickpussy", "like ass", "like tit", "limpdick", "lingerie", "llello", "lleyed", "loltard", "love ass", "love juice", "love tit", "lovehole", "lsd", "lucifer", "lumpkin", "m0f0", "m0fo", "m45terbate", "ma5terb8", "ma5terbate", "mack", "mammaries", "man2man", "man4man", "mandingo", "manforman", "mangina", "manonman", "mantoman", "marijuana", "masochism", "masochist", "master", "masterb8", "masterbat3", "masterbate", "masterbates", "masterbating", "masterbation", "masterbations", "masturbate", "masturbates", "masturbating", "masturbation", "meatcurtain", "men2men", "men4men", "menformen", "menonmen", "mentomen", "milf", "minge", "mistress", "mof0", "mofo", "motha", "motherfuck", "motherfucka", "motherfuckas", "motherfucked", "motherfucker", "motherfuckers", "motherfucking", "motherfuckka", "muff", "muff diver", "muffdiver", "muffdiving", "munch", "mung", "mutha", "muther", "muzza", "my ass", "n1gga", "n1gger", "naked", "nambla", "nards", "nazi", "nazies", "nazis", "necrophile", "necrophiles", "necrophilia", "necrophiliac", "negro", "neonazi", "nice ass", "nig", "nigg3r", "nigg4h", "nigga", "niggah", "niggas", "niggaz", "nigger", "niggers", "nigglet", "niglet", "nippies", "nipple", "nips", "nobhead", "nobjockey", "nobjocky", "nobjokey", "nookey", "nookie", "noshit", "nude", "nudes", "nudity", "numbnuts", "nut bag", "nut lick", "nut lover", "nut sack", "nut suck", "nutnyamouth", "nutnyomouth", "nuts", "nutsack", "nutstains", "nymph", "nympho", "nymphomania", "nymphomaniac", "nymphomaniacs", "nymphos", "oral", "orgasm", "orgasmic", "orgasms", "orgi", "orgiastic", "orgies", "orgy", "paedophile", "panooch", "panties", "panty", "patootie", "pecker", "peckerhead", "peckers", "pedophile", "pedophiles", "pedophilia", "peepshow", "peepshows", "pen15", "penii", "penis", "penises", "penisfucker", "perve", "perversion", "pervert", "perverted", "perverts", "phat", "phile", "philes", "philia", "phuck", "phucker", "phuckers", "phucking", "phucks", "phuk", "phuker", "phukers", "phuking", "phuks", "phuq", "phuqer", "phuqers", "phuqing", "phuqs", "pie", "pigfucker", "pimpis", "piss", "pissant", "pissed", "pisser", "pisses", "pissfart", "pissflaps", "pissing", "pocket", "poke", "poon", "poon eater", "poon tang", "poonani", "poonanny", "poonany", "poonj", "poonjab", "poonjabie", "poonjaby", "poontang", "poop", "poopchute", "poot", "porchmonkey", "porking", "porks", "porn", "porno", "prick", "pricks", "prostitot", "pube", "pubes", "pubic", "pud", "pudd", "puds", "punani", "punanni", "punanny", "punta", "puntang", "pusse", "pussi", "pussies", "pussy", "puta", "puto", "qeef", "qfmft", "qq more", "quafe", "quap", "quatch", "queef", "queefe", "queefed", "queefing", "queer", "queerbait", "queermo", "queers", "queev", "quefe", "queif", "quief", "quif", "quiff", "quim", "quim nuts", "qweef", "racial", "racism", "racist", "racists", "rape", "raped", "raper", "raping", "rapist", "redtide", "reefer", "renob", "retard", "ricockulous", "rim job", "rimjaw", "rimjob", "rimjobs", "rimming", "roofie", "rtard", "rtfm", "rubbers", "rump", "rumpranger", "rumprider", "rumps", "rustytrombone", "sadism", "sadist", "sadomasochism", "sand nigger", "sandnigga", "sandniggas", "sandnigger", "sandniggers", "sapphic", "sappho", "sapphos", "satan", "scatological", "scheiss", "scheisse", "schlong", "schlonging", "schlongs", "schtup", "schtupp", "schtupping", "schtups", "screw", "screw me", "screw this", "screw you", "screwed", "screwer", "screwing", "screws", "screwyou", "scroat", "scrog", "scrote", "scrotum", "secks", "sekks", "seks", "semen", "sex", "sexed", "sexking", "sexkitten", "sexmachine", "sexqueen", "sexual", "sexuality", "sexy", "sexybitch", "sexybitches", "sh1t", "shag", "shagger", "shaggin", "shagging", "shart", "shemale", "shit", "shitdick", "shite", "shited", "shitey", "shitface", "shitfaced", "shitfuck", "shithead", "shitlist", "shits", "shitt", "shitted", "shitter", "shittiest", "shitting", "shitts", "shitty", "shiznits", "shotacon", "shotakon", "shyte", "sickass", "sixtynine", "sixtynining", "skank", "skeet", "sketell", "sko", "skrew", "skrewing", "skrews", "slant eye", "slanteyes", "slattern", "slave", "slaves", "slopehead", "slopeheads", "slut", "slutbag", "slutpuppy", "sluts", "slutty", "slutwhore", "smegma", "smut", "snatch", "snatches", "soddom", "sodom", "sodomist", "sodomists", "sodomize", "sodomized", "sodomizing", "sodomy", "sonnofabitch", "sonnovabitch", "sonnuvabitch", "sonofabitch", "spank", "spanked", "spanking", "spanks", "spearchucker", "spearchuckers", "sperm", "spermicidal", "spermjuice", "sphincter", "spic", "spick", "spicks", "spics", "spik", "spiks", "spooge", "stank ho", "stankpuss", "steamer", "stfu", "stiffie", "stiffy", "stud", "studs", "submissive", "submissives", "suck", "suck ass", "sucks ass", "swinger", "swingers", "t1tt1e5", "t1tties", "take a dump", "takeadump", "tar baby", "tarbaby", "tard", "teabaggin", "teabagging", "teen2teen", "teen4teen", "teenforteen", "teenonteen", "teens2teens", "teens4teens", "teensforteens", "teensonteens", "teenstoteens", "teentoteen", "teets", "teez", "testes", "testical", "testicals", "testicle", "testicles", "threesome", "throat", "thundercunt", "tiddie", "tiddy", "tit", "titandass", "titbabe", "titball", "titfuck", "tits", "titsandass", "titt", "tittie", "tittie5", "tittiefucker", "titties", "titts", "titty", "tittyfuck", "tittywank", "titwank", "toke", "toss salad", "tramp", "tramps", "tranny", "transexual", "transexuals", "transvestite", "transvestites", "tubgirl", "turd", "tw4t", "twat", "twathead", "twatlips", "twats", "twatty", "twunt", "twunter", "ufia", "umfriend", "underwear", "up yours", "upyours", "upyourz", "urinate", "urinated", "urinates", "urinating", "urination", "urine", "vaffanculo", "vag", "vagina", "vaginal", "vaginas", "vaginer", "vagitarian", "vagoo", "vagy", "vajayjay", "viagra", "vibrator", "virgin", "virginity", "virgins", "voyeur", "voyeurism", "voyeurs", "vulva", "w00se", "wackedoff", "wackoff", "wad blower", "wad lover", "wad sucker", "wadblower", "wadlover", "wadsucker", "waffle", "wang", "wank", "wanker", "wanky", "wetback", "wetbacks", "wetdream", "wetdreams", "whackedoff", "whackoff", "whigger", "whip", "whipped", "whipping", "whips", "whiz", "whoe", "whore", "whored", "whorehouse", "whores", "whoring", "wigger", "willies", "wog", "wogged", "woggy", "wogs", "woman2woman", "woman4woman", "womanforwoman", "womanonwoman", "womantowoman", "women2women", "women4women", "womenforwomen", "womenonwomen", "womentowomen", "woof", "wop", "xx", "xxx", "yank", "yayo", "yeat", "yeet", "yeyo", "yiff", "yiffy", "yola", "yols", "yoni", "youaregay", "yourgay", "zipperhead", "zipperheads", "zorch" };

  public static SignalAwaiter Wait(this Node node, float duration) {
    return node.ToSignal(node.GetTree().CreateTimer(duration), "timeout");
  }

  public static SignalAwaiter NextFrame(this Node node) {
    return node.ToSignal(node.GetTree(), "idle_frame");
  }

  public static string SAVELOCATION = "user://godot.dll";

  public static GameSave GameSave { get; set; } = Load();

  public static void SavePlayer(Killable player) {
    player.SyncIds();
    GameSave.MaxHealth = player.MaxHealth;
    GameSave.Health = player.Health;
    GameSave.MaxMana = player.MaxMana;
    GameSave.Mana = player.Mana;
    GameSave.Resistance = player.Resistance;
    GameSave.AttackIds = player.AttackIds;
    GameSave.ItemIds = player.ItemIds;
    GameSave.PlayerSaved = true;
  }

  public static Killable LoadPlayer() {
    var player = new Killable();
    player.Name = GameSave.Name;
    player.MaxHealth = GameSave.MaxHealth;
    player.Health = GameSave.Health;
    player.MaxMana = GameSave.MaxMana;
    player.Mana = GameSave.Mana;
    player.Resistance = GameSave.Resistance;
    player.AttackIds = GameSave.AttackIds;
    player.ItemIds = GameSave.ItemIds;
    player.SyncActions();
    return player;
  }

  public static GameSave Load() {
    File f = new File();
    if (f.FileExists(SAVELOCATION)) {
      try {
        f.Open(SAVELOCATION, File.ModeFlags.Read);
        GameSave save = DeserializeObject<GameSave>(f.GetAsText());
        f.Close();
        return save;
      } catch {
        return new GameSave();
      }
    } else {
      return new GameSave();
    }
  }

  public static void Save() {
    File f = new File();
    if (f.FileExists(SAVELOCATION)) {
      Directory d = new Directory();
      d.Remove(SAVELOCATION);
    }
    f.Open(SAVELOCATION, File.ModeFlags.Write);
    f.StoreString(SerializeObject(GameSave));
    f.Close();
  }
}