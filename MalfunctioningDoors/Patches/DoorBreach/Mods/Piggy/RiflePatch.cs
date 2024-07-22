using HarmonyLib;
using MalfunctioningDoors.Functional;
using PiggyVarietyMod.Patches;
using UnityEngine;
using Math = System.Math;

namespace MalfunctioningDoors.Patches.DoorBreach.Mods.Piggy;

[HarmonyPatch(typeof(M4Item))]
public static class RiflePatch {
    
    [HarmonyPatch(nameof(M4Item.ShootGun))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void ShredDoors(M4Item __instance, Vector3 gunPosition, Vector3 gunForward) {
        var playerWhoShot = ActionSource.Source.SHOTGUN_ACCIDENT.ToInt();

        if (__instance.isHeld) playerWhoShot = (int) __instance.playerHeldBy.playerClientId;

        if (__instance.isHeldByEnemy) playerWhoShot = ActionSource.Source.SHOTGUN_ENEMY.ToInt();

        var ray = new Ray(gunPosition, gunForward);

        var hitDoor = Physics.Raycast(ray, out var doorLock, 8f, 1 << 9,
                                      QueryTriggerInteraction.Collide);


        if (!hitDoor) return;

        var hasHealth = doorLock.collider.TryGetComponent(out DoorHealth doorHealth);

        if (!hasHealth) return;

        var distance = doorLock.distance;

        const int baseDamage = 2;

        var logFactor = Math.Max(Math.Log(distance + 1, 2), 1);

        var adjustedDamage = (int) (baseDamage / logFactor);

        doorHealth.HitDoorServerRpc(playerWhoShot, adjustedDamage);
    }
}