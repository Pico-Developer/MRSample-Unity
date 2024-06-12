/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Pathfinding;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Data.Config;
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Runtime.PresetDecoration;
using PicoMRDemo.Runtime.Utils;
using Unity.XR.PXR;
using UnityEngine;
using VContainer;

namespace PicoMRDemo.Runtime.Service
{
    public class VirtualWorldManager : IVirtualWorldManager
    {
        public event Func<List<GraphNode>, UniTask> OnOpenWorldFinished;
        public event Func<UniTask> OnCloseWorldStart;
        public bool IsOpen { get; private set; } = false;
        public bool IsOpeningOrClosing => _isInOpeningOrClosing;
        private bool _isInOpeningOrClosing = false;


        [Inject]
        private IResourceLoader _resourceLoader;

        [Inject]
        private IEntityManager _entityManager;

        [Inject]
        private IPresetDecorationManager _presetDecorationManager;
        
        private readonly string TAG = nameof(VirtualWorldManager);

        public async UniTask OpenWorldAsync(CancellationToken cancellationToken)
        {
            if (_isInOpeningOrClosing)
                return;

            _isInOpeningOrClosing = true;

            // 打开virtual world
            Debug.unityLogger.Log(TAG, $"打开virtual world");
            if (_mirror == null)
            {
                var wall = _entityManager.GetRoomEntities().First(x => x.GetRoomLabel() == PxrSceneLabel.Wall && !IsEntityHasWindow(x));
                DrawPortal(wall);
            }
            _mirror.SetActive(true);
            _skybox.SetActive(true);
            var changedNodes = AddPlane();
            await PlayOpen(cancellationToken);
            
            _isInOpeningOrClosing = false;
            IsOpen = true;
            if (OnOpenWorldFinished != null)
            {
                await OnOpenWorldFinished.Invoke(changedNodes);
            }
        }

        public async UniTask CloseWorldAsync(CancellationToken cancellationToken)
        {
            if (_isInOpeningOrClosing)
                return;

            _isInOpeningOrClosing = true;

            if (OnCloseWorldStart != null)
            {
                await OnCloseWorldStart.Invoke();
            }
            
            // 关闭virtual world
            Debug.unityLogger.Log(TAG, $"关闭virtual world");
            RemovePlane();
            await PlayClose(cancellationToken);

            if (_mirror != null)
            {
                _mirror.SetActive(false);
            }
            _skybox.SetActive(false);
            _isInOpeningOrClosing = false;
            IsOpen = false;
        }

        private GameObject _mirror;
        private GameObject _extentPlane;
        private GameObject _skybox;
        private HashSet<GraphNode> _originNodes = new HashSet<GraphNode>();
        private List<GraphNode> _changedNodes = new List<GraphNode>();
        private void DrawPortal(IEntity entity)
        {
            var anchorObject = entity.GameObject;
            var anchorData = entity.AnchorData;
            var mirrorMaterial = _resourceLoader.AssetSetting.Mirror;
            var roomEntityMaterial = _resourceLoader.AssetSetting.RoomEntityMaterial;

            if (anchorData.SceneLabel == PxrSceneLabel.Wall)
            {
                Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} begin");
                var planeBoundaryInfo = entity.AnchorData.PlaneBoundaryInfo;
                var extent = planeBoundaryInfo.Extent;
                Debug.unityLogger.Log(TAG, $"Plane center: {planeBoundaryInfo.Center} extent: {planeBoundaryInfo.Extent}");
                Debug.unityLogger.Log(TAG, $"Label: {anchorData.SceneLabel} end");
                
                var wall = MeshGenerator.GenerateQuadMesh(planeBoundaryInfo.Center, planeBoundaryInfo.Extent, mirrorMaterial);
                wall.transform.parent = anchorObject.transform;
                wall.transform.localRotation = Quaternion.identity;
                wall.transform.localPosition = new Vector3(0f, 0f, 0.1f);
                wall.transform.localScale = Vector3.zero;
                wall.AddComponent<MeshCollider>();

                _skybox = GameObject.Instantiate(_resourceLoader.AssetSetting.Skybox);
                _skybox.transform.position = Vector3.zero;
                // _skybox.transform.SetParent(anchorObject.transform, false);
                _mirror = wall;

                var center = planeBoundaryInfo.Center;
                var halfExtentY = planeBoundaryInfo.Extent.y * 0.5f;
                var halfExtentX = planeBoundaryInfo.Extent.x * 0.5f;
                var realCenter = center - new Vector3(0, halfExtentY, 0) - new Vector3(0, 0, halfExtentY);
                _extentPlane = MeshGenerator.GeneratePlane(realCenter, planeBoundaryInfo.Extent, roomEntityMaterial);
                _extentPlane.transform.parent = anchorObject.transform;
                _extentPlane.transform.localRotation = Quaternion.identity;
                _extentPlane.transform.localPosition = new Vector3(0f, 0f, 0f);
                _extentPlane.transform.localScale = Vector3.one;
                _extentPlane.layer = 10;
                _extentPlane.AddComponent<BoxCollider>();
                _extentPlane.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        private List<GraphNode> AddPlane()
        {
            if (_originNodes.Count <= 0)
            {
                AstarPath.active.data.GetNodes((node) =>
                {
                    if (node.Walkable)
                        _originNodes.Add(node);
                });
            }
            _extentPlane.SetActive(true);
            var collider = _extentPlane.GetComponent<BoxCollider>();
            var guo = new GraphUpdateObject(collider.bounds);
            // Set some settings
            var tempNodes = new List<GraphNode>();
            guo.trackChangedNodes = true;
            guo.updatePhysics = true;
            AstarPath.active.UpdateGraphs(guo);
            if (_changedNodes.Count <= 0)
            {
                AstarPath.active.FlushGraphUpdates();
                tempNodes.AddRange(guo.changedNodes);
                foreach (var tempNode in tempNodes)
                {
                    if (!_originNodes.Contains(tempNode))
                    {
                        _changedNodes.Add(tempNode);
                    }
                }
            }
            return tempNodes;
        }

        private void RemovePlane()
        {
            _extentPlane.SetActive(false);
            if (_changedNodes.Count > 0)
            {
                foreach (var changedNode in _changedNodes)
                {
                    changedNode.Walkable = false;
                }
                AstarPath.active.FlushGraphUpdates();
            }
        }

        private async UniTask PlayOpen(CancellationToken cancellationToken)
        {
            await UniTask.WhenAll(_mirror.transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.InBack).WithCancellation(cancellationToken));
        }

        private async UniTask PlayClose(CancellationToken cancellationToken)
        {            
            await UniTask.WhenAll(_mirror.transform.DOScale(Vector3.zero, 0.6f).SetEase(Ease.OutBack).WithCancellation(cancellationToken));
        }

        private bool IsEntityHasWindow(IEntity entity)
        {
            return _presetDecorationManager.HasDecoration(PresetType.Window, entity.GameObject);
        }
    }
}