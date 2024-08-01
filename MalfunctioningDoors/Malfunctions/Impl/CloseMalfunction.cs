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
using System.Collections;
using BepInEx.Configuration;
using DoorBreach.Functional;
using GameNetcodeStuff;
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
        if (doorLock is null) return;

        if (!doorLock.isDoorOpened) return;

        if (doorLock.GetComponent<WaitingForDoorToBeClosed>() is not null) return;

        var chance = _syncedRandom.Next(0, 100);

        if (chance >= _lockChance) return;

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
        if (doorLock is null) return;

        var chance = _syncedRandom.Next(0, 100);

        var doorLocker = doorLock.gameObject.GetComponent<DoorLocker>();

        if (doorLocker is null) {
            MalfunctioningDoors.Logger.LogFatal("No DoorLocker found?!");
            return;
        }

        var open = !doorLock.isDoorOpened;

        if (chance >= _openCloseAfterTwoSecondsChance) {
            doorLocker.SetDoorOpenServerRpc((int) playerControllerB.playerClientId, open);
            return;
        }

        StartCoroutine(DelayedTask(2, () => {
            if (!doorLock.isDoorOpened)
                return;

            doorLocker.SetDoorOpenServerRpc((int) playerControllerB.playerClientId, open);
        }));
    }

    public override void UseKey() {
    }

    public override bool ShouldExecute() =>
        _syncedRandom.Next(0, 100) < _malfunctionChance && !IsDestroyed();

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