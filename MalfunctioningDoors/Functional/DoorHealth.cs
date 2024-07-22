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
using MalfunctioningDoors.Malfunctions;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MalfunctioningDoors.Functional;

public class DoorHealth : NetworkBehaviour {
    private bool _broken;
    private bool _hittable = true;
    private DoorLock _doorLock = null!;
    private DoorLocker _doorLocker = null!;
    private int _health = 8;

    private void Awake() {
        var minimumHealth = DoorBreachConfig.minimumDoorHealth;
        var maximumHealth = minimumHealth + DoorBreachConfig.possibleAdditionalHealth;

        _health = Random.RandomRangeInt(minimumHealth, maximumHealth + 1);
    }

    public bool IsBroken() => _broken;
    public bool IsDoorOpen() => _doorLock.isDoorOpened;

    private void Update() {
        if (!_broken) return;

        if (_doorLock.isDoorOpened) return;

        _doorLocker.SetDoorOpenServerRpc(ActionSource.Source.UNKNOWN.ToInt(), true);
    }

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();

        RequestHealthServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestHealthServerRpc() {
        SetHealthClientRpc(_health);
    }

    internal void SetDoorLock(DoorLock doorLock) => _doorLock = doorLock;

    internal void SetDoorLocker(DoorLocker doorLocker) => _doorLocker = doorLocker;

    [ServerRpc(RequireOwnership = false)]
    public void HitDoorServerRpc(int playerWhoHit, int damage) {
        if (damage == 0) return;

        var actionSource = playerWhoHit.FromInt();

        if (actionSource is null) return;

        if (!DoorBreachConfig.AllowedDoorBreachSources.Contains(actionSource.Value)) return;

        if (!_hittable) return;

        _hittable = false;

        StartCoroutine(ResetHittable());

        MalfunctioningDoors.Logger.LogDebug("Broken: " + _broken);

        MalfunctioningDoors.Logger.LogDebug("Current health: " + _health);
        MalfunctioningDoors.Logger.LogDebug("Damage dealt: " + damage);

        MalfunctioningDoors.Logger.LogDebug("Source: " + actionSource.Value);

        if (_broken) return;

        SetHealthClientRpc(_health - damage);

        if (_health > 0) return;

        BreakDoorClientRpc(playerWhoHit);
    }

    private IEnumerator ResetHittable() {
        yield return new WaitForSeconds(.05F);
        _hittable = true;
    }

    [ClientRpc]
    public void SetHealthClientRpc(int health) => _health = health;

    [ClientRpc]
    public void BreakDoorClientRpc(int playerWhoTriggered) {
        PlayAudio(gameObject);

        var malfunctions = gameObject.GetComponents<MalfunctionalDoor>();

        malfunctions ??= [
        ];

        foreach (var malfunctionalDoor in malfunctions) {
            if (malfunctionalDoor is null) continue;

            Destroy(malfunctionalDoor);
        }

        _broken = true;

        if (DoorBreachConfig.doorBreachMode == DoorBreachConfig.DoorBreachMode.DESTROY) {
            var doorLockTransform = _doorLock.transform;
            Landmine.SpawnExplosion(doorLockTransform.position, true);
            Destroy(doorLockTransform.parent.gameObject);
            return;
        }

        _doorLocker.SetDoorOpenServerRpc(playerWhoTriggered, true);

        _doorLock.doorTrigger.interactable = false;
        _doorLock.doorTrigger.enabled = false;

        _doorLock.doorTrigger.holdTip = "";
        _doorLock.doorTrigger.disabledHoverTip = "";

        _doorLock.doorTrigger.hoverIcon = null;
        _doorLock.doorTrigger.disabledHoverIcon = null;
    }

    public int GetHealth() => _health;

    private static void PlayAudio(GameObject gameObject) {
        if (MalfunctioningDoors.doorBreakSfx is null) return;

        var audioSource = gameObject.AddComponent<AudioSource>();

        // Set spatial blend to 1 for full 3D sound
        audioSource.spatialBlend = 1.0f;

        // Set max distance to 100 units
        audioSource.maxDistance = 30.0f;

        // Set rolloff mode to Logarithmic
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;

        audioSource.clip = MalfunctioningDoors.doorBreakSfx;
        audioSource.volume = 1F;
        audioSource.Play();

        Destroy(audioSource, MalfunctioningDoors.doorBreakSfx.length);
    }
}