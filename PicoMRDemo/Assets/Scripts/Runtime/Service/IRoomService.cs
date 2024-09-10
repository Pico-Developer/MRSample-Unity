/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Pathfinding;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Data.Decoration;
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Utils;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Runtime.Pet;
using PicoMRDemo.Runtime.Runtime.PresetDecoration;
using PicoMRDemo.Runtime.UI;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer;

namespace PicoMRDemo.Runtime.Service
{
    public interface IRoomService
    {
        bool IsAnchorCreate();
        void SetAnchorCreate(bool isAnchorCreate);
        void ShowRoomEntities();
        void HideRoomEntities();

        void RefreshRoomEntities();
        void EnterRoom();
        void QuitRoom();
        
        void SwitchTheme(IDecorationData data);
        void SwitchAllTheme(IDecorationData data);
        void ResetPetPosition();
    }
    public class RoomService : IRoomService
    {
        private bool _isAnchorCreate = false;
        
        
        private readonly string TAG = nameof(RoomService);
        
        private IEntityManager _entityManager;

        private IResourceLoader _resourceLoader;

        private TweenerCore<float, float, FloatOptions> _roomEntityTween;

        private AstarPath _astarpathFinder;

        private GameObject _pet;
        [Inject]
        private IPetFactory _petFactory;

        [Inject]
        private ILightManager _lightManager;

        [Inject]
        private IPresetDecorationManager _presetDecorationManager;

        [Inject]
        private IThemeManager _themeManager;

        [Inject] 
        private IPersistentLoader _persistentLoader;
        
        [Inject]
        public RoomService(IEntityManager entityManager, IResourceLoader resourceLoader)
        {
            _entityManager = entityManager;
            _resourceLoader = resourceLoader;
        }

        public bool IsAnchorCreate()
        {
            return _isAnchorCreate;
        }
        
        public void SetAnchorCreate(bool isAnchorCreate)
        {
            _isAnchorCreate = isAnchorCreate;
        }
        
        public void ShowRoomEntities()
        {
            var roomEntities = _entityManager.GetRoomEntities();
            foreach (var roomEntity in roomEntities)
            {
                var label = roomEntity.GetRoomLabel();
                if (label == PxrSemanticLabel.Table || label == PxrSemanticLabel.Wall || label == PxrSemanticLabel.Ceiling
                    || label == PxrSemanticLabel.Floor || label == PxrSemanticLabel.Door || label == PxrSemanticLabel.Window
                    || label == PxrSemanticLabel.Sofa || label == PxrSemanticLabel.Opening || label == PxrSemanticLabel.Chair
                    || label == PxrSemanticLabel.VirtualWall || label == PxrSemanticLabel.Human)
                {
                    var meshRenderer = roomEntity.GameObject.GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer == null)
                    {
                        //Debug.unityLogger.Log(TAG, $"Label {roomEntity.GetRoomLabel()}, Anchor Pose, Position: {roomEntity.AnchorData.Position}, Rotation: {roomEntity.AnchorData.Rotation}");
                        DrawRoomEntity(roomEntity);
                        meshRenderer = roomEntity.GameObject.GetComponentInChildren<MeshRenderer>();
                    }
                    meshRenderer.enabled = true;
                    //Debug.unityLogger.Log(TAG, $"Position: {roomEntity.AnchorData.Position}, Rotation: {roomEntity.AnchorData.Rotation}");
                }
            }
        }

