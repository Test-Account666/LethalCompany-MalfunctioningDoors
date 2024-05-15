using System;
using BepInEx.Configuration;
using HarmonyLib;
using MalfunctioningDoors.Functional;
using MalfunctioningDoors.Malfunctions;
using Random = System.Random;

namespace MalfunctioningDoors.Patches;

[HarmonyPatch(typeof(DoorLock))]
public static class DoorLockPatch {
    public static Random syncedRandom = new();
    private static int _malfunctioningDoorChance = 30;

    public static void InitializeConfig(ConfigFile configFile) =>
        _malfunctioningDoorChance = configFile.Bind("1. General", "1. Malfunctional Door Chance", 30,
                                                    "Defines the chance that a door can be malfunctional").Value;

    [HarmonyPatch(nameof(DoorLock.Awake))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void AfterAwake(DoorLock __instance) {
        if (syncedRandom.Next(0, 100) >= _malfunctioningDoorChance)
            return;

        __instance.gameObject.AddComponent<DoorLocker>();

        var malfunctionalDoorType = MalfunctionGenerator.GenerateMalfunctionalDoor(syncedRandom);

        AddMalfunction(__instance, malfunctionalDoorType);
    }

    [HarmonyPatch(nameof(DoorLock.UnlockDoorSyncWithServer))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void AfterUnlockDoorSyncWithServer(DoorLock __instance) {
        var malfunctionalDoor = __instance.gameObject.GetComponent<MalfunctionalDoor>();

        if (malfunctionalDoor is null)
            return;

        if (!malfunctionalDoor.ShouldExecute())
            return;

        malfunctionalDoor.UseKey();
    }

    internal static void AddMalfunction(DoorLock doorLock, Type malfunctionalDoorType) {
        if (!malfunctionalDoorType.IsSubclassOf(typeof(MalfunctionalDoor)))
            throw new ArgumentException($"Type '{malfunctionalDoorType.FullName}'");

        var malfunctionalDoor = (MalfunctionalDoor) doorLock.gameObject.AddComponent(malfunctionalDoorType);

        doorLock.doorTrigger.onInteract.AddListener(playerControllerB => {
            if (playerControllerB == null)
                return;

            if (!malfunctionalDoor.ShouldExecute())
                return;

            malfunctionalDoor.UseInteract(playerControllerB);
        });

        var interactTrigger = doorLock.gameObject.AddComponent<InteractTrigger>();

        interactTrigger.touchTrigger = true;

        interactTrigger.interactable = true;

        interactTrigger.onInteract = new();
        interactTrigger.onInteract.AddListener(playerControllerB => {
            if (playerControllerB == null)
                return;

            if (!malfunctionalDoor.ShouldExecute())
                return;

            malfunctionalDoor.TouchInteract(playerControllerB);
        });
    }
}