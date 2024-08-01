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
using DoorBreach.Functional;
using MalfunctioningDoors.Patches;
using UnityEngine;
using Random = System.Random;

namespace MalfunctioningDoors.Malfunctions.Impl;

[Malfunction(55)]
public class NoYouMalfunction : MalfunctionalDoor {
    private static int _malfunctionChance = 35;
    private Random _syncedRandom = null!;

    private void Start() {
        doorLock = GetComponent<DoorLock>();
        _syncedRandom = DoorLockPatch.syncedRandom;
    }

    public static int OverrideWeight(ConfigFile configFile) =>
        configFile.Bind("6. No You", "1. Malfunction Weight", 55,
                        "Defines the weight of a malfunction. The higher, the more likely it is to appear").Value;

    public new static void InitializeConfig(ConfigFile configFile) =>
        _malfunctionChance = configFile.Bind("6. No You", "2. Malfunction Chance", 35,
                                             "Defines the chance, if a malfunction is executed").Value;

    public override void TouchInteract(PlayerControllerB playerControllerB) {
    }

    public override void UseInteract(PlayerControllerB playerControllerB) {
        if (doorLock is null) return;

        var direction = 1;

        if (_syncedRandom.Next(0, 2) > 0) direction = -direction;

        StartCoroutine(StartRotation(playerControllerB, direction));

        playerControllerB.JumpToFearLevel(1F);

        var doorLocker = doorLock.gameObject.GetComponent<DoorLocker>();

        if (doorLocker is null) {
            MalfunctioningDoors.Logger.LogFatal("No DoorLocker found?!");
            return;
        }

        doorLocker.SetDoorOpenServerRpc((int) playerControllerB.playerClientId, !doorLock.isDoorOpened);
    }

    private static IEnumerator StartRotation(PlayerControllerB playerControllerB, float direction) {
        var terminate = 1F;

        var startRotation = playerControllerB.transform.rotation;

        var playerRotationY = startRotation.eulerAngles.y;

        while (Quaternion.Angle(startRotation, playerControllerB.transform.rotation) < 90) {
            terminate -= Time.deltaTime;

            if (terminate <= 0) yield break;

            playerControllerB.TeleportPlayer(playerControllerB.transform.position, true,
                                             playerRotationY + Time.deltaTime * 240F * direction);

            playerRotationY = playerControllerB.transform.rotation.eulerAngles.y;

            yield return new WaitForEndOfFrame();
        }
    }

    public override void UseKey() {
    }

    public override bool ShouldExecute() => _syncedRandom.Next(0, 100) <= _malfunctionChance && !IsDestroyed();
}