        public void HideRoomEntities()
        {
            var roomEntities = _entityManager.GetRoomEntities();
            foreach (var roomEntity in roomEntities)
            {
                var label = roomEntity.GetRoomLabel();
                if (label == PxrSemanticLabel.Table || label == PxrSemanticLabel.Wall || label == PxrSemanticLabel.Ceiling
                    || label == PxrSemanticLabel.Floor || label == PxrSemanticLabel.Door || label == PxrSemanticLabel.Window
                    || label == PxrSemanticLabel.Sofa || label == PxrSemanticLabel.Opening || label == PxrSemanticLabel.Chair
                    || label == PxrSemanticLabel.VirtualWall || label == PxrSemanticLabel.Human)
                {
                    var meshRenderer = roomEntity.GameObject.GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer == null)
                    {
                        DrawRoomEntity(roomEntity);
                        meshRenderer = roomEntity.GameObject.GetComponentInChildren<MeshRenderer>();
                    }
                    meshRenderer.enabled = false;
                }
            }
        }

        public void RefreshRoomEntities()
        {
            ShowRoomEntities();
            var themeDatas = _persistentLoader.GetAllThemeDatas();
            if (themeDatas.Count <= 0)
            {
                _themeManager.SwitchToDefaultTheme();
            }
            else
            {
                foreach (var decorationData in themeDatas)
                {
                    _themeManager.SwitchTheme(decorationData);
                }
            }
        }

        public async void EnterRoom()
        {
            Debug.unityLogger.Log(TAG, "EnterRoom");
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                if (mainCamera.transform.gameObject.GetComponent<PXR_ScreenFade>() == null)
                {
                    mainCamera.transform.gameObject.AddComponent<PXR_ScreenFade>();
                }
            }
            ShowRoomEntities();
            // theme
            var themeDatas = _persistentLoader.GetAllThemeDatas();
            if (themeDatas.Count <= 0)
            {
                _themeManager.SwitchToDefaultTheme();
            }
            else
            {
                foreach (var decorationData in themeDatas)
                {
                    _themeManager.SwitchTheme(decorationData);
                }
            }
            

            if (_astarpathFinder == null)
            {
                _astarpathFinder = await InitAStar();
            }
            
            ReplaceFloorAndCeilingMeshCollider();
            // set light
            var ceiling = _entityManager.GetRoomEntities(PxrSemanticLabel.Ceiling);
            var floor = _entityManager.GetRoomEntities(PxrSemanticLabel.Floor);
            var ceilingPosition = ceiling[0].AnchorData.Position;
            var points = ceiling[0].AnchorData.ScenePolygonData.Vertices;
            Vector3 ceilingCenter = Vector3.zero;
            foreach (var point in points)
            {
                ceilingCenter += point;
            }
            ceilingCenter /= points.Count;
            ceilingCenter = ceiling[0].GameObject.transform.TransformPoint(ceilingCenter);
            var ceilingHeight = ceilingPosition.y;
            var floorHeight = floor[0].AnchorData.Position.y;
            _lightManager.SetMainLightPositionByCeilingPosition(ceilingCenter);
            _lightManager.SetMainLightIntensityByCeilingHeight(ceilingHeight - floorHeight);
            await UniTask.Delay(1000);
            if (_pet == null)
            {
                _pet = _petFactory.CreatePet(GetWalkablePosition(_astarpathFinder), Quaternion.identity);
                ControllerManager.Instance.BingingTriggerHotKey(true, (args) =>
                {
                    var controller = ControllerManager.Instance.GetController(true);
                    Ray ray = new Ray(controller.transform.position, controller.transform.forward);
                    if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("Floor")))
                    {
                        var node = _astarpathFinder.GetNearest(hit.point);
                        if (_pet)
                        {
                            _pet.transform.position = node.position;
                        }
                    }
                });
                ControllerManager.Instance.BingingTriggerHotKey(false, (args) =>
                {
                    var controller = ControllerManager.Instance.GetController(false);
                    Ray ray = new Ray(controller.transform.position, controller.transform.forward);
                    if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("Floor")))
                    {
                        var node = _astarpathFinder.GetNearest(hit.point);
                        if (_pet)
                        {
                            _pet.transform.position = node.position;
                        }
                    }
                });
            }
            else
            {
                _pet.transform.position = GetWalkablePosition(_astarpathFinder);
                _pet.SetActive(true);
            }
            //CreatRoomPreviewItem();

            
            await _entityManager.LoadGameEntities();
        }

        public async void QuitRoom()
        {
            Debug.unityLogger.Log(TAG, "QuitRoom");
            HideRoomEntities();
            await _entityManager.ClearRoomEntities();
            _entityManager.ClearGameEntities();
            if (_pet)
            {
                _pet.SetActive(false);
            }
        }

        public void ResetPetPosition()
        {
            if (Camera.main != null)
            {
                var node = _astarpathFinder.GetNearest(Camera.main.transform.position + Vector3.forward * 0.5f);
                if (_pet)
                {
                    _pet.transform.position = node.position;
                }
            }
        }

        public void SwitchTheme(IDecorationData data)
        {
            _themeManager.SwitchTheme(data);
        }

        public void SwitchAllTheme(IDecorationData data)
        {
            _themeManager.SwitchAllTheme(data);
        }

        private void ReplaceFloorAndCeilingMeshCollider()
        {
            var roomEntities = _entityManager.GetRoomEntities();
            foreach (var roomEntity in roomEntities)
            {
                var label = roomEntity.GetRoomLabel();
                if (label == PxrSemanticLabel.Ceiling || label == PxrSemanticLabel.Floor )
                {
                    //Debug.unityLogger.Log(TAG, $"begin {label} refresh");
                    var meshCollider = roomEntity.GameObject.GetComponentInChildren<MeshCollider>();
                    meshCollider.enabled = false;
                    var boxCollider = roomEntity.GameObject.GetComponentInChildren<BoxCollider>();
                    boxCollider.enabled = true;
                    //Debug.unityLogger.Log(TAG, $"end {label} refresh");
                }
            }
        }
        private void DrawRoomEntity(IEntity entity)
        {
            var anchorObject = entity.GameObject;
            var anchorData = entity.AnchorData;
            var roomEntityMaterial = _resourceLoader.AssetSetting.RoomEntityMaterial;
            if (anchorData.SceneLabel == PxrSemanticLabel.Table 
                || anchorData.SceneLabel == PxrSemanticLabel.Sofa 
                || anchorData.SceneLabel == PxrSemanticLabel.Chair
                || anchorData.SceneLabel == PxrSemanticLabel.Human)
            {
                //Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} begin");
                var box3DInfo = entity.AnchorData.SceneBox3DData;
                //Debug.unityLogger.Log(TAG, $"Volume center: {box3DInfo.Center} extent: {box3DInfo.Extent}");
                //Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} end");
                var roomObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                roomObject.transform.parent = anchorObject.transform;
                var localPosition = box3DInfo.Center;
                roomObject.transform.localRotation = Quaternion.identity;
                roomObject.transform.localPosition = localPosition;
                roomObject.transform.localScale = box3DInfo.Extent;

                var meshRenderer = roomObject.GetComponent<MeshRenderer>();
                
                meshRenderer.material = roomEntityMaterial;
                roomObject.layer = 11;
            }

            if (anchorData.SceneLabel == PxrSemanticLabel.Wall || anchorData.SceneLabel == PxrSemanticLabel.VirtualWall)
            {
                //Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} begin");
                var box2DInfo = entity.AnchorData.SceneBox2DData;
                var extent = box2DInfo.Extent;
                //Debug.unityLogger.Log(TAG, $"Plane center: {box2DInfo.Center} extent: {box2DInfo.Extent}");
                //Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} end");
                
                var wall = MeshGenerator.GenerateQuadMesh(box2DInfo.Center, box2DInfo.Extent, null);
                wall.transform.parent = anchorObject.transform;
                wall.transform.localRotation = Quaternion.identity;
                wall.transform.localPosition = Vector3.zero;
                wall.transform.localScale = Vector3.one;
                var meshCollider = wall.AddComponent<MeshCollider>();
                meshCollider.convex = true;
                wall.layer = 9;

                CreateWhiteBoard(wall.transform, new Vector3(box2DInfo.Extent.x, box2DInfo.Extent.y, 0.1f));

                // generate skirting line
                var skirtingLine = MeshGenerator.GenerateSkirtingLine(box2DInfo.Center, box2DInfo.Extent, null);
                skirtingLine.transform.parent = anchorObject.transform;
                skirtingLine.transform.localRotation = Quaternion.identity;
                skirtingLine.transform.localPosition = Vector3.zero;
                skirtingLine.transform.localScale = Vector3.one;
                skirtingLine.AddComponent<MeshCollider>();
                skirtingLine.layer = 9;
            }

            if (anchorData.SceneLabel == PxrSemanticLabel.Ceiling || anchorData.SceneLabel == PxrSemanticLabel.Floor)
            {
                var planePolygonInfo = entity.AnchorData.ScenePolygonData;
                
                var roomObject = MeshGenerator.GeneratePolygonMesh(planePolygonInfo.Vertices, null);
                roomObject.transform.parent = anchorObject.transform;
                roomObject.transform.localRotation = Quaternion.identity;
                roomObject.transform.localPosition = Vector3.zero;
                roomObject.transform.localScale = Vector3.one;
                var meshCollider = roomObject.AddComponent<MeshCollider>();
                meshCollider.convex = false;
                meshCollider.enabled = true;
                var boxCollider = roomObject.AddComponent<BoxCollider>();
                var oldSize = boxCollider.size;
                boxCollider.size = new Vector3(oldSize.x, oldSize.y, 0.02f);
                if (anchorData.SceneLabel == PxrSemanticLabel.Floor)
                {
                    var oldCenter = boxCollider.center;
                    boxCollider.center = new Vector3(oldCenter.x, oldCenter.y, -0.01f);
                }
                else
                {
                    var oldCenter = boxCollider.center;
                    boxCollider.center = new Vector3(oldCenter.x, oldCenter.y, 0.01f);
                }
                boxCollider.enabled = false;
                if (anchorData.SceneLabel == PxrSemanticLabel.Floor)
                {
                    roomObject.layer = 10;
                    var meshRenderer = roomObject.GetComponentInChildren<MeshRenderer>();
                    meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                }
                else if (anchorData.SceneLabel == PxrSemanticLabel.Ceiling)
                {
                    roomObject.layer = 8;
                    var meshRenderer = roomObject.GetComponentInChildren<MeshRenderer>();
                    meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                }
            }

            if (anchorData.SceneLabel == PxrSemanticLabel.Door || anchorData.SceneLabel == PxrSemanticLabel.Window || anchorData.SceneLabel == PxrSemanticLabel.Opening)
            {
                var box2DInfo = entity.AnchorData.SceneBox2DData;
                var extent = box2DInfo.Extent;
                var doorOrWindow = MeshGenerator.GenerateQuadMesh(box2DInfo.Center, box2DInfo.Extent, null);
                doorOrWindow.transform.parent = anchorObject.transform;
                doorOrWindow.transform.localPosition = new Vector3(0f, 0f, 0.05f);
                doorOrWindow.transform.localRotation = Quaternion.identity;

                var meshRenderer = doorOrWindow.GetComponentInChildren<MeshRenderer>();
                meshRenderer.material = roomEntityMaterial;
                doorOrWindow.layer = 7;
                // doorOrWindow.transform.GetChild(0).gameObject.layer = 7;
            }
        }

        private void CreateWhiteBoard(Transform parent, Vector3 size)
        {
            var whiteBoardFront = Object.Instantiate(_resourceLoader.AssetSetting.WhiteBoard, parent);
            var whiteBoardBack = Object.Instantiate(_resourceLoader.AssetSetting.WhiteBoard, parent);
            whiteBoardFront.transform.localScale = size;
            whiteBoardBack.transform.localScale = size;
            var localPosition = whiteBoardBack.transform.localPosition;
            localPosition.z = -whiteBoardBack.transform.localPosition.z;
            whiteBoardBack.transform.localPosition = localPosition;
        }
        private async UniTask<AstarPath> InitAStar()
        {
            await UniTask.Delay(1000);
            var pathFinderPrefab = _resourceLoader.AssetSetting.AStar;
            var pathFinder = Object.Instantiate(pathFinderPrefab).GetComponent<AstarPath>();
            return pathFinder;
        }
        
        private Vector3 GetWalkablePosition(AstarPath astarPath)
        {
            List<GraphNode> walkableNodes = new List<GraphNode>();
            astarPath.data.GetNodes((node) =>
            {
                if (node.Walkable)
                {
                    walkableNodes.Add(node);
                }
            });
            Vector3 result = Vector3.zero;
            if (walkableNodes.Count > 0)
            {
                var node = walkableNodes[Random.Range(0, walkableNodes.Count)];
                result = (Vector3)node.position;
            }
            return result;
        }
        
        [Inject]
        private IDecorationDataLoader _decorationDataLoader;
        [Inject]
        private IItemFactory _itemFactory;
        //For Debug Entity Create Test
        private void CreatRoomPreviewItem()
        {
            var tableAreas = new Queue<Vector3>();
            var roomEntities = _entityManager.GetRoomEntities();
            
            foreach (var roomEntity in roomEntities)
            {
                if (roomEntity.GetRoomLabel() == PxrSemanticLabel.Table)
                {
                    var anchorData = roomEntity.AnchorData;
                    var transform = roomEntity.GameObject.transform;
                    var volumeInfo = anchorData.SceneBox3DData;
                    if (volumeInfo.Extent.x < ConstantProperty.SlotSize || volumeInfo.Extent.z < ConstantProperty.SlotSize)
                    {
                        continue;
                    }

                    Vector3 offset = new Vector3(- volumeInfo.Extent.x / 2, - volumeInfo.Extent.y / 2, 0.1f);
                    int lenghtNum = (int)(volumeInfo.Extent.x / ConstantProperty.SlotSize);
                    int widthNum = (int)(volumeInfo.Extent.y / ConstantProperty.SlotSize);
                    for (int i = 0; i < lenghtNum; i++)
                    {
                        for (int j = 0; j < widthNum; j++)
                        {
                            var local = offset + new Vector3(ConstantProperty.SlotSize * (i + 0.5f), ConstantProperty.SlotSize * (j + 0.5f), 0);
                            var target = transform.TransformPoint(local);
                            tableAreas.Enqueue(target);
                        }
                    }
                }
            }
            var data = _decorationDataLoader.LoadData(DecorationType.Item);

            var ids = new List<ulong>();
            foreach (var item in data)
            {
                if (item is DecorationData decorationData)
                {
                    if ((decorationData.ItemType & (~ItemType.Normal)) > 0)
                    {
                        continue;
                    }
                }
                var resourceID = ((DecorationData)item).ID;
                if (tableAreas.Count > 0)
                {
                    var pos = tableAreas.Dequeue();
                    _itemFactory.CreateItem(resourceID, pos, Quaternion.identity, ItemState.Normal); 
                }
                else
                {
                    ids.Add(resourceID);
                }
            }

            if (Camera.main != null)
            {
                var mainCameraTransform = Camera.main.transform;
                _itemFactory.CreateFloatItems(ids.ToArray(), mainCameraTransform.position + Vector3.forward * 0.5f);
            }
        }
    }
}