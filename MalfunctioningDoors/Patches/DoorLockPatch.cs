/*
    A Lethal Company Mod
    Copyright (C) 2024  TestAccount666 (Entity303 / Test-Account666)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/


using System;
using BepInEx.Configuration;
using HarmonyLib;
using MalfunctioningDoors.Functional;
using MalfunctioningDoors.Malfunctions;
using MalfunctioningDoors.Malfunctions.Impl;
using Random = System.Random;

namespace MalfunctioningDoors.Patches;

[HarmonyPatch(typeof(DoorLock))]
public static class DoorLockPatch {
    public static Random syncedRandom = new();
    private static int _malfunctioningDoorChance = 30;

    public static void InitializeConfig(ConfigFile configFile) {
        _malfunctioningDoorChance = configFile.Bind("1. General", "1. Malfunctional Door Chance", 30,
                                                    "Defines the chance that a door can be malfunctional").Value;
    }

    [HarmonyPatch(nameof(DoorLock.Awake))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void AfterAwake(DoorLock __instance) {
        var gameObject = __instance.gameObject;
        var doorLocker = gameObject.AddComponent<DoorLocker>();

        if (DoorBreachConfig.doorBreachEnabled) {
            var doorHealth = gameObject.AddComponent<DoorHealth>();

            doorHealth.SetDoorLock(__instance);
            doorHealth.SetDoorLocker(doorLocker);
        }

        var malfunctionalDoorType = typeof(DormantMalfunction);

        if (syncedRandom.Next(0, 100) < _malfunctioningDoorChance)
            malfunctionalDoorType = MalfunctionGenerator.GenerateMalfunctionalDoor(syncedRandom);

        AddMalfunction(__instance, malfunctionalDoorType);
    }

    [HarmonyPatch(nameof(DoorLock.UnlockDoorSyncWithServer))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void AfterUnlockDoorSyncWithServer(DoorLock __instance) {
        var malfunctionalDoor = __instance.gameObject.GetComponent<MalfunctionalDoor>();

        if (malfunctionalDoor is null) return;

        if (!malfunctionalDoor.ShouldExecute()) return;

        malfunctionalDoor.UseKey();
    }

    internal static void AddMalfunction(DoorLock? doorLock, Type malfunctionalDoorType) {
        if (doorLock is null) return;

        if (!malfunctionalDoorType.IsSubclassOf(typeof(MalfunctionalDoor)))
            throw new ArgumentException($"Type '{malfunctionalDoorType.FullName}'");

        var malfunctionalDoor = (MalfunctionalDoor) doorLock.gameObject.AddComponent(malfunctionalDoorType);

        doorLock.doorTrigger.onInteract.AddListener(playerControllerB => {
            if (playerControllerB is null) return;

            if (!malfunctionalDoor.ShouldExecute()) return;

            malfunctionalDoor.UseInteract(playerControllerB);
        });

        var interactTrigger = doorLock.gameObject.AddComponent<InteractTrigger>();

        interactTrigger.touchTrigger = true;

        interactTrigger.interactable = true;

        interactTrigger.onInteract = new();
        interactTrigger.onInteract.AddListener(playerControllerB => {
            if (playerControllerB is null) return;

            if (!malfunctionalDoor.ShouldExecute()) return;

            malfunctionalDoor.TouchInteract(playerControllerB);
        });
    }
}