using System;
using com.github.zehsteam.ToilHead.MonoBehaviours;
using HarmonyLib;
using MalfunctioningDoors.Functional;
using UnityEngine;

namespace MalfunctioningDoors.Patches.DoorBreach.Mods.ToilHead;

[HarmonyPatch(typeof(ToilHeadTurretBehaviour))]
public class ToilHeadTurretPatch {
    [HarmonyPatch(nameof(ToilHeadTurretBehaviour.TurretModeLogic))]
    [HarmonyPrefix]
    // ReSharper disable once InconsistentNaming
    private static void FireAwayThosePeskyDoors(ToilHeadTurretBehaviour __instance) {
        if (__instance.turretMode is not TurretMode.Firing and not TurretMode.Berserk) return;

        if (__instance is {
                _enteringBerserkMode: true, _berserkTimer: > 0,
            }) return;

        if (__instance._turretInterval < __instance._damageRate) return;

        var shootRay = new Ray(__instance.aimPoint.position, __instance.aimPoint.forward);
        var hitDoor = Physics.Raycast(shootRay, out var doorLock, 23f, 1 << 9, QueryTriggerInteraction.Collide);

        if (!hitDoor) return;

        var hasHealth = doorLock.collider.TryGetComponent(out DoorHealth doorHealth);

        if (!hasHealth) return;

        var distance = doorLock.distance;

        const int baseDamage = 9;

        var logFactor = Math.Max(Math.Log(distance + 1, 5), 1);

        var adjustedDamage = (int) (baseDamage / logFactor);

        doorHealth.HitDoorServerRpc(ActionSource.Source.TOIL_HEAD.ToInt(), adjustedDamage);
    }
}