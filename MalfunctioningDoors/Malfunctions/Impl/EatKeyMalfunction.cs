using System.Collections;
using BepInEx.Configuration;
using GameNetcodeStuff;
using MalfunctioningDoors.Functional;
using MalfunctioningDoors.Patches;
using UnityEngine;
using Random = System.Random;

namespace MalfunctioningDoors.Malfunctions.Impl;

[Malfunction(60)]
public class EatKeyMalfunction : MalfunctionalDoor {
    private static int _malfunctionChance = 60;
    private Random _syncedRandom = null!;

    private void Start() {
        doorLock = GetComponent<DoorLock>();
        _syncedRandom = DoorLockPatch.syncedRandom;

        StartCoroutine(LockDoorRoutine());
    }

    public static int OverrideWeight(ConfigFile configFile) =>
        configFile.Bind("3. Eat Key Malfunction", "1. Malfunction Weight", 60,
                        "Defines the weight of a malfunction. The higher, the more likely it is to appear").Value;

    public new static void InitializeConfig(ConfigFile configFile) =>
        _malfunctionChance = configFile.Bind("3. Eat Key Malfunction", "2. Malfunction Chance", 60,
                                             "Defines the chance, if a malfunction is executed").Value;

    public override void TouchInteract(PlayerControllerB playerControllerB) {
    }

    public override void UseInteract(PlayerControllerB playerControllerB) {
    }

    public override void UseKey() =>
        StartCoroutine(LockDoorRoutine());

    private IEnumerator LockDoorRoutine() {
        yield return new WaitForEndOfFrame();

        doorLock.LockDoor();

        var doorLocker = doorLock.gameObject.GetComponent<DoorLocker>();

        if (doorLocker is null) {
            MalfunctioningDoors.Logger.LogFatal("No DoorLocker found?!");
            yield break;
        }

        doorLocker.LockDoorServerRpc();
    }

    public override bool ShouldExecute() =>
        _syncedRandom.Next(0, 100) < _malfunctionChance;
}