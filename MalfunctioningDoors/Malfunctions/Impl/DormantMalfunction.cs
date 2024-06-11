using GameNetcodeStuff;

namespace MalfunctioningDoors.Malfunctions.Impl;

[Malfunction(100)]
public class DormantMalfunction : MalfunctionalDoor {
    public override void TouchInteract(PlayerControllerB playerControllerB) {
    }

    public override void UseInteract(PlayerControllerB playerControllerB) {
    }

    public override void UseKey() {
    }

    public override bool ShouldExecute() => false;
}