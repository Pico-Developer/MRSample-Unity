/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Unity.XR.PXR;
using UnityEngine.InputSystem;


public class Seethrough : MonoBehaviour
{
    public InputActionReference leftTrigger;
    public InputActionReference rightTrigger;

    public Transform leftSpawnPoint;
    public Transform rightSpawnPoint;

    public GameObject blueBoxPrefab;
    public GameObject redBoxPrefab;

    private void Awake()
    {
        PXR_Manager.EnableVideoSeeThrough = true;
        leftTrigger.action.started += OnLeftTrigger;
        rightTrigger.action.started += OnRightTrigger;
    }

    private void OnDestroy()
    {
        leftTrigger.action.started -= OnLeftTrigger;
        rightTrigger.action.started -= OnRightTrigger;
    }

    //Re-enable seethrough after the app resumes
    private void OnApplicationPause(bool pause)
    {
        if(!pause)
            PXR_Manager.EnableVideoSeeThrough = true;

    }

    private void OnLeftTrigger(InputAction.CallbackContext callback)
    {
       GameObject newLeftObj = Instantiate(redBoxPrefab, leftSpawnPoint.position, Quaternion.identity);
    }

    private void OnRightTrigger(InputAction.CallbackContext callback)
    {
        GameObject newRightObj = Instantiate(blueBoxPrefab, rightSpawnPoint.position, Quaternion.identity);
    }
}
