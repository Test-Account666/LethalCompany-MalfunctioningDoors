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
using UnityEngine;

namespace MalfunctioningDoors.Functional;

public class GhostHandRotator : MonoBehaviour {
    internal Transform? playerControllerTransform;

    private void Start() => StartCoroutine(DestroyLater(3F));

    private void Update() {
        if (!playerControllerTransform) return;

        transform.LookAt(playerControllerTransform);

        transform.rotation *= Quaternion.Euler(0, 90, 0);
    }

    private IEnumerator DestroyLater(float waitForSeconds) {
        yield return new WaitForSeconds(waitForSeconds);
        yield return new WaitForEndOfFrame();

        Destroy(gameObject);
    }
}