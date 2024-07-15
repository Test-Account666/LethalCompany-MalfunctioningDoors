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

namespace MalfunctioningDoors.Functional;

public class DoorLocker : NetworkBehaviour {
    private DoorLock _doorLock = null!;

    private void Awake() => _doorLock = GetComponent<DoorLock>();

    [ServerRpc(RequireOwnership = false)]
    public void LockDoorServerRpc() {
        LockDoorClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDoorOpenServerRpc(int playerWhoTriggered, bool open) {
        SetDoorOpenClientRpc(playerWhoTriggered, open);
    }

    [ClientRpc]
    private void LockDoorClientRpc() {
        _doorLock.LockDoor();
    }

    [ClientRpc]
    private void SetDoorOpenClientRpc(int playerWhoTriggered, bool open) {
        if (_doorLock.isDoorOpened == open) {
            MalfunctioningDoors.Logger.LogFatal($"Door state already matching state {open}!");
            return;
        }

        var allPlayerScripts = StartOfRound.Instance.allPlayerScripts;

        if (playerWhoTriggered < 0) return;

        if (playerWhoTriggered >= allPlayerScripts.Length) return;

        var player = allPlayerScripts[playerWhoTriggered];

        _doorLock.OpenOrCloseDoor(player);
    }
}