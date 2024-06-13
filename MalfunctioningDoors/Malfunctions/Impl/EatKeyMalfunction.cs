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

        yield return new WaitUntil(() => doorLock is not null);

        if (doorLock is null) yield break;

        doorLock.LockDoor();

        var doorLocker = doorLock.gameObject.GetComponent<DoorLocker>();

        if (doorLocker is null) {
            MalfunctioningDoors.Logger.LogFatal("No DoorLocker found?!");
            yield break;
        }

        doorLocker.LockDoorServerRpc();
    }

    public override bool ShouldExecute() =>
        _syncedRandom.Next(0, 100) < _malfunctionChance && !IsDestroyed();
}