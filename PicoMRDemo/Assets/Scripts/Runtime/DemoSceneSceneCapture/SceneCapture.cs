/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using UnityEngine;
using Unity.XR.PXR;
using System;
using System.IO;
using System.Linq;
using PicoMRDemo.Runtime.Utils;
using UnityEngine.Rendering;


public class SceneCapture : MonoBehaviour
{
    public GameObject anchorPrefab;
    public GameObject sofaPrefab;
    public GameObject tablePrefab;
    public GameObject windowDoorPrefab;
    public GameObject wallPrefab;
    public GameObject floorCeilingPrefab;
    public GameObject polyMeshPrefab;
    public Material roomEntityMaterial;
    [SerializeField]
    private float maxDriftDelay = 0.5f;
    private bool _needUpdateRoomEntities = false;
    private bool _isLoadingRoomAnchors = false;
    List<ulong> anchorHandleList = new List<ulong>();
    private float currDriftDelay = 0f;
    private Dictionary<ulong, Transform> anchorMap = new Dictionary<ulong, Transform>();
    private List<Transform> wallAnchors = new List<Transform>();
    private Transform ceilingTransform = null;
    private Transform floorTransform = null;
    private void Awake()
    {
        //Enable Seethrough
        PXR_Manager.EnableVideoSeeThrough = true;
    }

