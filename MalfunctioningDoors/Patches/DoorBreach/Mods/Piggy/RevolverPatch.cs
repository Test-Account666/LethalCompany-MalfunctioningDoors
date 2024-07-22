using System;
using HarmonyLib;
using MalfunctioningDoors.Functional;
using PiggyVarietyMod.Patches;
using UnityEngine;
using Random = System.Random;

namespace MalfunctioningDoors.Patches.DoorBreach.Mods.Piggy;

[HarmonyPatch(typeof(RevolverItem))]
public static class RevolverPatch {
    private static readonly Random _Random = new();

    [HarmonyPatch(nameof(RevolverItem.ShootGun))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void ShredDoors(RevolverItem __instance, Vector3 revolverPosition, Vector3 revolverForward) {
        var playerWhoShot = ActionSource.Source.SHOTGUN_ACCIDENT.ToInt();

        if (__instance.isHeld) playerWhoShot = (int) __instance.playerHeldBy.playerClientId;

        if (__instance.isHeldByEnemy) playerWhoShot = ActionSource.Source.SHOTGUN_ENEMY.ToInt();

        var ray = new Ray(revolverPosition, revolverForward);

        var hitDoor = Physics.Raycast(ray, out var doorLock, 8f, 1 << 9,
                                      QueryTriggerInteraction.Collide);


        if (!hitDoor) return;

        var hasHealth = doorLock.collider.TryGetComponent(out DoorHealth doorHealth);

        if (!hasHealth) return;

        var distance = doorLock.distance;

        const int baseDamage = 6;

        var adjustedDamage = 666;

        var instantBreak = distance <= 3? _Random.Next(0, (int) (3 - distance)) : 0;

        if (instantBreak <= 0) {
            var logFactor = Math.Max(Math.Log(distance + 1, 2), 1);

            adjustedDamage = (int) (baseDamage / logFactor);
        }

        doorHealth.HitDoorServerRpc(playerWhoShot, adjustedDamage);
    }
}