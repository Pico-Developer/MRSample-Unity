/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
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
using PicoMRDemo.Runtime.Data.Config;
using PicoMRDemo.Runtime.Data.Decoration;
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Utils;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Runtime.Pet;
using PicoMRDemo.Runtime.Runtime.PresetDecoration;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Rendering;
using VContainer;

namespace PicoMRDemo.Runtime.Service
{
    public interface IRoomService
    {
        void ShowRoomEntities();
        void HideRoomEntities();
        void EnterRoom();
        void QuitRoom();
        
        void SwitchTheme(IDecorationData data);
        void SwitchAllTheme(IDecorationData data);
    }
    public class RoomService : IRoomService
    {
        private readonly string TAG = nameof(RoomService);

        private IEntityManager _entityManager;

        private IResourceLoader _resourceLoader;

        private TweenerCore<float, float, FloatOptions> _roomEntityTween;

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
        
        public void ShowRoomEntities()
        {
            var roomEntities = _entityManager.GetRoomEntities();
            foreach (var roomEntity in roomEntities)
            {
                var label = roomEntity.GetRoomLabel();
                if (label == PxrSceneLabel.Table || label == PxrSceneLabel.Wall || label == PxrSceneLabel.Ceiling
                    || label == PxrSceneLabel.Floor || label == PxrSceneLabel.Door || label == PxrSceneLabel.Window
                    || label == PxrSceneLabel.Sofa)
                {
                    var meshRenderer = roomEntity.GameObject.GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer == null)
                    {
                        Debug.unityLogger.Log(TAG, $"Label {roomEntity.GetRoomLabel()}, Anchor Pose, Position: {roomEntity.AnchorData.Position}, Rotation: {roomEntity.AnchorData.Rotation}");
                        DrawRoomEntity(roomEntity);
                        meshRenderer = roomEntity.GameObject.GetComponentInChildren<MeshRenderer>();
                    }
                    meshRenderer.enabled = true;
                    Debug.unityLogger.Log(TAG, $"Position: {roomEntity.AnchorData.Position}, Rotation: {roomEntity.AnchorData.Rotation}");
                }
            }
        }