    //Re-enable seethrough after the app resumes
    void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            PXR_Manager.EnableVideoSeeThrough = true;
        }
    }

    private async void Update()
    {
        if (_needUpdateRoomEntities&&!_isLoadingRoomAnchors)
        {
            _isLoadingRoomAnchors = true;
            var result = await PXR_MixedReality.QuerySceneAnchorAsync(flags, default);
            _isLoadingRoomAnchors = false;
            if (result.result == PxrResult.SUCCESS)
            {
                _needUpdateRoomEntities = false;
                if (result.anchorHandleList.Count > 0)
                {
                    foreach (var key in result.anchorHandleList)
                    {
                        if (!anchorHandleList.Contains(key))
                        {
                            //Load an anchor at position 
                            GameObject anchorObject = Instantiate(anchorPrefab);

                            PXR_MixedReality.LocateAnchor(key, out var position, out var rotation);
                            anchorObject.transform.position = position;
                            anchorObject.transform.rotation = rotation;
                            //Now anchor is at correct position in our space

                            Anchor anchor = anchorObject.GetComponent<Anchor>();
                            if (anchor == null)
                                anchorObject.AddComponent<Anchor>();

                            anchorMap.Add(key, anchorObject.transform);

                            PxrResult labelResult = PXR_MixedReality.GetSceneSemanticLabel(key, out var label);
                            if (labelResult == PxrResult.SUCCESS)
                            {

                                anchor.UpdateLabel(label.ToString());
                                switch (label)
                                {
                                    //Sofa&Tables&Unknown/Objects
                                    //Volume: The Anchor is located at the center of the rectangle on the upper surface of the cube with Z axis as up
                                    case PxrSemanticLabel.Sofa:
                                    case PxrSemanticLabel.Table:
                                    case PxrSemanticLabel.Chair:
                                    case PxrSemanticLabel.Human:
                                    {
                                        PXR_MixedReality.GetSceneBox3DData(key, out position, out rotation,
                                            out var extent);
                                        //extent: x-width, y-height, z-depth from center
                                        var newSofa = Instantiate(sofaPrefab, anchorObject.transform, true);
                                        //All info is relative to the anchor position
                                        newSofa.transform.localPosition = position;
                                        newSofa.transform.localRotation = rotation;
                                        newSofa.transform.localScale = extent;
                                    }
                                        break;
                                    //Wall/Window/Door
                                    //Plane: Anchor is located in the center of the plane
                                    //x-axis - width, yaxis - height, zaxis - normal vector
                                    case PxrSemanticLabel.Wall:
                                    case PxrSemanticLabel.VirtualWall:
                                    {
                                        PXR_MixedReality.GetSceneBox2DData(key, out var center, out var extent);
                                        var wall = Instantiate(wallPrefab, anchorObject.transform, true);
                                        wall.transform.localPosition = Vector3.zero; //we are already at center
                                        wall.transform.localRotation = Quaternion.identity;
                                        wall.transform.Rotate(90, 0, 0);
                                        //extent - Vector2: x-width, y-depth
                                        //0.001f because I want a thin wall
                                        //increase wall height to cover any gaps
                                        wall.transform.localScale = new Vector3(extent.x, 0.001f, extent.y * 1.1f);
                                        wallAnchors.Add(wall.transform);
                                    }
                                        break;
                                    //Windows are labeled as Doors
                                    case PxrSemanticLabel.Window:
                                    case PxrSemanticLabel.Door:
                                    case PxrSemanticLabel.Opening:
                                    {
                                        PXR_MixedReality.GetSceneBox2DData(key, out var center, out var extent);
                                        var windowDoor = Instantiate(windowDoorPrefab, anchorObject.transform, true);
                                        windowDoor.transform.localPosition = Vector3.zero; //we are already at center
                                        windowDoor.transform.localRotation = Quaternion.identity;
                                        windowDoor.transform.Rotate(90, 0, 0);
                                        //extent - Vector2: x-width, y-depth
                                        //0.001f because I want a thin wall
                                        //increase wall height to cover any gaps
                                        windowDoor.transform.localScale = new Vector3(extent.x, 0.002f, extent.y);
                                    }
                                        break;
                                    //Not currently supported in the current SDK Version
                                    //!PXR_MixedReality.GetAnchorPlanePolygonInfo(ulong anchorHandle, out Vector3[] vertices)
                                    //but! we know the anchor object as at the center
                                    case PxrSemanticLabel.Ceiling:
                                    case PxrSemanticLabel.Floor:
                                    {
                                        PXR_MixedReality.GetScenePolygonData(key, out var vertices);
                                        var verVector3S = Array.ConvertAll(vertices, v2 => new Vector3(v2.x, v2.y, 0f));
                                        var roomObject = MeshGenerator.GeneratePolygonMesh(verVector3S, roomEntityMaterial);
                                        roomObject.transform.parent = anchorObject.transform;
                                        roomObject.transform.localRotation = Quaternion.identity;
                                        roomObject.transform.localPosition = Vector3.zero;
                                        roomObject.transform.localScale = Vector3.one;
                                        var meshCollider = roomObject.AddComponent<MeshCollider>();
                                        meshCollider.convex = false;
                                        meshCollider.enabled = true;
                                        
                                    }
                                        break;
                                    case PxrSemanticLabel.Unknown:
                                    {
                                        PXR_MixedReality.GetSceneBox3DData(key, out position, out rotation,
                                            out var extent);
                                        //extent: x-width, y-height, z-depth from center
                                        var newSofa = Instantiate(sofaPrefab, anchorObject.transform, true);
                                        //All info is relative to the anchor position
                                        newSofa.transform.localPosition = position;
                                        newSofa.transform.localRotation = rotation;
                                        newSofa.transform.localScale = extent;
                                    }
                                        break;
                                }
                            }
                        }
                    }
                    anchorHandleList = result.anchorHandleList;
                }
            }
        }
    }
        
    
    // Start is called before the first frame update
    async void Start()
    {
        StartSceneCaptureProvider();
        
    }
    private async void StartSceneCaptureProvider()
    {
        var result0 = await PXR_MixedReality.StartSenseDataProvider(PxrSenseDataProviderType.SceneCapture);
        Debug.Log($"StartSceneCaptureProvider:SceneCapture: {result0}");
        var result = await PXR_MixedReality.StartSceneCaptureAsync();
        Debug.unityLogger.Log($"StartSceneCaptureAsync: {result}");
        LoadSpaceData();
    }

    private void FixedUpdate()
    {
        HandleSpatialDrift();
    }

    private void HandleSpatialDrift()
    {
        //if no anchors, we don't need to handle drift
        if (anchorMap.Count == 0)
            return;

        currDriftDelay += Time.deltaTime;
        if(currDriftDelay >= maxDriftDelay)
        {
            currDriftDelay = 0f;
            foreach(var handlePair in anchorMap)
            {
                var handle = handlePair.Key;
                var anchorTransform = handlePair.Value;

                if(handle == UInt64.MinValue)
                {
                    continue;
                }

                PXR_MixedReality.LocateAnchor(handle, out var position, out var rotation);
                anchorTransform.position= position;
                anchorTransform.rotation= rotation;
            }
        }
    }
    //What type of flags are we looking for
    //12 flags: Ceiling,Door, Floor, Opening, Unknown, Wall,VirtualWall, Window,Table,Sofa,Chair,Human
    //Load all
    PxrSemanticLabel[] flags =
    {
        PxrSemanticLabel.Ceiling,
        PxrSemanticLabel.Door,
        PxrSemanticLabel.Floor,
        PxrSemanticLabel.Table,
        PxrSemanticLabel.Sofa,
        PxrSemanticLabel.Chair,
        PxrSemanticLabel.Opening,
        PxrSemanticLabel.Unknown,
        PxrSemanticLabel.Wall,
        PxrSemanticLabel.VirtualWall,
        PxrSemanticLabel.Window,
        PxrSemanticLabel.Human
    };
    private async void LoadSpaceData()
    {
        
        Debug.Log($"Start LoadSpaceData");
        //This will trigger AnchorEntityLoaded Event
        PXR_Manager.SceneAnchorDataUpdated += DoSceneAnchorDataUpdated;
        _isLoadingRoomAnchors = true;
        var result = await PXR_MixedReality.QuerySceneAnchorAsync(flags,default);
        _isLoadingRoomAnchors = false;
        Debug.Log($"Start LoadSpaceData result " + result.result);
        if (result.result == PxrResult.SUCCESS)
        {
            Debug.Log($"Start LoadSpaceData anchorHandleList Count " + result.anchorHandleList.Count);
            if (result.anchorHandleList.Count > 0)
            {
                anchorHandleList = result.anchorHandleList;
                foreach (var key in result.anchorHandleList)
                {
                    //Load an anchor at position 
                    GameObject anchorObject = Instantiate(anchorPrefab);

                    PXR_MixedReality.LocateAnchor(key, out var position,out var rotation);
                    anchorObject.transform.position = position;
                    anchorObject.transform.rotation = rotation;
                    //Now anchor is at correct position in our space

                    Anchor anchor = anchorObject.GetComponent<Anchor>();
                    if (anchor == null)
                        anchorObject.AddComponent<Anchor>();

                    anchorMap.Add(key, anchorObject.transform);

                    PxrResult labelResult = PXR_MixedReality.GetSceneSemanticLabel(key, out var label);
                    if(labelResult == PxrResult.SUCCESS)
                    {
                        
                        anchor.UpdateLabel(label.ToString());
                        switch (label)
                        {
                            //Sofa&Tables&Unknown/Objects
                            //Volume: The Anchor is located at the center of the rectangle on the upper surface of the cube with Z axis as up
                            case PxrSemanticLabel.Sofa:
                            case PxrSemanticLabel.Table:
                            case PxrSemanticLabel.Chair:
                            case PxrSemanticLabel.Human:
                                {
                                    PXR_MixedReality.GetSceneBox3DData(key, out position,out rotation, out var extent);
                                    //extent: x-width, y-height, z-depth from center
                                    var newSofa = Instantiate(sofaPrefab, anchorObject.transform, true);
                                    //All info is relative to the anchor position
                                    newSofa.transform.localPosition = position;
                                    newSofa.transform.localRotation = Quaternion.identity;
                                    newSofa.transform.localScale = extent;
                                }
                                break;
                            //Wall/Window/Door
                            //Plane: Anchor is located in the center of the plane
                            //x-axis - width, yaxis - height, zaxis - normal vector
                            case PxrSemanticLabel.Wall:
                            case PxrSemanticLabel.VirtualWall:
                                {
                                    PXR_MixedReality.GetSceneBox2DData(key, out var center, out var extent);
                                    var wall = Instantiate(wallPrefab, anchorObject.transform, true);
                                    wall.transform.localPosition = Vector3.zero;//we are already at center
                                    wall.transform.localRotation = Quaternion.identity;
                                    wall.transform.Rotate(90, 0, 0);
                                    //extent - Vector2: x-width, y-depth
                                    //0.001f because I want a thin wall
                                    //increase wall height to cover any gaps
                                    wall.transform.localScale = new Vector3(extent.x, 0.001f, extent.y * 1.1f);
                                    wallAnchors.Add(wall.transform);
                                }
                                break;
                            //Windows are labeled as Doors
                            case PxrSemanticLabel.Window:
                            case PxrSemanticLabel.Door:
                            case PxrSemanticLabel.Opening:
                                {
                                    PXR_MixedReality.GetSceneBox2DData(key, out var center, out var extent);
                                    var windowDoor = Instantiate(windowDoorPrefab, anchorObject.transform, true);
                                    windowDoor.transform.localPosition = Vector3.zero;//we are already at center
                                    windowDoor.transform.localRotation = Quaternion.identity;
                                    windowDoor.transform.Rotate(90, 0, 0);
                                    //extent - Vector2: x-width, y-depth
                                    //0.001f because I want a thin wall
                                    //increase wall height to cover any gaps
                                    windowDoor.transform.localScale = new Vector3(extent.x, 0.002f, extent.y);
                                }
                                break;
                            //Not currently supported in the current SDK Version
                            //!PXR_MixedReality.GetScenePolygonData(ulong anchorHandle, out Vector2[] vertices)
                            //but! we know the anchor object as at the center
                            case PxrSemanticLabel.Ceiling:
                            case PxrSemanticLabel.Floor:
                                {
                                    PXR_MixedReality.GetScenePolygonData(key, out var vertices);
                                    var verVector3S = Array.ConvertAll(vertices, v2 => new Vector3(v2.x, v2.y, 0f));
                                    var roomObject = MeshGenerator.GeneratePolygonMesh(verVector3S, roomEntityMaterial);
                                    roomObject.transform.parent = anchorObject.transform;
                                    roomObject.transform.localRotation = Quaternion.identity;
                                    roomObject.transform.localPosition = Vector3.zero;
                                    roomObject.transform.localScale = Vector3.one;
                                    var meshCollider = roomObject.AddComponent<MeshCollider>();
                                    meshCollider.convex = false;
                                    meshCollider.enabled = true;
                                    
                                }
                                break;
                            case PxrSemanticLabel.Unknown:
                                {
                                    PXR_MixedReality.GetSceneBox3DData(key, out position,out rotation, out var extent);
                                    //extent: x-width, y-height, z-depth from center
                                    var newSofa = Instantiate(sofaPrefab, anchorObject.transform, true);
                                    //All info is relative to the anchor position
                                    newSofa.transform.localPosition = position;
                                    newSofa.transform.localRotation = rotation;
                                    newSofa.transform.localScale = extent;
                                }
                                break;
                                
                        }
                    }
                }
            }
        }
    }
    private void DoSceneAnchorDataUpdated()
    {
        Debug.unityLogger.Log($"Set DoSceneAnchorDataUpdated");
        _needUpdateRoomEntities = true;
    }
}
