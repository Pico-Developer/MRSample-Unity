/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Runtime.SDK;
using Unity.XR.PXR;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace PicoMRDemo.Runtime.Entity
{
    public class EntityManager : IEntityManager
    {
        private readonly string TAG = nameof(EntityManager);

        private bool _isLoadingGameEntities = false;
        private ulong _loadGameEntitiesTaskId;
        private IList<IEntity> _gameEntities = new List<IEntity>();

        private bool _isLoadingRoomEntities = false;
        private ulong _loadRoomEntitiesTaskId;
        private readonly IList<IEntity> _allRoomEntities = new List<IEntity>();

        private bool _isClearGameEntitiesAnchor = false;
        private ulong _clearGameEntitiesAnchorTaskId;

        private bool _isSavingGameEntities = false;
        private ulong _savingGameEntitiesTaskId;

        private readonly IDictionary<PxrSceneLabel, IList<IEntity>> _roomEntities =
            new Dictionary<PxrSceneLabel, IList<IEntity>>();

        private GameObject _roomEntityRoot;
        private GameObject _gameEntityRoot;

        [Inject]
        private IMRSDKManager _mrsdkManager;

        [Inject]
        private IPersistentLoader _persistentLoader;

        [Inject]
        private IItemFactory _itemFactory;
        public void LoadEntity(PxrEventAnchorEntityLoaded entityLoadedResult)
        {
            PXR_MixedReality.GetAnchorEntityLoadResults(entityLoadedResult.taskId, entityLoadedResult.count, out var loadedAnchors);
            foreach (var anchor in loadedAnchors)
            {
                Debug.unityLogger.Log(TAG, $"Load Anchor Key: {anchor.Key}, Guid: {anchor.Value}");

                PXR_MixedReality.GetAnchorSceneLabel(anchor.Key, out var label);
                
                Debug.unityLogger.Log(TAG, $"label: {label}");
            }
        }

        public async UniTask LoadRoomEntities()
        {            
            var anchors = await _mrsdkManager.LoadRoomAnchors();
            _allRoomEntities.Clear();
            _roomEntities.Clear();
            if (_roomEntityRoot == null)
            {
                _roomEntityRoot = new GameObject("RoomEntities");
            }
            foreach (var anchor in anchors)
            {
                Debug.unityLogger.Log(TAG, $"Load Room Anchor Key: {anchor.Handle}, Guid: {anchor.Uuid}");
                
                GameObject gameObject = new GameObject($"RoomEntity: {anchor.Handle}");
                var transform = gameObject.transform;
                transform.SetParent(_roomEntityRoot.transform);
                transform.position = anchor.Position;
                transform.rotation = anchor.Rotation;
                IEntity entity = new Entity()
                {
                    AnchorData = anchor,
                    GameObject = gameObject,
                    IsRoomEntity = true,
                };
                _allRoomEntities.Add(entity);

                var label = entity.AnchorData.SceneLabel;
                if (_roomEntities.TryGetValue(label, out var entities))
                {
                    entities.Add(entity);
                }
                else
                {
                    entities = new List<IEntity>();
                    entities.Add(entity);
                    _roomEntities.Add(label, entities);
                }
            }
        }

        public async UniTask LoadGameEntities()
        {
            Debug.unityLogger.Log(TAG, "Begin Load Game Anchors");
            var anchors = await _mrsdkManager.LoadGameAnchors();
            Debug.unityLogger.Log(TAG, $"Load Game Anchors Finished, total anchors: {anchors.Count}");
            _gameEntities.Clear();
            if (_gameEntityRoot == null)
            {
                _gameEntityRoot = new GameObject("GameEntities");
            }
            foreach (var anchorData in anchors)
            {
                Debug.unityLogger.Log(TAG, $"Load Game Anchor Key: {anchorData.Handle}, Guid: {anchorData.Uuid}");
                if (!_persistentLoader.TryGetItemInfo(anchorData.Uuid, out var itemInfo))
                {
                    Debug.unityLogger.LogWarning(TAG, $"Can't find Item, uuid: {anchorData.Uuid}");
                    continue;
                }
                ulong itemId = itemInfo.ItemId;
                var item = _itemFactory.CreateItem(itemId, anchorData.Position, anchorData.Rotation, itemInfo.ItemState);
                GameObject gameObject = item.GameObject;
                gameObject.transform.SetParent(_gameEntityRoot.transform);
                IEntity entity = new Entity()
                {
                    AnchorData = anchorData,
                    GameObject = gameObject,
                    IsRoomEntity = false,
                };
                item.Entity = entity;
                item.EntityManager = this;
                _gameEntities.Add(entity);
            }
            
            
        }

        public async UniTask SaveGameEntities()
        {
            Debug.unityLogger.Log(TAG, $"Start Update Anchor");
            await UpdateAnchor();
            Debug.unityLogger.Log(TAG, $"Finish Update Anchor");
            IList<UUID2ItemId> dataList = new List<UUID2ItemId>();
            foreach (var gameEntity in _gameEntities)
            {
                var item = gameEntity.GameObject.GetComponent<IItem>();
                var data = new UUID2ItemId
                {
                    Uuid = gameEntity.AnchorData.Uuid,
                    ItemId = item.Id,
                    ItemState = item.ItemState
                };
                dataList.Add(data);
            }
            _persistentLoader.StageAllItemData(dataList);
            Debug.unityLogger.Log(TAG, $"Start Save Game Anchors");
            await _mrsdkManager.SaveGameAnchors(_gameEntities.Select(x => x.AnchorData).ToList());
            Debug.unityLogger.Log(TAG, $"Finished Save Game Anchors");
        }
        
        public async UniTask ClearGameEntities()
        {
            await ClearGameEntitiesAnchor();
            foreach (var gameEntity in _gameEntities)
            {
                Object.Destroy(gameEntity.GameObject);
            }
            _gameEntities.Clear();
            _persistentLoader.ClearAllItemData();
        }

        private async UniTask ClearGameEntitiesAnchor()
        {
            await _mrsdkManager.DeleteAllAnchors();
        }

        private async UniTask UpdateAnchor()
        {
            await ClearGameEntitiesAnchor();
            Debug.unityLogger.Log(TAG, $"Clear Old Anchor Finished");
            IList<IEntity> newEntities = await CreateEntity(_gameEntities.Where(x=>x.GameObject.activeSelf).Select(x => x.GameObject).ToList());
            _gameEntities.Clear();
            _gameEntities = newEntities;
            Debug.unityLogger.Log(TAG, $"Update new Anchor Finished");
        }

        public async UniTask<IEntity> CreateEntity(GameObject gameObject)
        {
            var transform = gameObject.transform;
            var anchorData = await _mrsdkManager.CreateAnchor(transform);
            var entity = new Entity
            {
                AnchorData = anchorData,
                GameObject = gameObject
            };
            Debug.unityLogger.Log(TAG, $"Create Entity, uuid: {entity.AnchorData.Uuid}, handle: {entity.AnchorData.Handle}");
            return entity;
        }

        public async UniTask<IEntity> CreateAndAddEntity(GameObject gameObject)
        {
            var entity = await CreateEntity(gameObject);
            _gameEntities.Add(entity);
            return entity;
        }

        private async UniTask<IList<IEntity>> CreateEntity(IList<GameObject> gameObjects)
        {
            IList<UniTask<IEntity>> createEntityTasks = new List<UniTask<IEntity>>();
            IList<IEntity> newEntities = new List<IEntity>();
            foreach (var gameObject in gameObjects)
            {
                var entityTask = CreateEntity(gameObject);
                createEntityTasks.Add(entityTask);
            }

            await UniTask.WhenAll(createEntityTasks);
            foreach (var entityTask in createEntityTasks)
            {
                newEntities.Add(entityTask.GetAwaiter().GetResult());
            }

            return newEntities;
        }

        public void DeleteEntity(IEntity entity)
        {
            _mrsdkManager.DeleteAnchor(entity.AnchorData);
            Object.Destroy(entity.GameObject);
        }

        public IList<IEntity> GetRoomEntities()
        {
            return _allRoomEntities;
        }

        public IList<IEntity> GetGameEntities()
        {
            return _gameEntities;
        }

        public IList<IEntity> GetRoomEntities(PxrSceneLabel label)
        {
            IList<IEntity> result = null;
            if (!_roomEntities.TryGetValue(label, out result))
            {
                result = new List<IEntity>();
            }
            return result;
        }
    }
}