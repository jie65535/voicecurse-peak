using BepInEx.Configuration;

namespace VoiceCurse;

public class Config {
    // Global
    public ConfigEntry<bool> EnableDebugLogs { get; private set; }
    public ConfigEntry<float> GlobalCooldown { get; private set; }

    // Event: Launch
    public ConfigEntry<bool> LaunchEnabled { get; private set; }
    public ConfigEntry<string> LaunchKeywords { get; private set; }
    public ConfigEntry<float> LaunchStunDuration { get; private set; }
    public ConfigEntry<float> LaunchForceLowerBound { get; private set; }
    public ConfigEntry<float> LaunchForceHigherBound { get; private set; }

    // Event: Affliction
    public ConfigEntry<bool> AfflictionEnabled { get; private set; }
    public ConfigEntry<float> AfflictionMinPercent { get; private set; }
    public ConfigEntry<float> AfflictionMaxPercent { get; private set; }
    public ConfigEntry<bool> AfflictionTemperatureSwapEnabled { get; private set; }
    
    // Affliction Keywords
    public ConfigEntry<string> AfflictionKeywordsInjury { get; private set; }
    public ConfigEntry<string> AfflictionKeywordsHunger { get; private set; }
    public ConfigEntry<string> AfflictionKeywordsCold { get; private set; }
    public ConfigEntry<string> AfflictionKeywordsHot { get; private set; }
    public ConfigEntry<string> AfflictionKeywordsPoison { get; private set; }
    public ConfigEntry<string> AfflictionKeywordsSpores { get; private set; }

    // Event: Transmute
    public ConfigEntry<bool> TransmuteEnabled { get; private set; }
    
    // Individual Transmute Toggles
    public ConfigEntry<bool> TransmuteMilkEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteCactusEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteCoconutEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteAppleEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteBananaEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteEggEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteFruitEnabled { get; private set; }
    public ConfigEntry<bool> TransmuteMushroomEnabled { get; private set; }
    
    // Event: Death
    public ConfigEntry<bool> DeathEnabled { get; private set; }
    public ConfigEntry<string> DeathKeywords { get; private set; }

    // Event: Zombify
    public ConfigEntry<bool> ZombifyEnabled { get; private set; }
    public ConfigEntry<string> ZombifyKeywords { get; private set; }

    // Event: Sleep
    public ConfigEntry<bool> SleepEnabled { get; private set; }
    public ConfigEntry<string> SleepKeywords { get; private set; }
    
    // Event: Drop
    public ConfigEntry<bool> DropEnabled { get; private set; }
    public ConfigEntry<string> DropKeywords { get; private set; }

    // Event: Explode
    public ConfigEntry<bool> ExplodeEnabled { get; private set; }
    public ConfigEntry<string> ExplodeKeywords { get; private set; }
    public ConfigEntry<float> ExplodeRadius { get; private set; }
    public ConfigEntry<float> ExplodeDamage { get; private set; }
    public ConfigEntry<float> ExplodeStunDuration { get; private set; }
    public ConfigEntry<float> ExplodeForceLowerBound { get; private set; }
    public ConfigEntry<float> ExplodeForceHigherBound { get; private set; }
    
    // Event: Slip
    public ConfigEntry<bool> SlipEnabled { get; private set; }
    public ConfigEntry<string> SlipKeywords { get; private set; }
    public ConfigEntry<float> SlipStunDuration { get; private set; }

