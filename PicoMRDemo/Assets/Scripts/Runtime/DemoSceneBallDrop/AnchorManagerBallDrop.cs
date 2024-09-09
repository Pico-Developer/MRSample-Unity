/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.InputSystem;
//Using in Demo scene BallDrop
public class AnchorManagerBallDrop : MonoBehaviour
{

    public GameObject roadPreview;//right grip
    public GameObject blockPreview;//left grip
    public GameObject ballPreview;//right primary

    public GameObject roadPrefab;
    public GameObject blockPrefab;
    public GameObject ballPrefab;

    [SerializeField]
    private InputActionReference rightGrip;//place road
    [SerializeField]
    private InputActionReference leftGrip;//place block
    [SerializeField]
    private InputActionReference rightPrimary;//place Ball
    [SerializeField]
    private InputActionReference leftPrimary;//Delete

    [SerializeField]
    private float maxDriftDelay = 0.5f;

    private float _currDriftDelay = 0f;

    private Dictionary<ulong, GameObject> anchorMap= new Dictionary<ulong, GameObject>();
    private Stack<GameObject> gameObjectStack= new Stack<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        StartSpatialAnchorProvider();
    }
    
    private async void StartSpatialAnchorProvider()
    {
        var result0 = await PXR_MixedReality.StartSenseDataProvider(PxrSenseDataProviderType.SpatialAnchor);
        Debug.unityLogger.Log($"StartSenseDataProvider: {result0}");
    }

    private void OnEnable()
    {
        rightGrip.action.started += OnRightGripPressed;
        rightGrip.action.canceled += OnRightGripReleased;
        leftGrip.action.started += OnLeftGripPressed;
        leftGrip.action.canceled+= OnLeftGripReleased;
        rightPrimary.action.started += OnRightPrimaryPressed;
        rightPrimary.action.canceled += OnRightPrimaryReleased;
        leftPrimary.action.started += OnLeftPrimaryPressed;
    }

    private void OnDisable()
    {
        rightGrip.action.started -= OnRightGripPressed;
        rightGrip.action.canceled -= OnRightGripReleased;
        leftGrip.action.started -= OnLeftGripPressed;
        leftGrip.action.canceled -= OnLeftGripReleased;
        leftPrimary.action.started -= OnLeftPrimaryPressed;
        rightPrimary.action.started -= OnRightPrimaryPressed;
        rightPrimary.action.canceled -= OnRightPrimaryReleased;
    }

    private void FixedUpdate()
    {
        HandleSpatialDrift();
    }

    private void OnLeftPrimaryPressed(InputAction.CallbackContext callback)
    {
        DeleteObj();
    }

    private void DeleteObj()
    {
        if(gameObjectStack.Count > 0)
        {
            GameObject obj = gameObjectStack.Pop();
            Destroy(obj);
        }
    }

    private void OnRightPrimaryPressed(InputAction.CallbackContext callback)
    {
        ShowAnchorPreview(ballPreview);

    }

    private void OnRightPrimaryReleased(InputAction.CallbackContext callback)
    {
        CreateObj(ballPreview, ballPrefab);
    }

    //called on action.started
    private void OnRightGripPressed(InputAction.CallbackContext callback)
    {
        ShowAnchorPreview(roadPreview);
    }

    //called on action.release
    private void OnRightGripReleased(InputAction.CallbackContext callback)
    {
        CreateObj(roadPreview,roadPrefab);
    }

    private void OnLeftGripPressed(InputAction.CallbackContext callback)
    {
        ShowAnchorPreview(blockPreview);
    }

    //called on action.release
    private void OnLeftGripReleased(InputAction.CallbackContext callback)
    {
        CreateObj(blockPreview, blockPrefab);

    }

    private void ShowAnchorPreview(GameObject previewObj)
    {
        //Show anchor
        previewObj.SetActive(true);
    }

    private void CreateObj(GameObject previewObj, GameObject prefab)
    {
        previewObj.SetActive(false);
        GameObject newObj = Instantiate(prefab, previewObj.transform.position, previewObj.transform.rotation);
        gameObjectStack.Push(newObj);
    }

    private async void CreateAnchor(GameObject previewObj)
    {
        //hide anchor
        previewObj.SetActive(false);
        //Use Spatial Anchor Api to create anchor
        //This will  trigger AnchorEntityCreatedEvent
        var result = await PXR_MixedReality.CreateSpatialAnchorAsync(previewObj.transform.position, previewObj.transform.rotation);
        if (result.result == PxrResult.SUCCESS)
        {
            GameObject anchorObject = Instantiate(roadPrefab);
            anchorObject.transform.rotation = previewObj.transform.rotation;
            anchorObject.transform.position = previewObj.transform.position;
            anchorObject.GetComponent<Anchor>().UpdateMeshLabel($"Handle:{result.anchorHandle}", anchorObject.transform.position);
            //Keep track of our anchors to handle spatial drift
            gameObjectStack.Push(anchorObject);
            anchorMap.Add(result.anchorHandle, anchorObject);
        }

    }

    private void HandleSpatialDrift()
    {
        //if no anchors, dont need to handle spatial
        if (anchorMap.Count == 0)
            return;

        _currDriftDelay += Time.deltaTime;
        if(_currDriftDelay >= maxDriftDelay)
        {
            _currDriftDelay = 0;
            foreach (var handlePair in anchorMap)
            {
                var handle = handlePair.Key;
                var anchorObj = handlePair.Value;

                if (handle == UInt64.MinValue)
                {
                    continue;
                }

                PXR_MixedReality.LocateAnchor(handle, out var position, out var rotation);
                anchorObj.transform.rotation = rotation;
                anchorObj.transform.position = position;
            }
        }
    }

}
