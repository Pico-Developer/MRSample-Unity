/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PicoMRDemo.Runtime.Data.Anchor;
using PicoMRDemo.Runtime.Entity;
using Unity.XR.PXR;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace PicoMRDemo.Runtime.Mock
{
    public class MockEntityManager : IEntityManager
    {
        private readonly string TAG = nameof(MockEntityManager);
        private IList<IEntity> RoomEntities = new List<IEntity>();
        private GameObject _root;
        
        public UniTask LoadRoomEntities()
        {
            if (_root == null)
            {
                _root = new GameObject("RoomEntities");
            }
            // TODO load from config
            Debug.unityLogger.Log(TAG, $"Load Mock Anchor Data");
            // Table
            {
                ulong handle = 1111;
                MockAnchorData anchorData = new MockAnchorData(handle, new Guid());
                anchorData.SceneLabel = PxrSceneLabel.Table;
                anchorData.Position = new Vector3(1.39f, 1.17f, 0.05f);
                anchorData.Rotation = new Quaternion(0.04811f, 0.70547f, 0.70547f, -0.04811f);
                anchorData.VolumeInfo = new VolumeInfo()
                {
                    Center = new Vector3(0.00f, 0.00f, -0.56f),
                    Extent = new Vector3(0.84f, 1.28f, 1.13f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_root.transform);
                transform.position = anchorData.Position;
                transform.rotation = anchorData.Rotation;
                IEntity tableEntity = new Entity.Entity()
                {
                    AnchorData = anchorData,
                    GameObject = gameObject,
                    IsRoomEntity = true,
                };
                RoomEntities.Add(tableEntity);
            }

            // floor
            {
                ulong handle = 2222;
                MockAnchorData anchorData = new MockAnchorData(handle, new Guid());
                anchorData.SceneLabel = PxrSceneLabel.Floor;
                anchorData.Position = new Vector3(-0.19f, 0.04f, 0.62f);
                anchorData.Rotation = new Quaternion(0.69893f, -0.10720f, -0.10720f, -0.69893f);
                anchorData.PlanePolygonInfo = new PlanePolygonInfo()
                {
                    Vertices = new List<Vector3>()
                    {
                        new Vector3(3.56f, -1.57f, 0.00f),
                        new Vector3(4.55f, 5.07f, 0.00f),
                        new Vector3(-6.22f, 8.31f, 0.00f),
                        new Vector3(-5.86f, -7.25f, 0.00f),
                        new Vector3(3.98f, -4.56f, 0.00f)
                    }
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_root.transform);
                transform.position = anchorData.Position;
                transform.rotation = anchorData.Rotation;
                IEntity tableEntity = new Entity.Entity()
                {
                    AnchorData = anchorData,
                    GameObject = gameObject,
                    IsRoomEntity = true,
                };
                RoomEntities.Add(tableEntity);
            }
            
            // wall
            {
                ulong handle = 3333;
                MockAnchorData anchorData = new MockAnchorData(handle, new Guid());
                anchorData.SceneLabel = PxrSceneLabel.Wall;
                anchorData.Position = new Vector3(0.68f, 1.56f, 6.53f);
                anchorData.Rotation = new Quaternion(0.00000f, 0.95955f, 0.00000f, -0.28155f);
                anchorData.PlaneBoundaryInfo = new PlaneBoundaryInfo()
                {
                    Center = Vector3.zero,
                    Extent = new Vector2(10.20f, 3.04f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_root.transform);
                transform.position = anchorData.Position;
                transform.rotation = anchorData.Rotation;
                IEntity tableEntity = new Entity.Entity()
                {
                    AnchorData = anchorData,
                    GameObject = gameObject,
                    IsRoomEntity = true,
                };
                RoomEntities.Add(tableEntity);
            }
            
            {
                ulong handle = 4444;
                MockAnchorData anchorData = new MockAnchorData(handle, new Guid());
                anchorData.SceneLabel = PxrSceneLabel.Wall;
                anchorData.Position = new Vector3(-6.11f, 1.56f, 1.92f);
                anchorData.Rotation = new Quaternion(0.00000f, 0.81299f, 0.00000f, 0.58228f);
                anchorData.PlaneBoundaryInfo = new PlaneBoundaryInfo()
                {
                    Center = Vector3.zero,
                    Extent = new Vector2(15.56f, 3.04f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_root.transform);
                transform.position = anchorData.Position;
                transform.rotation = anchorData.Rotation;
                IEntity tableEntity = new Entity.Entity()
                {
                    AnchorData = anchorData,
                    GameObject = gameObject,
                    IsRoomEntity = true,
                };
                RoomEntities.Add(tableEntity);
            }

            {
                ulong handle = 5555;
                MockAnchorData anchorData = new MockAnchorData(handle, new Guid());
                anchorData.SceneLabel = PxrSceneLabel.Wall;
                anchorData.Position = new Vector3(4.33f, 1.56f, 2.41f);
                anchorData.Rotation = new Quaternion(0.00000f, 0.53535f, 0.00000f, -0.84463f);
                anchorData.PlaneBoundaryInfo = new PlaneBoundaryInfo()
                {
                    Center = Vector3.zero,
                    Extent = new Vector2(3.02f, 3.04f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_root.transform);
                transform.position = anchorData.Position;
                transform.rotation = anchorData.Rotation;
                IEntity tableEntity = new Entity.Entity()
                {
                    AnchorData = anchorData,
                    GameObject = gameObject,
                    IsRoomEntity = true,
                };
                RoomEntities.Add(tableEntity);
            }
            
            {
                ulong handle = 6666;
                MockAnchorData anchorData = new MockAnchorData(handle, new Guid());
                anchorData.SceneLabel = PxrSceneLabel.Wall;
                anchorData.Position = new Vector3(-3.00f, 1.56f, -5.51f);
                anchorData.Rotation = new Quaternion(0.00000f, -0.00640f, 0.00000f, -0.99998f);
                anchorData.PlaneBoundaryInfo = new PlaneBoundaryInfo()
                {
                    Center = Vector3.zero,
                    Extent = new Vector2(11.24f, 3.04f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_root.transform);
                transform.position = anchorData.Position;
                transform.rotation = anchorData.Rotation;
                IEntity tableEntity = new Entity.Entity()
                {
                    AnchorData = anchorData,
                    GameObject = gameObject,
                    IsRoomEntity = true,
                };
                RoomEntities.Add(tableEntity);
            }
            
            {
                ulong handle = 7777;
                MockAnchorData anchorData = new MockAnchorData(handle, new Guid());
                anchorData.SceneLabel = PxrSceneLabel.Wall;
                anchorData.Position = new Vector3(3.15f, 1.56f, -2.27f);
                anchorData.Rotation = new Quaternion(0.00000f, 0.64921f, 0.00000f, -0.76061f);
                anchorData.PlaneBoundaryInfo = new PlaneBoundaryInfo()
                {
                    Center = Vector3.zero,
                    Extent = new Vector2(6.72f, 3.04f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_root.transform);
                transform.position = anchorData.Position;
                transform.rotation = anchorData.Rotation;
                IEntity tableEntity = new Entity.Entity()
                {
                    AnchorData = anchorData,
                    GameObject = gameObject,
                    IsRoomEntity = true,
                };
                RoomEntities.Add(tableEntity);
            }
            
            // ceiling
            {
                ulong handle = 8888;
                MockAnchorData anchorData = new MockAnchorData(handle, new Guid());
                anchorData.SceneLabel = PxrSceneLabel.Ceiling;
                anchorData.Position = new Vector3(-0.19f, 3.09f, 0.62f);
                anchorData.Rotation = new Quaternion(-0.10720f, 0.69893f, -0.69893f, -0.10720f);
                anchorData.PlanePolygonInfo = new PlanePolygonInfo()
                {
                    Vertices = new List<Vector3>()
                    {
                        new Vector3(-3.56f, -1.57f, 0.00f),
                        new Vector3(-4.55f, 5.07f, 0.00f),
                        new Vector3(6.22f, 8.31f, 0.00f),
                        new Vector3(5.86f, -7.25f, 0.00f),
                        new Vector3(-3.98f, -4.56f, 0.00f)
                    }
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_root.transform);
                transform.position = anchorData.Position;
                transform.rotation = anchorData.Rotation;
                IEntity tableEntity = new Entity.Entity()
                {
                    AnchorData = anchorData,
                    GameObject = gameObject,
                    IsRoomEntity = true,
                };
                RoomEntities.Add(tableEntity);
            }

            // Door
            {
                ulong handle = 9999;
                MockAnchorData anchorData = new MockAnchorData(handle, new Guid());
                anchorData.SceneLabel = PxrSceneLabel.Door;
                anchorData.Position = new Vector3(3.20f, 1.31f, -0.60f);
                anchorData.Rotation = new Quaternion(0.00000f, 0.63994f, 0.00000f, -0.76842f);
                anchorData.PlaneBoundaryInfo = new PlaneBoundaryInfo()
                {
                    Center = Vector3.zero,
                    Extent = new Vector2(1.09f, 2.44f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_root.transform);
                transform.position = anchorData.Position;
                transform.rotation = anchorData.Rotation;
                IEntity tableEntity = new Entity.Entity()
                {
                    AnchorData = anchorData,
                    GameObject = gameObject,
                    IsRoomEntity = true,
                };
                RoomEntities.Add(tableEntity);
            }
            
            // Sofa
            {
                ulong handle = 11111;
                MockAnchorData anchorData = new MockAnchorData(handle, new Guid());
                anchorData.SceneLabel = PxrSceneLabel.Sofa;
                anchorData.Position = new Vector3(-0.24f, 1.02f, -0.74f);
                anchorData.Rotation = new Quaternion(0.53312f, 0.46453f, 0.46453f, -0.53312f);
                anchorData.VolumeInfo = new VolumeInfo()
                {
                    Center = new Vector3(0.00f, 0.00f, -0.47f),
                    Extent = new Vector3(0.46f, 0.55f, 0.95f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_root.transform);
                transform.position = anchorData.Position;
                transform.rotation = anchorData.Rotation;
                IEntity tableEntity = new Entity.Entity()
                {
                    AnchorData = anchorData,
                    GameObject = gameObject,
                    IsRoomEntity = true,
                };
                RoomEntities.Add(tableEntity);
            }
            
            
            return UniTask.CompletedTask;
        }

        public UniTask LoadGameEntities()
        {
            return UniTask.CompletedTask;
        }

        public UniTask SaveGameEntities()
        {
            return UniTask.CompletedTask;
        }

        public UniTask ClearGameEntities()
        {
            throw new NotImplementedException();
        }

        public async UniTask<IEntity> CreateAndAddEntity(GameObject gameObject)
        {
            return null;
        }

        public void DeleteEntity(IEntity entity)
        {
            Object.Destroy(entity.GameObject);
        }

        public IList<IEntity> GetRoomEntities()
        {
            return RoomEntities;
        }

        public IList<IEntity> GetGameEntities()
        {
            throw new System.NotImplementedException();
        }

        public IList<IEntity> GetRoomEntities(PxrSceneLabel label)
        {
            IList<IEntity> result = new List<IEntity>();
            foreach (var roomEntity in RoomEntities)
            {
                if (roomEntity.GetRoomLabel() == label)
                {
                    result.Add(roomEntity);
                }
            }
            return result;
        }
    }
}