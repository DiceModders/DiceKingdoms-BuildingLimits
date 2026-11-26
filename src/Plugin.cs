using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using DiceModders.ModCore.Tools;

namespace DiceModders.BuildingLimits;

using Core = DiceModders.ModCore.ModCore;   // inside DiceModders.* the bare name would be the namespace

/// <summary>
/// DiceKingdoms Building limits: lobby rules with which the host sets how many buildings of each kind one island may have (0 = no limit).
/// Every building of the game's list is in it, also those other mods add (the Casino, 1 by default). ModCore does the checking.
/// </summary>
[BepInPlugin(Guid, Name, Version)]
[BepInDependency("DiceModders.ModCore")]
[BepInProcess("Dice-Kingdoms.exe")]
public sealed class Plugin : BasePlugin
{
    public const string Guid = "org.dicemodders.dicekingdoms.buildinglimits";
    public const string Name = "DiceKingdoms Building Limits";
    public const string Version = "1.0.1";

    internal static ManualLogSource L;
    /// <summary>The mod's own switch ([General] Enabled).</summary>
    internal static bool On;

    private static readonly HashSet<string> Logged = new();

    public override void Load()
    {
        Tx.Load();
        L = Log;
        Limits.Install();
        Core.Mods.Switch(Config, affectsSimulation: true, description: "Turns the Building limits rules of this mod on or off.")
            .WhenOn(() => On = true, () => On = false);
    }

    /// <summary>Logs a problem once per key.</summary>
    internal static void Once(string key, string message)
    {
        if (Logged.Add(key)) L?.LogError(message);
    }
}
