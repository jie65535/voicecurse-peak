# VoiceCurse

Mod that triggers gameplay events based on what players are saying.
It uses an offline voice recognition library to detect you saying specific words or phrases, which then activate various in-game effects.

## 中文本地化 / Chinese Localization

本模组现已支持中文本地化！玩家可以使用中文语音命令触发游戏事件。

VoiceCurse mod 现在支持中文本地化！玩家可以使用中文语音指令来触发游戏事件。您可以说中文单词或短语来激活各种游戏效果。

中文关键词已添加到默认配置中，与英文关键词并存以确保兼容性。

## Features

The keywords themselves can all be configured to your own preference, although there is a lot of them. First, let's go over all possible events.

| Events     | Keyword Examples                            | 中文关键词示例                                | Effect                                                                                          |
|------------|---------------------------------------------|----------------------------------------------|-------------------------------------------------------------------------------------------------|
| Affliction | "hot", "cold", "ill", "sick"                | "热", "火", "寒冷", "冷", "病", "毒", "发烧", "难受" | Gives you the affliction of the related keyword.                                                  |
| Death      | "die", "end", "bones", "skeleton"           | "死", "死亡", "尸体", "骷髅", "骨头", "安息", "归西" | Kills you instantly.                                                                              |
| Drop       | "oops", "release", "off", "lose"            | "丢", "掉", "失手", "松手", "丢失", "放手", "掉落" | Drops all your items, including the items in your backpack.                                       |
| Explode    | "blow", "explode", "blast", "nuke"          | "爆炸", "爆破", "炸药", "炸弹", "引爆", "爆裂" | Makes you explode instantly, this causes damage to your surroundings too.                         |
| Launch     | "up", "left", "right", "cannon"             | "发射", "飞行", "上升", "左", "右", "前", "后", "上天", "弹射", "飞" | Launches you in a specified direction if provided, otherwise randomly.                            |
| Sleep      | "rest", "tired", "exhausted", "nap"         | "睡", "休息", "疲劳", "昏倒", "打盹", "困" | Makes you pass out instantly.                                                                     |
| Slip       | "crap", "damn",  "trip", "fall"             | "滑倒", "绊倒", "跌倒", "不稳", "摔倒", "甲沟炎" | Causes you to trip like stepping on a banana.                                                     |
| Transmute  | "milk", "fruit", "apple", "banana"          | "奶", "钙", "水果", "苹果", "香蕉", "蘑菇", "转化", "变化" | Causes you and your inventory items to transform into related objects, this kills you instantly.  |
| Zombify    | "rot", "zombie", "ghoul", "bite"            | "僵尸", "腐烂", "感染", "病毒", "丧尸", "咬" | Turns you into a zombie.                                                                          |
| Sacrifice  | "trade", "sacrifice", "revive", "resurrect" | "献祭", "交易", "复活", "重生", "牺牲", "救赎" | 立即杀死你，但会在死亡位置完全复活最近的玩家。有冷却时间。 |
| Blind      | "blind", "flash", "eyes", "can't see"        | "致盲", "闪光", "眼睛", "看不见", "失明", "眼花" | 像在Alpine中被某个危险击中一样暂时使你失明。                            |

> ### Note
> Note that the events are triggered when it detects the keyword *anywhere* in the spoken sentence.
> This means that saying words like "gro**up**" will trigger the "up" launch event because "up" is contained within "group".
> This applies to every keyword and is not functionality that can be disabled.
>
> This means that saying safe words like "n**ice**" can trigger the cold affliction event because "ice" is a keyword. There are many such examples and this is the central challenge of using this mod.

## Configuration

All keywords can be configured (except of Transmute, which operates a bit differently), all events can be enabled or disabled, and certain aspects of events can be customized.

You can modify the following settings for each event:

* **Enabled:** Toggle specific events on or off.
* **Keywords:** Custom comma-separated lists of trigger words for every event (now includes both English and Chinese keywords by default).
* **Forces & Durations:** Adjust launch forces, explosion radius, stun durations, and damage percentages.
* **Specific Toggles:** Enable or disable specific transmutation rules or mechanics.

The configuration applies only to yourself and what you want to happen to you when you say the keywords.

## 中文配置 / Chinese Configuration

配置文件已包含中英双语关键词，以支持中文语音识别。您可以在 BepInEx 配置文件中修改或添加中文关键词以自定义您的体验。

## Installation

1.  Ensure BepInEx is installed.
2.  Download the latest release of VoiceCurse.
3.  Extract the contents into your game's `BepInEx/plugins` folder.
4.  Run the game once to generate the configuration file.

## Everyone needs to have the mod installed for it to work properly!

## 中文语音支持 / Chinese Voice Support

安装后，您可以直接使用中文语音命令。首次运行游戏后，配置文件将包含中英双语关键词，您可以根据需要进行调整。