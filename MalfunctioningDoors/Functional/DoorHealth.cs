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


using Unity.Netcode;
using UnityEngine;

namespace MalfunctioningDoors.Functional;

public class DoorHealth : NetworkBehaviour {
    private bool _broken;
    private DoorLock _doorLock = null!;
    private int _health = 10;

    private void Awake() => SetHealthClientRpc(10);

    private void Update() {
        if (_health > 0 || _broken) return;

        BreakDoorClientRpc();
    }

    internal void SetDoorLock(DoorLock doorLock) => _doorLock = doorLock;

    [ClientRpc]
    public void SetHealthClientRpc(int health) => _health = health;

    [ClientRpc]
    public void BreakDoorClientRpc() {
        if (!_doorLock.isDoorOpened) _doorLock.OpenDoorAsEnemyServerRpc();

        var boxColliders = _doorLock.GetComponents<BoxCollider>();

        foreach (var boxCollider in boxColliders)
            boxCollider.enabled = false;

        _doorLock.doorTrigger.interactable = false;
        _doorLock.doorTrigger.enabled = false;
        _doorLock.enabled = false;

        _broken = true;
    }

    public int GetHealth() => _health;
}