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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using MalfunctioningDoors.Functional;
using MalfunctioningDoors.Patches;
using UnityEngine;
using Random = System.Random;

namespace MalfunctioningDoors.Malfunctions.Impl;

[Malfunction(55)]
public class ExplodeMalfunction : MalfunctionalDoor {
    private static int _malfunctionChance = 35;
    private static Vector3 _position;
    private Action? _spawnExplosionExpression;
    private MethodInfo? _spawnExplosionMethod;
    private Random _syncedRandom = null!;

    private void Start() {
        doorLock = GetComponent<DoorLock>();
        _syncedRandom = DoorLockPatch.syncedRandom;
    }

    public static int OverrideWeight(ConfigFile configFile) =>
        configFile.Bind("4. Explode Malfunction", "1. Malfunction Weight", 55,
                        "Defines the weight of a malfunction. The higher, the more likely it is to appear").Value;

    public new static void InitializeConfig(ConfigFile configFile) =>
        _malfunctionChance = configFile.Bind("4. Explode Malfunction", "2. Malfunction Chance", 35,
                                             "Defines the chance, if a malfunction is executed").Value;

    public override void TouchInteract(PlayerControllerB playerControllerB) {
        if (doorLock is null) return;

        if (doorLock.isDoorOpened)
            return;

        var doorLocker = doorLock.gameObject.GetComponent<DoorLocker>();

        if (doorLocker is null) {
            MalfunctioningDoors.Logger.LogFatal("No DoorLocker found?!");
            return;
        }

        doorLocker.SetDoorOpenServerRpc((int) playerControllerB.playerClientId, true);

        PlayGhostHandSound();

        CreateGhostHand(playerControllerB);

        playerControllerB.DamagePlayer(10, true, true, CauseOfDeath.Bludgeoning, 1, false, playerControllerB.velocityLastFrame);

        if (_spawnExplosionExpression is not null) {
            _spawnExplosionExpression();
            return;
        }

        var fetched = false;

        try {
            fetched = FetchAndExecuteSpawnExplosion([
                doorLock.transform.position, false, 0f, 0f,
            ]);
        } catch {
            //Ignored
        } finally {
            if (!fetched) {
                var doorLockTransform = doorLock.transform;
                var doorPosition = doorLockTransform.position;

                _position = doorPosition;

                fetched = FetchAndExecuteSpawnExplosion([
                    doorPosition, false, 0f, 0f, 50, 15.0f, MalfunctioningDoors.ghostHandPrefab,
                ]);
            }
        }

        if (fetched) return;

        MalfunctioningDoors.Logger.LogFatal("Something went wrong trying to fetch spawnExplosion!");
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

    public override bool ShouldExecute() =>
        _syncedRandom.Next(0, 100) <= _malfunctionChance && !IsDestroyed();

    private bool FetchAndExecuteSpawnExplosion(IReadOnlyCollection<object> parameters) {
        // Get the method info
        _spawnExplosionMethod ??= typeof(Landmine).GetMethod("SpawnExplosion", BindingFlags.Public | BindingFlags.Static);

        // Return false, as we failed to identify the method
        if (_spawnExplosionMethod is null) return false;

        // Return false, as we failed to identify the method
        if (_spawnExplosionMethod.GetParameters().Length != parameters.Count) return false;

        // ReSharper disable once CoVariantArrayConversion
        var parameterExpressions = parameters.Select(o => o is null
                                                         ? (Expression) Expression.Call(
                                                             AccessTools.Method(typeof(ExplodeMalfunction), nameof(CreateExplosionObject)))
                                                         : Expression.Constant(o)).ToArray();

        var callExpression = Expression.Call(_spawnExplosionMethod, parameterExpressions);

        var lambdaExpression = Expression.Lambda<Action>(callExpression);

        // Compile the lambda expression into a delegate
        _spawnExplosionExpression = lambdaExpression.Compile();

        // Invoke the delegate
        _spawnExplosionExpression();
        return true; // Return true, as we successfully identified the method
    }

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

    public static GameObject CreateExplosionObject() =>
        Instantiate(StartOfRound.Instance.explosionPrefab, _position, Quaternion.identity,
                    RoundManager.Instance.mapPropsContainer.transform);
}