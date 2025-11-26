# VoiceCurse

Mod that triggers gameplay events based on voice input. 
It uses an offline voice recognition library to detect you saying specific words or phrases, which then activate various in-game effects.

## Features

The keywords themselves can all be configured to your own preference, although there is a lot of them. First, let's go over all possible events.

| Events     | Keyword Examples                    | Effect                                                                               |
|------------|-------------------------------------|--------------------------------------------------------------------------------------|
| Affliction | "hot", "cold", "ill", "sick"        | Gives you the affliction of the related keyword.                                     |
| Death      | "die", "end", "bones", "skeleton"   | Kills you instantly.                                                                 |
| Drop       | "oops", "release", "off", "lose"    | Drops all your items, including the items in your backpack.                          |
| Explode    | "blow", "explode", "blast", "blast" | Makes you explode instantly, this causes damage to your surroundings too.            |
| Launch     | "up", "down", "right", "cannon"     | Launches you in a specified direction if provided, otherwise randomly.               |
| Sleep      | "rest", "tired", "exhausted", "nap" | Makes you pass out instantly.                                                        |
| Slip       | "crap", "damn",  "trip", "fall"     | Causes you to trip like stepping on a banana.                                        |
| Transmute  | "milk", "fruit", "apple", "banana"  | Causes you and your items to tranform into related objects, this kills you instantly |
| Zombify    | "rot", "zombie", "ghoul", "bite"    | Turns you into a zombie.                                                             |

> [!NOTE]
> Note that the events are triggered when it detects the keyword *anywhere*.
> This means that saying words like "gro**up**" will trigger the "up" launch event because "up" is contained within "group".
> This applies to every keyword and is not functionality that can be disabled.

> [!TIP]
> This means that saying safe words like "n**ice**" can trigger the cold affliction event because "ice" is a keyword. There are many such examples and this is the central challenge of using this mod.
