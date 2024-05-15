using System;
using System.Collections;
using BepInEx.Configuration;
using GameNetcodeStuff;
using MalfunctioningDoors.Functional;
using MalfunctioningDoors.Patches;
using UnityEngine;
using Random = System.Random;

namespace MalfunctioningDoors.Malfunctions.Impl;

[Malfunction(100)]
public class CloseMalfunction : MalfunctionalDoor {
    private static int _lockChance = 30;
    private static int _lockWhenCloseChance = 80;
    private static int _openCloseAfterTwoSecondsChance = 40;
    private static int _malfunctionChance = 20;
    private Random _syncedRandom = null!;

    private void Start() {
        doorLock = GetComponent<DoorLock>();
        _syncedRandom = DoorLockPatch.syncedRandom;
    }

    public static int OverrideWeight(ConfigFile configFile) =>
        configFile.Bind("2. Close Malfunction", "1. Malfunction Weight", 100,
                        "Defines the weight of a malfunction. The higher, the more likely it is to appear").Value;

    public new static void InitializeConfig(ConfigFile configFile) {
        _malfunctionChance = configFile.Bind("2. Close Malfunction", "2. Malfunction Chance", 20,
                                             "Defines the chance, if a malfunction is executed").Value;

        _lockChance = configFile.Bind("2. Close Malfunction", "3. Lock Chance", 30,
                                      "Defines the chance, if a door will be locked").Value;

        _lockWhenCloseChance = configFile.Bind("2. Close Malfunction", "4. Lock When Close Chance", 80,
                                               "Defines the chance, if a door will be locked after closing (The 'Lock Chance' will be rolled first)")
                                         .Value;

        _openCloseAfterTwoSecondsChance = configFile.Bind("2. Close Malfunction", "5. Open Close After Two Seconds Chance", 40,
                                                          "Defines the chance, if a door will open/close after two seconds after being opened/closed"
                                                        + " ('Malfunction Chance' will be rolled first)").Value;
    }

    public override void TouchInteract(PlayerControllerB playerControllerB) {
        if (!doorLock.isDoorOpened)
            return;

        if (doorLock.GetComponent<WaitingForDoorToBeClosed>() is not null)
            return;

        var chance = _syncedRandom.Next(0, 100);

        if (chance >= _lockChance)
            return;

        var chance1 = _syncedRandom.Next(0, 100);

        if (chance1 < _lockWhenCloseChance) {
            var waitingForDoorToBeClosed = doorLock.gameObject.AddComponent<WaitingForDoorToBeClosed>();
            waitingForDoorToBeClosed.StartCoroutine(WaitingForDoorToBeClosed.WaitForDoorToBeClosed(doorLock));
            return;
        }

        MalfunctioningDoors.Logger.LogInfo("Locking door <:)");

        doorLock.LockDoor();

        var doorLocker = doorLock.gameObject.GetComponent<DoorLocker>();

        if (doorLocker is null) {
            MalfunctioningDoors.Logger.LogFatal("No DoorLocker found?!");
            return;
        }

        doorLocker.LockDoorServerRpc();
    }

    public override void UseInteract(PlayerControllerB playerControllerB) {
        var chance = _syncedRandom.Next(0, 100);

        if (chance >= _openCloseAfterTwoSecondsChance)
            doorLock.OpenOrCloseDoor(playerControllerB);
        else
            StartCoroutine(DelayedTask(2, () => {
                if (!doorLock.isDoorOpened)
                    return;

                doorLock.OpenOrCloseDoor(playerControllerB);
            }));
    }

    public override void UseKey() {
    }

    public override bool ShouldExecute() =>
        _syncedRandom.Next(0, 100) < _malfunctionChance;

    private static IEnumerator DelayedTask(float delay, Action action) {
        yield return new WaitForSeconds(delay);
        yield return new WaitForEndOfFrame();

        action.Invoke();
    }
}

internal class WaitingForDoorToBeClosed : MonoBehaviour {
    private static bool _done;

    private void Update() {
        if (!_done)
            return;

        _done = false;
        Destroy(this);
    }

    internal static IEnumerator WaitForDoorToBeClosed(DoorLock doorLock) {
        MalfunctioningDoors.Logger.LogInfo("Waiting for door to be closed :)");

        while (doorLock.isDoorOpened)
            yield return new WaitForSeconds(1);

        yield return new WaitForEndOfFrame();

        MalfunctioningDoors.Logger.LogInfo("Locking door <:)");

        doorLock.LockDoor();

        var doorLocker = doorLock.gameObject.GetComponent<DoorLocker>();

        if (doorLocker is null) {
            MalfunctioningDoors.Logger.LogFatal("No DoorLocker found?!");
            yield break;
        }

        doorLocker.LockDoorServerRpc();
        _done = true;
    }
}