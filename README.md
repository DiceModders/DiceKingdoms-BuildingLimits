# DiceKingdoms Building Limits

Lobby rules for Dice Kingdoms: the host sets how many buildings of each kind one island may have.

## The limits

- One number per kind of building: every building that can be built (not the castle), in the build bar's order. The list is the game's own
  building list, so buildings from a game update and buildings other mods add (the [Casino](https://github.com/DiceModders/DiceKingdoms-Casino))
  are in it too. An upgraded building (the upgraded Guard Tower) counts with its base building. Walls are counted in pieces.
- **0 = no limit**, the default, unless the mod that adds a building suggests another default (the Casino: **1**). The host can change every one.
- A building at its limit is greyed out in the build bar, and its tooltip shows how many the island has of how many it may have. Walls: a drag
  that would go over the limit is red and is not placed.
- Computer players keep to the limits too.
- Only standing buildings count: a building that burnt down or was destroyed frees its place; ruins do not count.

## Lobby rules

The numbers are in the *Building limits* settings window (**Settings > Lobby > Building limits: settings**). The titles are the game's own
building names in your language. The host edits them; everybody else sees the host's values.

There is no switch: while every number is at its default nothing changes. As soon as the host changes one, every player in the match needs this
mod (ModCore compares the mods in the lobby).

## Install

Needs BepInEx 6 (IL2CPP), [ModCore](https://github.com/DiceModders/DiceKingdoms-ModCore) 1.3.0 or newer and, to set the limits in the lobby,
[ModBridge](https://github.com/DiceModders/DiceKingdoms-ModBridge) 1.1.0 or newer (with [UiKit](https://github.com/DiceModders/UiKit)). Put
`DiceModders.BuildingLimits.dll` into `BepInEx\plugins`, with the game closed.

## Build

```
.\scripts\build.ps1 -GameDir "<your Dice-Kingdoms folder>"   # first time
.\scripts\build.ps1                                           # afterwards
```

The script builds the mod and copies it into the game's `BepInEx\plugins` folder (close the game first).

## How it works

- This mod only holds the numbers; [ModCore](https://github.com/DiceModders/DiceKingdoms-ModCore) does the checking
  (`ModCore.Buildings.SetLimits`). So a limit a building mod suggests (the Casino: 1) applies even without this mod.
- The limit is part of the game's own check when a building is placed (`BuildData.CanBuildType`). Every computer runs that check at the same
  step, with the buildings placed earlier in the same build phase already counted, so all of them agree.
- Walls are placed ten at a time and the game checks the type once per group, so a group of walls that would go over the limit is refused when
  it is planned.
- The build bar greys out a building at its limit; computer players skip it.
- The list of buildings is read from the game's data when it is loaded. A mod that adds a building suggests its default with
  `ModCore.Buildings.SuggestLimit(type, n)` in its plugin's `Load`.

## Texts

`texts.json` holds the texts in English, German, French, Russian, Ukrainian and Chinese. A `DiceModders.BuildingLimits.texts.json` next to the
DLL wins line by line.

## License

MIT
