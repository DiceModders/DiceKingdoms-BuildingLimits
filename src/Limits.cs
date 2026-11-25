using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using DiceKingdoms.Game;
using DiceModders.ModCore;
using UnityEngine;

namespace DiceModders.BuildingLimits;

using Core = DiceModders.ModCore.ModCore;

/// <summary>
/// The lobby rules. Every building that can be built (it has a cost; the castle has none) gets a number "most per island" under the heading
/// "Building limits", 0 = no limit. The list is the game's own building list, read when the game's data is loaded, so buildings of a game update
/// and buildings other mods add (the Casino) are in it too. A building's default is the limit the mod that adds it suggests
/// (<c>ModCore.Buildings.SuggestLimit</c>; the Casino: 1), else 0; the host can change every one. There is no switch: while every number is at its
/// default nothing changes (the suggested limits apply with or without this mod), and as soon as one differs every player needs this mod.
/// An upgraded building (the upgraded Guard Tower) counts with its base building. ModCore does the checking; this mod only gives it the numbers.
/// </summary>
internal static class Limits
{
    private sealed class Kind
    {
        public string Key;
        public LobbyRule<int> Rule;
    }

    private static LobbyGroup _heading;
    private static BuildingTypes _types;
    private static readonly List<Kind> Kinds = new();
    private static readonly Dictionary<int, Kind> ByType = new();   // base BuildingTypeName -> kind

    internal static void Install()
    {
        _heading = Core.Lobby.Groups.Get("limits", "Building limits");
        Core.Buildings.SetLimits("Building limits", LimitOf);

        // the building list: when the game's data is loaded, and again when a lobby changes (a mod that adds its building late)
        Core.Civilizations.GameDataLoaded += Scan;
        Core.Lobby.Changed += Scan;
        Core.Texts.LanguageChanged += GameNames;
        Scan();
    }

    /// <summary>The host's number for a base building in this match; null (the suggested limit) when this mod is switched off or has no rule for it.</summary>
    private static int? LimitOf(BuildingTypeName type)
    {
        if (!Plugin.On || !ByType.TryGetValue((int)type, out var kind)) return null;
        return Math.Max(0, kind.Rule.Value);
    }

    // ------------------------------------------------------------------ the building list

    /// <summary>Makes a rule for every buildable building of the game's list that has none yet (only new ones: cheap when nothing changed).</summary>
    private static void Scan()
    {
        try
        {
            if (Core.Lobby.Rules.Fixed) return;   // never add rules during a match
            if (_types == null) foreach (var t in Resources.FindObjectsOfTypeAll<BuildingTypes>()) { _types = t; break; }
            var list = _types != null ? _types.Types : null;
            if (list == null || list.Length == 0) return;
            int before = Kinds.Count;
            foreach (var type in list)   // the game's own order, the build bar's order
            {
                if (type == null || !type.IsBuildable) continue;
                if (Core.Buildings.BaseOf(type.name) != type.name) continue;   // an upgraded building counts with its base building
                if (!ByType.ContainsKey((int)type.name)) Add(type);
            }
            if (Kinds.Count == before) return;
            Plugin.L.LogInfo($"building limits: {Kinds.Count} kinds of building ({Kinds.Count - before} new)");
            GameNames();
        }
        catch (Exception e) { Plugin.Once("scan", "building limits (building list): " + e); }
    }

    private static void Add(BuildingType type)
    {
        string name = type.name.ToString();   // FARM, GUARD_TOWER ...; a mod's building: its number (the Casino: 24)
        bool pieces = type.bulkSize > 1;        // walls: placed and counted in pieces
        int step = pieces ? type.bulkSize : 1, max = 50 * step;
        int suggested = Math.Clamp(Core.Buildings.SuggestedLimit(type.name), 0, max);
        var kind = new Kind { Key = name };
        kind.Rule = Core.Lobby.Rules.Number("limits." + name, Pretty(name), suggested, 0, max, step, pieces ? "Counted in pieces." : null);
        kind.Rule.InGroup(_heading);
        Kinds.Add(kind);
        ByType[(int)type.name] = kind;
    }

    /// <summary>"GUARD_TOWER" -> "Guard Tower"; a number -> "Building 24" (only until the game's own name is known).</summary>
    private static string Pretty(string name)
    {
        if (name.Length > 0 && char.IsDigit(name[0])) return "Building " + name;
        var sb = new StringBuilder();
        foreach (var word in name.Split('_', StringSplitOptions.RemoveEmptyEntries))
        {
            if (sb.Length > 0) sb.Append(' ');
            sb.Append(char.ToUpperInvariant(word[0])).Append(word.Substring(1).ToLowerInvariant());
        }
        return sb.ToString();
    }

    /// <summary>The rules' titles are the game's own building names in the game's language (laid over this mod's texts).</summary>
    private static void GameNames()
    {
        try
        {
            string language = Core.Texts.Language;
            var texts = new Dictionary<string, Dictionary<string, string>>();
            foreach (var k in Kinds)
            {
                string name = Core.Texts.Game("building." + k.Key + ".name", "");
                if (!string.IsNullOrWhiteSpace(name)) texts["rule.limits." + k.Key + ".title"] = new Dictionary<string, string> { [language] = name };
            }
            if (texts.Count > 0) Tx.Set?.Merge(TextSet.Parse(JsonSerializer.Serialize(texts)));
        }
        catch (Exception e) { Plugin.Once("names", "building limits (building names): " + e.Message); }
    }
}
