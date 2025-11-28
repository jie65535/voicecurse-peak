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
        LaunchKeywords = config.Bind("Event.Launch", "Keywords", "发射,飞,升空,火箭,弹射,推进,弹跳,喷射,弹起,左,右,前,后,上,下,冲天,蹦极,弹簧,投掷,抛射,加速,猛冲,升天,弹射器,导弹,太空,轨道", "触发发射事件的关键字列表，用逗号分隔。");
        LaunchForceLowerBound = config.Bind("Event.Launch", "ForceLowerBound", 1500f, "发射时施加给玩家的最低力值。");
        LaunchForceHigherBound = config.Bind("Event.Launch", "ForceHigherBound", 3000f, "发射时施加给玩家的最高力值。");
        LaunchStunDuration = config.Bind("Event.Launch", "StunDuration", 3.0f, "发射后玩家被眩晕/物理击倒的持续时间（以秒为单位）。");

        // Event: Affliction
        AfflictionEnabled = config.Bind("Event.Affliction", "Enabled", true, "启用负面状态事件。");
        AfflictionMinPercent = config.Bind("Event.Affliction", "MinPercent", 0.2f, "触发时状态条填充的最小百分比 (0.0 到 1.0)。");
        AfflictionMaxPercent = config.Bind("Event.Affliction", "MaxPercent", 0.6f, "触发时状态条填充的最大百分比 (0.0 到 1.0)。");
        AfflictionTemperatureSwapEnabled = config.Bind("Event.Affliction", "TemperatureSwapEnabled", true, "如果启用，尝试通过说出相反的词来摆脱热或冷状态，只会将现有效果切换到新的，并在顶部叠加，以防止滥用。");

        AfflictionKeywordsInjury = config.Bind("Event.Affliction", "KeywordsInjury", "受伤,伤害,疼痛,创伤,流血,骨折,扭伤,割伤,淤青,伤口,刺痛,酸痛,痛楚,内伤,外伤,重伤,轻伤,伤痛,痛感,橙色", "触发物理伤害的关键字。");
        AfflictionKeywordsHunger = config.Bind("Event.Affliction", "KeywordsHunger", "饿,饥渴,吃饭,食物,进食,食欲,想吃,黄色,营养,进食,用餐,零食,大餐,喂食", "触发饥饿状态的关键字。");
        AfflictionKeywordsCold = config.Bind("Event.Affliction", "KeywordsCold", "寒冷,冷,冰冻,冰凉,结冰,霜冻,发抖,哆嗦,打颤,寒颤,刺骨,降温,冰雪,冰川,冰柱,低温,冻僵,蓝色,北极,极地", "触发寒冷状态的关键字。");
        AfflictionKeywordsHot = config.Bind("Event.Affliction", "KeywordsHot", "热,火烧,火烤,烫,火焰,夏天,红色,地狱,灼烧,沸腾,烘烤,炙烤,烈火,燃烧,点燃", "触发炎热状态的关键字。");
        AfflictionKeywordsPoison = config.Bind("Event.Affliction", "KeywordsPoison", "中毒,毒药,毒素,毒液,恶心,想吐,呕吐,头晕,晕眩,不适,难受,污染,感染,疾病,生病,病症,病毒,紫色,病态,疾患", "触发中毒状态的关键字。");
        AfflictionKeywordsSpores = config.Bind("Event.Affliction", "KeywordsSpores", "孢子,真菌,霉菌,蘑菇,菌类,粉红,粉色,发霉,霉变", "触发孢子状态的关键字。");

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
        DeathEnabled = config.Bind("Event.Death", "Enabled", false, "启用死亡事件。");
        DeathKeywords = config.Bind("Event.Death", "Keywords", "死亡,去世,毙命,自杀,杀死,骷髅,骨头,尸体,棺材,坟墓,安息,永别,完蛋,嗝屁,牺牲,阵亡,挂了,断气,升天,投胎,终结,末日,死神,坟墓,墓穴,葬送,毁灭,消除,终结", "触发死亡事件的关键字列表，用逗号分隔。");

        // Event: Zombify
        ZombifyEnabled = config.Bind("Event.Zombify", "Enabled", true, "启用僵尸化事件。");
        ZombifyKeywords = config.Bind("Event.Zombify", "Keywords", "僵尸,丧尸,感染,病毒,瘟疫,行尸,咬,脑子,腐烂,变异,不死,末日,生化,尸变,感染者,丧尸化,僵尸化,行尸走肉,活死人,亡灵,丧尸病毒,爆发,食人,血肉", "触发僵尸化事件的关键字列表，用逗号分隔。");

        // Event: Sleep
        SleepEnabled = config.Bind("Event.Sleep", "Enabled", true, "启用睡眠事件。");
        SleepKeywords = config.Bind("Event.Sleep", "Keywords", "睡觉,睡眠,疲惫,疲倦,打盹,小睡,休息,昏迷,昏倒,做梦,床,晚安,睡意,困倦,打瞌睡,昏睡,沉睡,熟睡,打哈欠,眼皮重,想睡,睡一觉,眯一会,躺平,打瞌睡,午睡,打盹儿", "触发睡眠事件的关键字列表，用逗号分隔。");

        // Event: Drop
        DropEnabled = config.Bind("Event.Drop", "Enabled", true, "启用掉落事件。");
        DropKeywords = config.Bind("Event.Drop", "Keywords", "掉落,丢下,扔掉,失手,滑落,放手,放弃,丢失,手滑,没拿住,掉了,落下了,忘拿了,不见了,失踪,抛弃,丢弃,脱手,撒手,失物,遗失", "触发掉落事件的关键字列表，用逗号分隔。");

        // Event: Explode
        ExplodeEnabled = config.Bind("Event.Explode", "Enabled", true, "启用爆炸事件。");
        ExplodeKeywords = config.Bind("Event.Explode", "Keywords", "爆炸,引爆,炸弹,炸药,爆破,爆裂,核弹,手榴弹,地雷,自爆,炸裂,炸飞,boom,爆破,引爆,爆炸物,炸开,炸成灰,炸成碎片", "触发爆炸事件的关键字列表，用逗号分隔。");
        ExplodeRadius = config.Bind("Event.Explode", "Radius", 6.0f, "爆炸效果和伤害的半径。");
        ExplodeDamage = config.Bind("Event.Explode", "DamagePercent", 0.4f, "施加给玩家的伤害百分比 (0.0 到 1.0)。");
        ExplodeStunDuration = config.Bind("Event.Explode", "StunDuration", 3.0f, "爆炸后玩家被眩晕/物理击倒的持续时间（以秒为单位）。");
        ExplodeForceLowerBound = config.Bind("Event.Explode", "ForceLowerBound", 2000f, "施加的最低爆炸力值。");
        ExplodeForceHigherBound = config.Bind("Event.Explode", "ForceHigherBound", 3000f, "施加的最高爆炸力值。");

        // Event: Slip
        SlipEnabled = config.Bind("Event.Slip", "Enabled", true, "启用滑倒事件。");
        SlipKeywords = config.Bind("Event.Slip", "Keywords", "甲沟炎,滑倒,摔倒,绊倒,跌倒,摔跤,失足,溜滑,扑街,摔个狗吃屎,四脚朝天,人仰马翻,失衡,不稳,倾斜,打滑,滑行,绊脚,跌跤", "触发滑倒事件的关键字列表，用逗号分隔。");
        SlipStunDuration = config.Bind("Event.Slip", "StunDuration", 2.0f, "滑倒后玩家被眩晕/物理击倒的持续时间（以秒为单位）。");
    }
}