/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Data.Anchor;
using PicoMRDemo.Runtime.Entity;
using Unity.XR.PXR;
using UnityEngine;
using PicoMRDemo.Runtime.Runtime.Item;
using VContainer;
using Object = UnityEngine.Object;

namespace PicoMRDemo.Runtime.Mock
{
    public class MockEntityManager : IEntityManager
    {
        private readonly string TAG = nameof(MockEntityManager);
        private IList<IEntity> RoomEntities = new List<IEntity>();
        private IList<IEntity> GameEntities = new List<IEntity>();
        private GameObject _roomEntityRoot;
        private GameObject _gameEntityRoot;
        
        [Inject]
        private IPersistentLoader _persistentLoader;
        public async UniTask ClearRoomEntities()
        {
            foreach (var roomEntity in RoomEntities)
            {
                Object.Destroy(roomEntity.GameObject);
            }
            RoomEntities.Clear();
        }
        public UniTask LoadRoomEntities()
        {
            if (_roomEntityRoot == null)
            {
                _roomEntityRoot = new GameObject("RoomEntities");
            }
            // TODO load from config
            Debug.unityLogger.Log(TAG, $"Load Mock Anchor Data");
            // Table
            {
                ulong handle = 1111;
                MockAnchorData anchorData = new MockAnchorData(handle, new Guid());
                anchorData.SceneLabel = PxrSemanticLabel.Table;
                anchorData.Position = new Vector3(1.39f, 1.17f, 0.05f);
                anchorData.Rotation = new Quaternion(0.04811f, 0.70547f, 0.70547f, -0.04811f);
                anchorData.SceneBox3DData = new SceneBox3DData()
                {
                    Center = new Vector3(0.00f, 0.00f, -0.56f),
                    Extent = new Vector3(0.84f, 1.28f, 1.13f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_roomEntityRoot.transform);
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
                anchorData.SceneLabel = PxrSemanticLabel.Floor;
                anchorData.Position = new Vector3(-0.19f, 0.04f, 0.62f);
                anchorData.Rotation = new Quaternion(0.69893f, -0.10720f, -0.10720f, -0.69893f);
                anchorData.ScenePolygonData = new ScenePolygonData()
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
                transform.SetParent(_roomEntityRoot.transform);
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
                anchorData.SceneLabel = PxrSemanticLabel.VirtualWall;
                anchorData.Position = new Vector3(0.68f, 1.56f, 6.53f);
                anchorData.Rotation = new Quaternion(0.00000f, 0.95955f, 0.00000f, -0.28155f);
                anchorData.SceneBox2DData = new SceneBox2DData()
                {
                    Center = Vector3.zero,
                    Extent = new Vector2(10.20f, 3.04f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_roomEntityRoot.transform);
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
                anchorData.SceneLabel = PxrSemanticLabel.Wall;
                anchorData.Position = new Vector3(-6.11f, 1.56f, 1.92f);
                anchorData.Rotation = new Quaternion(0.00000f, 0.81299f, 0.00000f, 0.58228f);
                anchorData.SceneBox2DData = new SceneBox2DData()
                {
                    Center = Vector3.zero,
                    Extent = new Vector2(15.56f, 3.04f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_roomEntityRoot.transform);
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
                anchorData.SceneLabel = PxrSemanticLabel.Wall;
                anchorData.Position = new Vector3(4.33f, 1.56f, 2.41f);
                anchorData.Rotation = new Quaternion(0.00000f, 0.53535f, 0.00000f, -0.84463f);
                anchorData.SceneBox2DData = new SceneBox2DData()
                {
                    Center = Vector3.zero,
                    Extent = new Vector2(3.02f, 3.04f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_roomEntityRoot.transform);
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
                anchorData.SceneLabel = PxrSemanticLabel.Wall;
                anchorData.Position = new Vector3(-3.00f, 1.56f, -5.51f);
                anchorData.Rotation = new Quaternion(0.00000f, -0.00640f, 0.00000f, -0.99998f);
                anchorData.SceneBox2DData = new SceneBox2DData()
                {
                    Center = Vector3.zero,
                    Extent = new Vector2(11.24f, 3.04f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_roomEntityRoot.transform);
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
                anchorData.SceneLabel = PxrSemanticLabel.Wall;
                anchorData.Position = new Vector3(3.15f, 1.56f, -2.27f);
                anchorData.Rotation = new Quaternion(0.00000f, 0.64921f, 0.00000f, -0.76061f);
                anchorData.SceneBox2DData = new SceneBox2DData()
                {
                    Center = Vector3.zero,
                    Extent = new Vector2(6.72f, 3.04f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_roomEntityRoot.transform);
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
                anchorData.SceneLabel = PxrSemanticLabel.Ceiling;
                anchorData.Position = new Vector3(-0.19f, 3.09f, 0.62f);
                anchorData.Rotation = new Quaternion(-0.10720f, 0.69893f, -0.69893f, -0.10720f);
                anchorData.ScenePolygonData = new ScenePolygonData()
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
                transform.SetParent(_roomEntityRoot.transform);
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
                anchorData.SceneLabel = PxrSemanticLabel.Door;
                anchorData.Position = new Vector3(3.20f, 1.31f, -0.60f);
                anchorData.Rotation = new Quaternion(0.00000f, 0.63994f, 0.00000f, -0.76842f);
                anchorData.SceneBox2DData = new SceneBox2DData()
                {
                    Center = Vector3.zero,
                    Extent = new Vector2(1.09f, 2.44f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_roomEntityRoot.transform);
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
                anchorData.SceneLabel = PxrSemanticLabel.Sofa;
                anchorData.Position = new Vector3(-0.24f, 1.02f, -0.74f);
                anchorData.Rotation = new Quaternion(0.53312f, 0.46453f, 0.46453f, -0.53312f);
                anchorData.SceneBox3DData = new SceneBox3DData()
                {
                    Center = new Vector3(0.00f, 0.00f, -0.47f),
                    Extent = new Vector3(0.46f, 0.55f, 0.95f)
                };
                GameObject gameObject = new GameObject($"MockRoomEntity: {anchorData.SceneLabel}-{handle}");
                var transform = gameObject.transform;
                transform.SetParent(_roomEntityRoot.transform);
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
            
            if (_gameEntityRoot == null)
            {
                _gameEntityRoot = new GameObject("GoomEntities");
            }
            return UniTask.CompletedTask;
        }

        public UniTask SaveGameEntities()
        {
            IList<UUID2ItemId> dataList = new List<UUID2ItemId>();
            foreach (var gameEntity in GameEntities)
            {
                var item = gameEntity.GameObject.GetComponent<IItem>();
                var data = new UUID2ItemId
                {
                    Uuid = gameEntity.AnchorData.Uuid,
                    ItemId = item.Id,
                    ItemState = (item.ItemState==null)?ItemState.Normal:item.ItemState
                };
                dataList.Add(data);
            }
            _persistentLoader.StageAllItemData(dataList);
            Debug.unityLogger.Log(TAG, $"Start Save Game Anchors");
            var output = GameEntities.Select(x => x.AnchorData).ToList();
            Debug.unityLogger.Log(TAG, $"Finished Save Game Anchors");
            return UniTask.CompletedTask;
        }

        public UniTask ClearGameEntities()
        {
            foreach (var gameEntity in GameEntities)
            {
                Object.Destroy(gameEntity.GameObject);
            }
            GameEntities.Clear();
            return UniTask.CompletedTask;
        }

        public async UniTask<IEntity> CreateAndAddEntity(GameObject gameObject)
        {
            var entity = await CreateEntity(gameObject);
            GameEntities.Add(entity);
            return entity;
        }
        
        
        private async UniTask<IEntity> CreateEntity(GameObject gameObject)
        {
            var transform = gameObject.transform;
            ulong handle = 8888888;
            MockAnchorData anchorData = new MockAnchorData(handle, new Guid());
            anchorData.SceneLabel = PxrSemanticLabel.Unknown;
            anchorData.Position = transform.position;
            anchorData.Rotation = transform.rotation;
            transform.SetParent(_gameEntityRoot.transform);
            IEntity entity = new Entity.Entity()
            {
                AnchorData = anchorData,
                GameObject = gameObject,
                IsRoomEntity = false,
            };
            Debug.unityLogger.Log(TAG, $"Create Entity, uuid: {entity.AnchorData.Uuid}, handle: {entity.AnchorData.Handle}");
            return entity;
        }
        
        public void DeleteEntity(IEntity entity)
        {
            GameEntities.Remove(entity);
            Object.Destroy(entity.GameObject);
        }

        public IList<IEntity> GetRoomEntities()
        {
            return RoomEntities;
        }

        public IList<IEntity> GetGameEntities()
        {
            return GameEntities;
        }

        public Transform GetRoomEntityRoot()
        {
            return _roomEntityRoot.transform;
        }
        
        public Transform GetGameEntityRoot()
        {
            return _gameEntityRoot.transform;
        }
        
        public void SetGameEntityRootVisiable(bool isVisible)
        {
            _gameEntityRoot.SetActive(isVisible);
        }
        
        public void SetRoomEntityRootVisiable(bool isVisible)
        {
            _roomEntityRoot.SetActive(isVisible);
        }

        public IList<IEntity> GetRoomEntities(PxrSemanticLabel label)
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
        
        public async UniTask CheckSceneAnchorUpdate()
        {
            
        }
        public async UniTask CheckSpatialAnchorUpdate()
        {
            
        }

        public async UniTask UpdateSpatialAnchorPosition()
        {
            
        }
        
    }
}