        public void HideRoomEntities()
        {
            var roomEntities = _entityManager.GetRoomEntities();
            foreach (var roomEntity in roomEntities)
            {
                var label = roomEntity.GetRoomLabel();
                if (label == PxrSceneLabel.Table || label == PxrSceneLabel.Wall || label == PxrSceneLabel.Ceiling)
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

        private void RefreshRoomEntity()
        {
            var roomEntities = _entityManager.GetRoomEntities();
            foreach (var roomEntity in roomEntities)
            {
                var label = roomEntity.GetRoomLabel();
                if (label == PxrSceneLabel.Ceiling || label == PxrSceneLabel.Floor )
                {
                    Debug.unityLogger.Log(TAG, $"begin {label} refresh");
                    var meshCollider = roomEntity.GameObject.GetComponentInChildren<MeshCollider>();
                    GameObject.Destroy(meshCollider);
                    var boxCollider = roomEntity.GameObject.GetComponentInChildren<BoxCollider>();
                    boxCollider.enabled = true;
                    Debug.unityLogger.Log(TAG, $"end {label} refresh");
                }
            }
        }

        public async void EnterRoom()
        {
            Debug.unityLogger.Log(TAG, "EnterRoom");
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.transform.gameObject.AddComponent<PXR_ScreenFade>();
                // var fadeEffectPrefab = _resourceLoader.AssetSetting.FadeEffect;
                // var fadeEffect = GameObject.Instantiate(fadeEffectPrefab, mainCamera.transform);
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
            var pathFinder = await InitAStar();
            RefreshRoomEntity();
            
            // set light
            var ceiling = _entityManager.GetRoomEntities(PxrSceneLabel.Ceiling);
            var floor = _entityManager.GetRoomEntities(PxrSceneLabel.Floor);
            var ceilingPosition = ceiling[0].AnchorData.Position;
            var points = ceiling[0].AnchorData.PlanePolygonInfo.Vertices;
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
            var pet = _petFactory.CreatePet(GetWalkablePosition(pathFinder), Quaternion.identity);
            CreatRoomPreviewItem();
            
            
            await _entityManager.LoadGameEntities();
        }

        public void QuitRoom()
        {
            Debug.unityLogger.Log(TAG, "QuitRoom");
        }

        public void SwitchTheme(IDecorationData data)
        {
            _themeManager.SwitchTheme(data);
        }

        public void SwitchAllTheme(IDecorationData data)
        {
            _themeManager.SwitchAllTheme(data);
        }


        private void DrawRoomEntity(IEntity entity)
        {
            var anchorObject = entity.GameObject;
            var anchorData = entity.AnchorData;
            var roomEntityMaterial = _resourceLoader.AssetSetting.RoomEntityMaterial;
            if (anchorData.SceneLabel == PxrSceneLabel.Table || anchorData.SceneLabel == PxrSceneLabel.Sofa)
            {
                Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} begin");
                var volumeInfo = entity.AnchorData.VolumeInfo;
                Debug.unityLogger.Log(TAG, $"Volume center: {volumeInfo.Center} extent: {volumeInfo.Extent}");
                Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} end");
                var roomObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                roomObject.transform.parent = anchorObject.transform;
                var localPosition = roomObject.transform.localPosition;
                localPosition = volumeInfo.Center;
                roomObject.transform.localRotation = Quaternion.identity;
                roomObject.transform.localPosition = localPosition;
                roomObject.transform.localScale = volumeInfo.Extent;

                var meshRenderer = roomObject.GetComponent<MeshRenderer>();
                
                meshRenderer.material = roomEntityMaterial;
                roomObject.layer = 11;
            }

            if (anchorData.SceneLabel == PxrSceneLabel.Wall)
            {
                Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} begin");
                var planeBoundaryInfo = entity.AnchorData.PlaneBoundaryInfo;
                var extent = planeBoundaryInfo.Extent;
                Debug.unityLogger.Log(TAG, $"Plane center: {planeBoundaryInfo.Center} extent: {planeBoundaryInfo.Extent}");
                Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} end");
                
                var wall = MeshGenerator.GenerateQuadMesh(planeBoundaryInfo.Center, planeBoundaryInfo.Extent, null);
                wall.transform.parent = anchorObject.transform;
                wall.transform.localRotation = Quaternion.identity;
                wall.transform.localPosition = Vector3.zero;
                wall.transform.localScale = Vector3.one;
                wall.AddComponent<MeshCollider>();

                var meshRenderer = wall.GetComponentInChildren<MeshRenderer>();
                // meshRenderer.material = roomEntityMaterial;
                wall.layer = 9;

                // var whiteBoard = GameObject.Instantiate(_resourceLoader.AssetSetting.WhiteBoard, wall.transform);
                CreateWhiteBoard(wall.transform, new Vector3(planeBoundaryInfo.Extent.x, planeBoundaryInfo.Extent.y, 0.1f));
                