    public Config(ConfigFile config) {
        // Global
        EnableDebugLogs = config.Bind("Global", "EnableDebugLogs", true, "启用语音识别的调试日志。");
        GlobalCooldown = config.Bind("Global", "GlobalCooldown", 2.0f, "在另一个语音事件可以被触发之前必须经过的时间（以秒为单位）。");

        // Event: Launch
        LaunchEnabled = config.Bind("Event.Launch", "Enabled", true, "启用发射事件。");
        LaunchKeywords = config.Bind("Event.Launch", "Keywords", "launch, fly, boost, ascend, lift, up, cannon, canon, rocket, soar, jump, spring, catapult, fling, hurl, propel, shoot, skyrocket, takeoff, left, right, forward, back, yeet, lob, pitch, chuck, heave, airborne, levitate, hover, elevate, rise, vault, leap, bound, hop, eject, thrust, projectile, missile, space, orbit, 发射, 飞行, 推进, 升空, 上升, 左, 右, 前, 后", "触发发射事件的关键字列表，用逗号分隔。");
        LaunchForceLowerBound = config.Bind("Event.Launch", "ForceLowerBound", 1500f, "发射时施加给玩家的最低力值。");
        LaunchForceHigherBound = config.Bind("Event.Launch", "ForceHigherBound", 3000f, "发射时施加给玩家的最高力值。");
        LaunchStunDuration = config.Bind("Event.Launch", "StunDuration", 3.0f, "发射后玩家被眩晕/物理击倒的持续时间（以秒为单位）。");

        // Event: Affliction
        AfflictionEnabled = config.Bind("Event.Affliction", "Enabled", true, "启用负面状态事件。");
        AfflictionMinPercent = config.Bind("Event.Affliction", "MinPercent", 0.2f, "触发时状态条填充的最小百分比 (0.0 到 1.0)。");
        AfflictionMaxPercent = config.Bind("Event.Affliction", "MaxPercent", 0.6f, "触发时状态条填充的最大百分比 (0.0 到 1.0)。");
        AfflictionTemperatureSwapEnabled = config.Bind("Event.Affliction", "TemperatureSwapEnabled", true, "如果启用，尝试通过说出相反的词来摆脱热或冷状态，只会将现有效果切换到新的，并在顶部叠加，以防止滥用。");

        AfflictionKeywordsInjury = config.Bind("Event.Affliction", "KeywordsInjury", "damage, hurt, injury, injured, pain, harm, wound, hit, bleed, bruise, cut, slash, orange, ache, sore, trauma, gash, scrape, laceration, tear, torn, broken, fracture, sprain, puncture, stab, maim, cripple, batter, 伤害, 受伤, 疼痛, 创伤, 橙色", "触发物理伤害的关键字。");
        AfflictionKeywordsHunger = config.Bind("Event.Affliction", "KeywordsHunger", "hunger, hungry, starving, starve, food, malnourishment, famished, eat, snack, meal, yellow, appetite, crave, craving, ravenous, peckish, feast, feed, sustenance, nourishment, nutrition, consume, 饥饿, 饥渴, 饥肠辘辘, 食物, 黄色, 吃, 饮食, 营养", "触发饥饿状态的关键字。");
        AfflictionKeywordsCold = config.Bind("Event.Affliction", "KeywordsCold", "freezing, cold, blizzard, shiver, ice, frozen, chill, frigid, winter, blue, frost, arctic, polar, glacier, icicle, hypothermia, numb, shivering, freeze, 寒冷, 冰, 冰冻, 冬天, 蓝色, 霜, 冰川, 冰柱, 低温, 冰结", "触发寒冷状态的关键字。");
        AfflictionKeywordsHot = config.Bind("Event.Affliction", "KeywordsHot", "hot, fire, melt, scorching, heat, burn, pyro, flame, summer, cook, hell, red, sizzle, sear, swelter, boil, roast, bake, baking, scald, inferno, blaze, blazing, ignite, combust, incinerate, 热, 火, 熔化, 热量, 燃烧, 火焰, 夏天, 红色, 烤, 烧灼, 烈火", "触发炎热状态的关键字。");
        AfflictionKeywordsPoison = config.Bind("Event.Affliction", "KeywordsPoison", "poison, sick, vomit, toxic, venom, contaminate, purple, nausea, nauseous, intoxicate, pollute, taint, corrupt, disease, ill, ailment, malady, 中毒, 生病, 毒素, 毒液, 污染, 紫色, 病, 疾病", "触发中毒状态的关键字。");
        AfflictionKeywordsSpores = config.Bind("Event.Affliction", "KeywordsSpores", "spore, pink", "触发孢子状态的关键字。");

        // Event: Transmute
        TransmuteEnabled = config.Bind("Event.Transmute", "Enabled", true, "启用转换事件。");

        TransmuteMilkEnabled = config.Bind("Event.Transmute", "EnableMilk", true, "启用 'milk/calcium/奶/钙' -> 奶白金");
        TransmuteCactusEnabled = config.Bind("Event.Transmute", "EnableCactus", true, "启用 'cactus/仙人掌' -> 仙人球");
        TransmuteCoconutEnabled = config.Bind("Event.Transmute", "EnableCoconut", true, "启用 'coconut/椰子' -> 椰子");
        TransmuteAppleEnabled = config.Bind("Event.Transmute", "EnableApple", true, "启用 'apple/berry/苹果/浆果' -> 脆莓");
        TransmuteBananaEnabled = config.Bind("Event.Transmute", "EnableBanana", true, "启用 'banana/香蕉' -> 莓蕉皮");
        TransmuteEggEnabled = config.Bind("Event.Transmute", "EnableEgg", true, "启用 'egg/鸡蛋' -> 煎蛋");
        TransmuteFruitEnabled = config.Bind("Event.Transmute", "EnableFruit", true, "启用 'fruit/水果' -> 随机水果");
        TransmuteMushroomEnabled = config.Bind("Event.Transmute", "EnableMushroom", true, "启用 'mushroom/fungus/蘑菇/真菌' -> 蘑菇");

        // Event: Death
        DeathEnabled = config.Bind("Event.Death", "Enabled", true, "启用死亡事件。");
        DeathKeywords = config.Bind("Event.Death", "Keywords", "die, death, dead, suicide, kill, deceased, skeleton, skull, bone, perish, demise, expire, fatal, mortal, slain, dying, corpse, cadaver, lifeless, cease, extinct, eliminate, terminate, execute, obliterate, annihilate, eradicate, end, finish, doom, grave, burial, coffin, casket, tomb, crypt, reaper, grim, 死, 死亡, 死去, 杀死, 骷髅, 尸体, 棺材, 墓穴", "触发死亡事件的关键字列表，用逗号分隔。");

        // Event: Zombify
        ZombifyEnabled = config.Bind("Event.Zombify", "Enabled", true, "启用僵尸化事件。");
        ZombifyKeywords = config.Bind("Event.Zombify", "Keywords", "zombie, zombify, zombified, walker, ghoul, bitten, bite, brain, rot, decay, infected, infection, plague, pandemic, virus, outbreak, cannibal, flesh, meat, undead, risen, horde, apocalypse, reanimate, lurker, creeper, crawler, groaning, groan, moan, growl, snarl, 僵尸, 脑子, 腐烂, 感染, 病毒, 疫情, 行尸走肉", "触发僵尸化事件的关键字列表，用逗号分隔。");

        // Event: Sleep
        SleepEnabled = config.Bind("Event.Sleep", "Enabled", true, "启用睡眠事件。");
        SleepKeywords = config.Bind("Event.Sleep", "Keywords", "faint, sleep, exhausted, sleepy, tired, bed, nap, rest, slumber, doze, snooze, pass out, knockout, blackout, coma, narc, drowsy, unconscious, collapse, zonk, conk, yawn, fatigue, fatigued, weary, lethargic, sluggish, drained, wiped, beat, worn, spent, shut-eye, shuteye, siesta, catnap, dreamland, nodding off, drift off, lights out, out cold, 睡眠, 疲惫, 疲劳, 休息, 昏倒, 无意识, 打盹, 嗜睡", "触发睡眠事件的关键字列表，用逗号分隔。");

        // Event: Drop
        DropEnabled = config.Bind("Event.Drop", "Enabled", true, "启用掉落事件。");
        DropKeywords = config.Bind("Event.Drop", "Keywords", "drop, oops, whoops, butterfingers, fumble, release, discard, off, loss, lose, let go, slip away, misplace, clumsy, accident, unhand, relinquish, surrender, abandon, ditch, shed, cast, toss, throw away, get rid, 丢掉, 失手, 摔落, 松手, 丢失, 抛弃, 投掷, 丢下", "触发掉落事件的关键字列表，用逗号分隔。");

        // Event: Explode
        ExplodeEnabled = config.Bind("Event.Explode", "Enabled", true, "启用爆炸事件。");
        ExplodeKeywords = config.Bind("Event.Explode", "Keywords", "explosion, dynamite, grenade, explode, blow, blast, boom, nuke, bomb, nuclear, detonate, detonation, explosive, kaboom, burst, 爆炸, 炸药, 手榴弹, 爆破, 炸弹, 核弹, 引爆, 爆裂", "触发爆炸事件的关键字列表，用逗号分隔。");
        ExplodeRadius = config.Bind("Event.Explode", "Radius", 6.0f, "爆炸效果和伤害的半径。");
        ExplodeDamage = config.Bind("Event.Explode", "DamagePercent", 0.4f, "施加给玩家的伤害百分比 (0.0 到 1.0)。");
        ExplodeStunDuration = config.Bind("Event.Explode", "StunDuration", 3.0f, "爆炸后玩家被眩晕/物理击倒的持续时间（以秒为单位）。");
        ExplodeForceLowerBound = config.Bind("Event.Explode", "ForceLowerBound", 2000f, "施加的最低爆炸力值。");
        ExplodeForceHigherBound = config.Bind("Event.Explode", "ForceHigherBound", 3000f, "施加的最高爆炸力值。");

        // Event: Slip
        SlipEnabled = config.Bind("Event.Slip", "Enabled", true, "启用滑倒事件。");
        SlipKeywords = config.Bind("Event.Slip", "Keywords", "fuck, asshole, bastard, bitch, fag, damn, crap, slip, slide, trip, fall, fell, stumble, tumble, topple, stagger, wobble, skid, slick, peel, unbalanced, unstable, tilt, 滑倒, 滑行, 绊倒, 跌倒, 摔倒, 失衡, 不稳, 倾斜", "触发滑倒事件的关键字列表，用逗号分隔。");
        SlipStunDuration = config.Bind("Event.Slip", "StunDuration", 2.0f, "滑倒后玩家被眩晕/物理击倒的持续时间（以秒为单位）。");
    }
}