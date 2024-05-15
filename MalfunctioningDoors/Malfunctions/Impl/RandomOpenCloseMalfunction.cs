using System.Collections;
using BepInEx.Configuration;
using GameNetcodeStuff;
using MalfunctioningDoors.Patches;
using UnityEngine;
using Random = System.Random;

namespace MalfunctioningDoors.Malfunctions.Impl;

[Malfunction(150)]
public class RandomOpenCloseMalfunction : MalfunctionalDoor {
    private static int _malfunctionChance = 40;
    private Random _syncedRandom = null!;
    private bool _waiting;

    private void Start() {
        doorLock = GetComponent<DoorLock>();
        _syncedRandom = DoorLockPatch.syncedRandom;
    }

    private void Update() {
        if (_waiting)
            return;

        StartCoroutine(StartWaiting());

        var chance = _syncedRandom.Next(0, 100);

        if (chance >= _malfunctionChance)
            return;

        doorLock.OpenOrCloseDoor(StartOfRound.Instance.allPlayerScripts[0]);
    }

    public static int OverrideWeight(ConfigFile configFile) =>
        configFile.Bind("5. Random Open Close Malfunction", "1. Malfunction Weight", 150,
                        "Defines the weight of a malfunction. The higher, the more likely it is to appear").Value;

    public new static void InitializeConfig(ConfigFile configFile) =>
        _malfunctionChance = configFile.Bind("5. Random Open Close Malfunction", "2. Malfunction Chance", 40,
                                             "Defines the chance, if a malfunction is executed").Value;

    public override void TouchInteract(PlayerControllerB playerControllerB) {
    }

    public override void UseInteract(PlayerControllerB playerControllerB) {
    }

    public override void UseKey() {
    }

    public override bool ShouldExecute() => false;

    private IEnumerator StartWaiting() {
        _waiting = true;

        yield return new WaitForSeconds(_syncedRandom.Next(5, 30));

        _waiting = false;
    }
}