                // generate skirting line
                var skirtingLine = MeshGenerator.GenerateSkirtingLine(planeBoundaryInfo.Center, planeBoundaryInfo.Extent, null);
                skirtingLine.transform.parent = anchorObject.transform;
                skirtingLine.transform.localRotation = Quaternion.identity;
                skirtingLine.transform.localPosition = Vector3.zero;
                skirtingLine.transform.localScale = Vector3.one;
                skirtingLine.AddComponent<MeshCollider>();
                skirtingLine.layer = 9;
            }

            if (anchorData.SceneLabel == PxrSceneLabel.Ceiling || anchorData.SceneLabel == PxrSceneLabel.Floor)
            {
                var planePolygonInfo = entity.AnchorData.PlanePolygonInfo;
                Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} begin");
                
                foreach (var vertex in planePolygonInfo.Vertices)
                {
                    Debug.unityLogger.Log(TAG, $"vertex: {vertex}");
                }
                Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} end");
                
                var roomObject = MeshGenerator.GeneratePolygonMesh(planePolygonInfo.Vertices, null);
                roomObject.transform.parent = anchorObject.transform;
                roomObject.transform.localRotation = Quaternion.identity;
                roomObject.transform.localPosition = Vector3.zero;
                roomObject.transform.localScale = Vector3.one;
                var boxCollider = roomObject.AddComponent<BoxCollider>();
                var oldSize = boxCollider.size;
                boxCollider.size = new Vector3(oldSize.x, oldSize.y, 0.2f);
                if (anchorData.SceneLabel == PxrSceneLabel.Floor)
                {
                    var oldCenter = boxCollider.center;
                    boxCollider.center = new Vector3(oldCenter.x, oldCenter.y, -0.1f);
                }
                else
                {
                    var oldCenter = boxCollider.center;
                    boxCollider.center = new Vector3(oldCenter.x, oldCenter.y, 0.1f);
                }
                boxCollider.enabled = false;
                var collider = roomObject.AddComponent<MeshCollider>();
                if (anchorData.SceneLabel == PxrSceneLabel.Floor)
                {
                    roomObject.layer = 10;
                    var meshRenderer = roomObject.GetComponentInChildren<MeshRenderer>();
                    meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                }
                else if (anchorData.SceneLabel == PxrSceneLabel.Ceiling)
                {
                    roomObject.layer = 8;
                    var meshRenderer = roomObject.GetComponentInChildren<MeshRenderer>();
                    meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                }
            }

            if (anchorData.SceneLabel == PxrSceneLabel.Door || anchorData.SceneLabel == PxrSceneLabel.Window)
            {
                Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} begin");
                var planeBoundaryInfo = entity.AnchorData.PlaneBoundaryInfo;
                var extent = planeBoundaryInfo.Extent;
                Debug.unityLogger.Log(TAG, $"Plane center: {planeBoundaryInfo.Center} extent: {planeBoundaryInfo.Extent}");
                Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} end");
                // var doorOrWindowPrefab = _resourceLoader.AssetSetting.DoorPrefab;
                // var doorOrWindow = GameObject.Instantiate(doorOrWindowPrefab);
                var doorOrWindow = MeshGenerator.GenerateQuadMesh(planeBoundaryInfo.Center, planeBoundaryInfo.Extent, null);
                doorOrWindow.transform.parent = anchorObject.transform;
                doorOrWindow.transform.localPosition = new Vector3(0f, 0f, 0.05f);
                doorOrWindow.transform.localRotation = Quaternion.identity;
                // doorOrWindow.transform.Rotate(90f, 0, 0);
                // doorOrWindow.transform.localScale = new Vector3(extent.x, 1f, extent.y);

                var meshRenderer = doorOrWindow.GetComponentInChildren<MeshRenderer>();
                meshRenderer.material = roomEntityMaterial;
                doorOrWindow.layer = 7;
                // doorOrWindow.transform.GetChild(0).gameObject.layer = 7;
            }
        }

        private void CreateWhiteBoard(Transform parent, Vector3 size)
        {
            var whiteBoard = GameObject.Instantiate(_resourceLoader.AssetSetting.WhiteBoard, parent);
            whiteBoard.transform.localScale = size;
        }
        private async UniTask<AstarPath> InitAStar()
        {
            await UniTask.Delay(1000);
            var pathFinderPrefab = _resourceLoader.AssetSetting.AStar;
            var pathFinder = GameObject.Instantiate(pathFinderPrefab).GetComponent<AstarPath>();
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
        
        private void CreatRoomPreviewItem()
        {
            var tableAreas = new Queue<Vector3>();
            var roomEntities = _entityManager.GetRoomEntities();
            
            // todo : 目前是平铺，可能都会放在一个桌子上
            foreach (var roomEntity in roomEntities)
            {
                if (roomEntity.GetRoomLabel() == PxrSceneLabel.Table)
                {
                    var anchorData = roomEntity.AnchorData;
                    var transform = roomEntity.GameObject.transform;
                    var volumeInfo = anchorData.VolumeInfo;
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

            // todo : 使用空间数据处理（目前是显示在摄像机前方）
            var mainCameraTransform = Camera.main.transform;
            _itemFactory.CreateFloatItems(ids.ToArray(), mainCameraTransform.position + Vector3.forward * 0.5f);
        }
    }
}