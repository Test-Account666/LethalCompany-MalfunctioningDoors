using HarmonyLib;

namespace MalfunctioningDoors.Patches;

[HarmonyPatch(typeof(RoundManager))]
public static class RoundManagerPatch {
    [HarmonyPatch("GenerateNewLevelClientRpc")]
    [HarmonyPrefix]
    public static void BeforeGenerateNewLevelClientRpc(int randomSeed) =>
        DoorLockPatch.syncedRandom = new(randomSeed);
}