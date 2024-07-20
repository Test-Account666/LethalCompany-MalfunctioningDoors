using System;
using HarmonyLib;
using MalfunctioningDoors.Functional;
using UnityEngine;

namespace MalfunctioningDoors.Patches.DoorBreach;

[HarmonyPatch(typeof(Landmine))]
public class LandminePatch {
    [HarmonyPatch(nameof(Landmine.Detonate))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    private static void DisintegrateDoors(Landmine __instance) {
        var position = __instance.transform.position;

        var results = new Collider[12];

        var size = Physics.OverlapSphereNonAlloc(position, 6F, results, 1 << 9, QueryTriggerInteraction.Collide);

        if (size <= 0) return;

        for (var index = 0; index < size; index++) {
            var collider = results[index];

            var hasHealth = collider.TryGetComponent(out DoorHealth doorHealth);

            if (!hasHealth) continue;

            var distance = Vector3.Distance(position, collider.transform.position);

            const int baseDamage = 11;

            var adjustedDamage = baseDamage;

            if (distance <= 3.6f) {
                adjustedDamage = 666;
            } else {
                var logFactor = Math.Max(Math.Log(distance + 1, 4), 1);

                adjustedDamage = (int) (baseDamage / logFactor);
            }

            doorHealth.HitDoorServerRpc(ActionSource.Source.LANDMINE.ToInt(), adjustedDamage);
        }
    }
}