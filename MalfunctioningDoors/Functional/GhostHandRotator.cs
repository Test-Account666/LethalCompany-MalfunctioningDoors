using System.Collections;
using UnityEngine;

namespace MalfunctioningDoors.Functional;

public class GhostHandRotator : MonoBehaviour {
    internal Transform? playerControllerTransform;

    private void Start() =>
        StartCoroutine(DestroyLater(3F));

    private void Update() {
        if (!playerControllerTransform)
            return;

        transform.LookAt(playerControllerTransform);

        transform.rotation *= Quaternion.Euler(0, 90, 0);
    }

    private IEnumerator DestroyLater(float waitForSeconds) {
        yield return new WaitForSeconds(waitForSeconds);
        yield return new WaitForEndOfFrame();

        Destroy(gameObject);
    }
}