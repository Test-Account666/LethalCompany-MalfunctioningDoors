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


using BepInEx.Configuration;
using GameNetcodeStuff;
using DoorBreach.Functional;
using MalfunctioningDoors.Functional;
using MalfunctioningDoors.Patches;
using UnityEngine;
using Random = System.Random;

namespace MalfunctioningDoors.Malfunctions.Impl;

[Malfunction(75)]
public class GhostHandMalfunction : MalfunctionalDoor {
    private static int _malfunctionChance = 86;
    private Random _syncedRandom = null!;

    private void Start() {
        doorLock = GetComponent<DoorLock>();
        _syncedRandom = DoorLockPatch.syncedRandom;
    }

    public static int OverrideWeight(ConfigFile configFile) =>
        configFile.Bind("4. Ghost Hand Malfunction", "1. Malfunction Weight", 75,
                        "Defines the weight of a malfunction. The higher, the more likely it is to appear").Value;

    public new static void InitializeConfig(ConfigFile configFile) =>
        _malfunctionChance = configFile.Bind("4. Ghost Hand Malfunction", "2. Malfunction Chance", 86,
                                             "Defines the chance, if a malfunction is executed").Value;

    public override void TouchInteract(PlayerControllerB playerControllerB) {
        if (doorLock is null) return;

        if (doorLock.isDoorOpened) return;

        var doorLocker = doorLock.gameObject.GetComponent<DoorLocker>();

        if (doorLocker is null) {
            MalfunctioningDoors.Logger.LogFatal("No DoorLocker found?!");
            return;
        }

        doorLocker.SetDoorOpenServerRpc((int) playerControllerB.playerClientId, true);

        PlayGhostHandSound();

        CreateGhostHand(playerControllerB);

        playerControllerB.DamagePlayer(10, true, true, CauseOfDeath.Bludgeoning, 1, false, playerControllerB.velocityLastFrame);

        Landmine.SpawnExplosion(doorLock.transform.position, killRange: 0F, damageRange: 0F, physicsForce: 15.0F);
    }

    private static void CreateGhostHand(Component playerControllerB) {
        var playerPosition = playerControllerB.transform.position;

        var position = new Vector3(playerPosition.x, playerPosition.y + 1, playerPosition.z);

        if (Instantiate(MalfunctioningDoors.ghostHandPrefab, position, new(0, 0, 0, 0)) is not GameObject ghostHands) {
            MalfunctioningDoors.Logger.LogFatal("Something went wrong while trying to instantiate the GhostHands!");
            return;
        }

        ghostHands.transform.LookAt(playerControllerB.transform);
        ghostHands.transform.rotation *= Quaternion.Euler(0, 90, 0);

        ghostHands.transform.position = position;

        ghostHands.transform.localScale *= 1.8F;

        var ghostHandRotator = ghostHands.AddComponent<GhostHandRotator>();

        ghostHandRotator.playerControllerTransform = playerControllerB.transform;
    }

    public override void UseInteract(PlayerControllerB playerControllerB) {
    }

    public override void UseKey() {
    }

    public override bool ShouldExecute() => _syncedRandom.Next(0, 100) <= _malfunctionChance && !IsDestroyed();

    private void PlayGhostHandSound() {
        var soundIndex = _syncedRandom.Next(0, MalfunctioningDoors.GhostHandSfxList.Length);

        var ghostHandAudio = MalfunctioningDoors.GhostHandSfxList[soundIndex];

        MalfunctioningDoors.Logger.LogDebug($"Playing clip '{ghostHandAudio.name}' ({soundIndex})");

        var audioObject = new GameObject("TemporaryGhostHandAudio");

        var audioSource = audioObject.AddComponent<AudioSource>();

        audioSource.clip = ghostHandAudio;
        audioSource.volume = 2F;
        audioSource.Play();

        Destroy(audioObject, ghostHandAudio.length);
    }
}