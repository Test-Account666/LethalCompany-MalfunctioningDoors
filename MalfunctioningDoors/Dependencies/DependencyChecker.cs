using System.Linq;
using BepInEx.Bootstrap;

namespace MalfunctioningDoors.Dependencies;

internal static class DependencyChecker {
    internal static bool IsLobbyCompatibilityInstalled() =>
        Chainloader.PluginInfos.Values.Any(metadata => metadata.Metadata.GUID.Contains("LobbyCompatibility"));

    internal static bool IsPiggyInstalled() =>
        Chainloader.PluginInfos.Values.Any(metadata => metadata.Metadata.GUID.Contains("Piggy.PiggyVarietyMod"));
}