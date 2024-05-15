using Unity.Netcode;
using UnityEngine;

namespace MalfunctioningDoors.Functional;

public class DoorHealth : NetworkBehaviour {
    private bool _broken;
    private DoorLock _doorLock = null!;
    private int _health = 10;

    private void Awake() =>
        SetHealthClientRpc(10);

    private void Update() {
        if (_health > 0 || _broken)
            return;

        BreakDoorClientRpc();
    }

    internal void SetDoorLock(DoorLock doorLock) =>
        _doorLock = doorLock;

    [ClientRpc]
    public void SetHealthClientRpc(int health) =>
        _health = health;

    [ClientRpc]
    public void BreakDoorClientRpc() {
        if (!_doorLock.isDoorOpened)
            _doorLock.OpenDoorAsEnemyServerRpc();

        var boxColliders = _doorLock.GetComponents<BoxCollider>();

        foreach (var boxCollider in boxColliders)
            boxCollider.enabled = false;

        _doorLock.doorTrigger.interactable = false;
        _doorLock.doorTrigger.enabled = false;
        _doorLock.enabled = false;

        _broken = true;
    }

    public int GetHealth() =>
        _health;
}