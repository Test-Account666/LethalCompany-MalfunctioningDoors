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
using static DoorBreach.DoorBreach;
using Random = System.Random;

namespace MalfunctioningDoors.Malfunctions.Impl;

[Malfunction(80)]
public class NoYouMalfunction : MalfunctionalDoor {
    private static int _malfunctionChance = 35;
    private static Random SyncedRandom => DoorLockPatch.SyncedRandom;

    private void Start() => doorLock = GetComponent<DoorLock>();

    public static int OverrideWeight(ConfigFile configFile) =>
        configFile.Bind("6. No You", "1. Malfunction Weight", 80, "Defines the weight of a malfunction. The higher, the more likely it is to appear").Value;

    public new static void InitializeConfig(ConfigFile configFile) =>
        _malfunctionChance = configFile.Bind("6. No You", "2. Malfunction Chance", 35, "Defines the chance, if a malfunction is executed").Value;

    public override void TouchInteract(PlayerControllerB playerControllerB) {
    }

    public override void UseInteract(PlayerControllerB playerControllerB) {
        if (doorLock is null || !doorLock) return;

        var direction = 1;

        if (SyncedRandom.Next(0, 2) > 0) direction = -direction;

        StartCoroutine(StartRotation(playerControllerB, direction));

        playerControllerB.JumpToFearLevel(1F);

        var doorLocker = doorLock.gameObject.GetComponent<DoorLocker>();

        if (doorLocker is null) {
            MalfunctioningDoors.Logger.LogFatal("No DoorLocker found?!");
            return;
        }

        DoorNetworkManager.SetDoorOpenServerRpc(doorLock.NetworkObject, (int) playerControllerB.playerClientId, !doorLock.isDoorOpened);
    }

    private static IEnumerator StartRotation(PlayerControllerB playerControllerB, float direction) {
        var terminate = 1f;
        var startRotation = playerControllerB.transform.rotation;
        var targetRotationY = startRotation.eulerAngles.y + 90 * direction; // Target rotation Y axis

        // Define acceleration and deceleration rates
        const float accelerationRate = 10f; // Adjust as needed
        const float decelerationRate = 5f; // Adjust as needed

        var currentAngle = startRotation.eulerAngles.y;
        var rotationSpeed = 0f; // Initial speed set to 0

        while (Quaternion.Angle(startRotation, playerControllerB.transform.rotation) < 90) {
            terminate -= Time.deltaTime;

            if (terminate <= 0) yield break;

            switch (rotationSpeed) {
                // Calculate new rotation speed based on acceleration and deceleration
                case < accelerationRate:
                    rotationSpeed += Time.deltaTime * accelerationRate;
                    break;
                case > decelerationRate:
                    rotationSpeed -= Time.deltaTime * decelerationRate;
                    break;
            }

            // Apply rotation speed to current angle
            currentAngle += rotationSpeed * Time.deltaTime * 240f * direction;

            // Clamp current angle to ensure it doesn't exceed the target
            currentAngle = direction > 0
                ? Mathf.Clamp(currentAngle, startRotation.eulerAngles.y, targetRotationY)
                : Mathf.Clamp(currentAngle, targetRotationY, startRotation.eulerAngles.y);

            playerControllerB.TeleportPlayer(playerControllerB.transform.position, true, Quaternion.Euler(0, currentAngle, 0).eulerAngles.y);

            yield return new WaitForEndOfFrame();
        }
    }


    public override void UseKey() {
    }

    public override bool ShouldExecute() => SyncedRandom!.Next(0, 100) <= _malfunctionChance && !IsDestroyed();
}