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
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Runtime.SDK;
using PicoMRDemo.Runtime.Data.Anchor;
using PicoMRDemo.Runtime.Game;
using Unity.XR.PXR;
using UnityEngine;
using VContainer;
using Object = UnityEngine.Object;

namespace PicoMRDemo.Runtime.Entity
{
    public class EntityManager : IEntityManager
    {
        private readonly string TAG = nameof(EntityManager);

        private bool _needUpdateGameEntities = false;
        private IList<IEntity> _gameEntities = new List<IEntity>();

        private bool _needUpdateRoomEntities = false;
        private readonly IList<IEntity> _allRoomEntities = new List<IEntity>();

        private readonly IDictionary<PxrSemanticLabel, IList<IEntity>> _roomEntities =
            new Dictionary<PxrSemanticLabel, IList<IEntity>>();

        private readonly GameObject _roomEntityRoot;
        private readonly GameObject _gameEntityRoot;
        
        [Inject]
        private IMRSDKManager _mrsdkManager;

        [Inject]
        private IPersistentLoader _persistentLoader;

        [Inject]
        private IItemFactory _itemFactory;
        
        EntityManager()
        {
            _roomEntityRoot = new GameObject("RoomEntities");
            _gameEntityRoot = new GameObject("GameEntities");
        }

