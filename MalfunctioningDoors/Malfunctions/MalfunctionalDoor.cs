using System.Collections;
using BepInEx.Configuration;
using GameNetcodeStuff;
using MalfunctioningDoors.Patches;
using UnityEngine;

namespace MalfunctioningDoors.Malfunctions;

public abstract class MalfunctionalDoor : MonoBehaviour {
    private static int _changeMalfunctionChance = 30;
    protected DoorLock doorLock = null!;

    private void Start() =>
        StartCoroutine(RollChangeMalfunctionChance());

    public abstract void TouchInteract(PlayerControllerB playerControllerB);
    public abstract void UseInteract(PlayerControllerB playerControllerB);

    public static void InitializeConfig(ConfigFile configFile) =>
        _changeMalfunctionChance = configFile.Bind("1. General", "2. Malfunction Change Chance", 30,
                                                   "Defines the chance, if a malfunction is changed").Value;

    public abstract void UseKey();

    public abstract bool ShouldExecute();

    private IEnumerator RollChangeMalfunctionChance() {
        while (true) {
            yield return new WaitForSeconds(60);
            yield return new WaitForEndOfFrame();

            var chance = DoorLockPatch.syncedRandom.Next(0, 100);

            if (chance > _changeMalfunctionChance)
                continue;

            var malfunctionalDoor = MalfunctionGenerator.GenerateMalfunctionalDoor(DoorLockPatch.syncedRandom);

            DoorLockPatch.AddMalfunction(doorLock, malfunctionalDoor);
            Destroy(this);
            break;
        }
    }
}