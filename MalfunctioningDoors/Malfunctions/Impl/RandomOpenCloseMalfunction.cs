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
using DoorBreach;
using GameNetcodeStuff;
using DoorBreach.Functional;
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
        if (_waiting) return;

        StartCoroutine(StartWaiting());

        if (doorLock is null || !doorLock) return;

        var chance = _syncedRandom.Next(0, 100);

        if (chance >= _malfunctionChance) return;

        var doorLocker = doorLock.gameObject.GetComponent<DoorLocker>();

        if (doorLocker is null) {
            MalfunctioningDoors.Logger.LogFatal("No DoorLocker found?!");
            return;
        }

        var open = !doorLock.isDoorOpened;

        doorLocker.SetDoorOpenServerRpc(ActionSource.Source.MALFUNCTION.ToInt(), open);
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