        public async UniTask LoadRoomEntities()
        {   
            Debug.unityLogger.Log(TAG, "Begin Load Room Anchors");
            await ClearRoomEntities();
            PXR_Manager.SceneAnchorDataUpdated += DoSceneAnchorDataUpdated;
            var anchors = await _mrsdkManager.LoadRoomAnchors();
            Debug.unityLogger.Log(TAG, $"Load Room Anchors Finished, total anchors: {anchors.Count}");
            
            
            foreach (var anchor in anchors)
            {
                //Debug.unityLogger.Log(TAG, $"Load Room Anchor Key: {anchor.Handle}, Guid: {anchor.Uuid} SceneLabel: {anchor.SceneLabel}");
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
            PXR_Manager.SpatialAnchorDataUpdated  += DoSpatialAnchorDataUpdated;
            var anchors = await _mrsdkManager.LoadGameAnchors();
            Debug.unityLogger.Log(TAG, $"Load Game Anchors Finished, total anchors: {anchors.Count}");
            _gameEntities.Clear();
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
            Debug.unityLogger.Log(TAG, $"Start Save Game Anchors "+ dataList.Count);
            await _mrsdkManager.SaveGameAnchorsToLocal(_gameEntities.Select(x => x.AnchorData).ToList());
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
        
        public async UniTask ClearRoomEntities()
        {
            foreach (var roomEntity in _allRoomEntities)
            {
                Object.Destroy(roomEntity.GameObject);
            }
            _allRoomEntities.Clear();
            _roomEntities.Clear();
            PXR_Manager.SceneAnchorDataUpdated -= DoSceneAnchorDataUpdated;
        }

        private async UniTask ClearGameEntitiesAnchor()
        {
            await _mrsdkManager.DeleteAllAnchors();
        }
        

        private async UniTask<IEntity> CreateEntity(GameObject gameObject)
        {
            var transform = gameObject.transform;
            var anchorData = await _mrsdkManager.CreateAnchor(transform);
            if (anchorData != null)
            {
                var entity = new Entity
                {
                    AnchorData = anchorData,
                    GameObject = gameObject
                };
                Debug.unityLogger.Log(TAG, $"Create Entity, uuid: {entity.AnchorData.Uuid}, handle: {entity.AnchorData.Handle} Position:({transform.position.x:F3}, {transform.position.y:F3}, {transform.position.z:F3})");
                return entity;
            }
            else
            {
                Debug.unityLogger.LogError(TAG, $"Create Entity anchorData == null");
                return null;
            }
        }

        public async UniTask<IEntity> CreateAndAddEntity(GameObject gameObject)
        {
            var entity = await CreateEntity(gameObject);
            _gameEntities.Add(entity);
            return entity;
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

        public IList<IEntity> GetRoomEntities(PxrSemanticLabel label)
        {
            if (!_roomEntities.TryGetValue(label, out var result))
            {
                result = new List<IEntity>();
            }
            return result;
        }
        public async UniTask CheckSceneAnchorUpdate()
        {
            if (_needUpdateRoomEntities)
            {
                var anchors = await _mrsdkManager.DoSceneAnchorDataUpdated();
                if (anchors != null)
                {
                    _needUpdateRoomEntities = false;
                    UpdateRoomEntities(anchors);
                    var roomService = App.Instance.GetRoomService();
                    roomService.RefreshRoomEntities();
                }
            }
        }
        
        public async UniTask CheckSpatialAnchorUpdate()
        {
            if (_needUpdateGameEntities)
            {
                //Debug.unityLogger.Log(TAG, $"CheckSpatialAnchorUpdate _needUpdateGameEntities == true");

                var anchors = await _mrsdkManager.DoSpatialAnchorDataUpdated();
                if (anchors != null)
                {
                    _needUpdateGameEntities = false;
                    UpdateGameEntities(anchors);
                }
            }
        }

        public async UniTask UpdateSpatialAnchorPosition()
        {
            var gameEntities = _gameEntities.ToArray();
            foreach (var entity in gameEntities)
            {
                var result = PXR_MixedReality.LocateAnchor(entity.AnchorData.Handle, out var position, out var rotation);
                //Debug.unityLogger.Log(TAG, $"Update Spatial Anchor Position Anchor Key: {entity.AnchorData.Handle}, Guid: {entity.AnchorData.Uuid} Position:({entity.GameObject.transform.position.x:F3}, {entity.GameObject.transform.position.y:F3}, {entity.GameObject.transform.position.z:F3} " +
                //                           $"TO Position:({position.x:F3}, {position.y:F3}, {position.z:F3}");
                if (result == PxrResult.SUCCESS)
                {
                    entity.GameObject.transform.position = position;
                    entity.GameObject.transform.rotation = rotation;
                }
            }
        }


        private void DoSceneAnchorDataUpdated()
        {
            Debug.unityLogger.Log($"Set DoSceneAnchorDataUpdated");
            _needUpdateRoomEntities = true;
        }

        private void DoSpatialAnchorDataUpdated()
        {
            Debug.unityLogger.Log($"Set DoSpatialAnchorDataUpdated");
            _needUpdateGameEntities = true;
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
        
        private void UpdateRoomEntities(IList<IAnchorData> anchors)
        {   
            Debug.unityLogger.Log($"Start UpdateRoomEntities");
            var allRoomEntities = _allRoomEntities.ToArray();
            foreach (var anchor in anchors)
            {
                if (allRoomEntities.FirstOrDefault(x => (x.AnchorData == anchor)) == null)
                {
                    //Debug.unityLogger.Log(TAG, $"Update Room Anchor Key: {anchor.Handle}, Guid: {anchor.Uuid}");
                
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
        }
        private void UpdateGameEntities(IList<IAnchorData> anchors)
        {   
            Debug.unityLogger.Log($"Start UpdateRoomEntities");
            var gameEntities = _gameEntities.ToArray();
            foreach (var anchor in anchors)
            {
                if (gameEntities.FirstOrDefault(x => (x.AnchorData == anchor)) == null)
                {
                    Debug.unityLogger.Log(TAG, $"Update Game Anchor Key: {anchor.Handle}, Guid: {anchor.Uuid}");
                    if (!_persistentLoader.TryGetItemInfo(anchor.Uuid, out var itemInfo))
                    {
                        Debug.unityLogger.LogWarning(TAG, $"Can't find Item, uuid: {anchor.Uuid}");
                        continue;
                    }
                    ulong itemId = itemInfo.ItemId;
                    var item = _itemFactory.CreateItem(itemId, anchor.Position, anchor.Rotation, itemInfo.ItemState);
                    GameObject gameObject = item.GameObject;
                    var transform = gameObject.transform;
                    transform.SetParent(_gameEntityRoot.transform);
                    transform.position = anchor.Position;
                    transform.rotation = anchor.Rotation;
                    IEntity entity = new Entity()
                    {
                        AnchorData = anchor,
                        GameObject = gameObject,
                        IsRoomEntity = false,
                    };
                    _gameEntities.Add(entity);
                }
            }
        }
    }
}