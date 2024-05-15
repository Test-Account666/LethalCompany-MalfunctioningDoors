using Unity.Netcode;

namespace MalfunctioningDoors.Functional;

public class DoorLocker : NetworkBehaviour {
    private DoorLock _doorLock = null!;

    private void Awake() {
        _doorLock = GetComponent<DoorLock>();
    }

    [ServerRpc(RequireOwnership = false)]
    public void LockDoorServerRpc() {
        LockDoorClientRpc();
    }

    [ClientRpc]
    private void LockDoorClientRpc() {
        _doorLock.LockDoor();
    }
}