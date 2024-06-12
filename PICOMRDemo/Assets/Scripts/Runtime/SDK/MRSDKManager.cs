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
using Cysharp.Threading.Tasks;
using PicoMRDemo.Runtime.Data.Anchor;
using Unity.XR.PXR;
using UnityEngine;

namespace PicoMRDemo.Runtime.Runtime.SDK
{
    public class MRSDKManager : IMRSDKManager
    {
        private readonly string TAG = nameof(MRSDKManager);
        #region Task and flag

        // Load Room Anchor
        private bool _isLoadingRoomAnchors = false;
        private ulong _loadRoomAnchorTaskId;
        private IList<IAnchorData> _roomAnchors = new List<IAnchorData>();
        
        // Load Game Anchor
        private bool _isLoadingGameAnchors = false;
        private ulong _loadGameAnchorTaskId;
        private IList<IAnchorData> _gameAnchors = new List<IAnchorData>();
        
        // Save Game Anchor
        private bool _isSavingGameAnchors = false;
        private ulong _savingGameAnchorTaskId;
        
        // Create Game Anchor
        private Dictionary<ulong, PxrEventAnchorEntityCreated?> CreateSingleAnchorTask = new Dictionary<ulong, PxrEventAnchorEntityCreated?>();
        
        // Delete Game Anchor
        private Dictionary<ulong, PxrEventAnchorEntityUnPersisted?> DeleteSingleAnchorTask = new Dictionary<ulong, PxrEventAnchorEntityUnPersisted?>();
        
        // Delete All Game Anchor
        private bool _isDeleteAllGameAnchors = false;
        private ulong _deleteAllGameAnchorsTaskId;
        

        #endregion

        public MRSDKManager()
        {
            PXR_Manager.AnchorEntityCreated += DoAnchorCreated;
            PXR_Manager.AnchorEntityUnPersisted += DoAnchorDeleted;
        }

        public async UniTask<IList<IAnchorData>> LoadRoomAnchors()
        {
            if (_isLoadingRoomAnchors)
                return null;
            _roomAnchors.Clear();
            PXR_Manager.AnchorEntityLoaded += DoLoadRoomAnchors;
            PxrSpatialSceneDataTypeFlags[] flags = { PxrSpatialSceneDataTypeFlags.Ceiling, PxrSpatialSceneDataTypeFlags.Door, PxrSpatialSceneDataTypeFlags.Floor, PxrSpatialSceneDataTypeFlags.Opening,
                PxrSpatialSceneDataTypeFlags.Window,PxrSpatialSceneDataTypeFlags.Wall,PxrSpatialSceneDataTypeFlags.Object };
            PXR_MixedReality.LoadAnchorEntityBySceneFilter(flags, out var taskId);
            _isLoadingRoomAnchors = true;
            _loadRoomAnchorTaskId = taskId;

            while (_isLoadingRoomAnchors)
            {
                await UniTask.Yield();
            }

            PXR_Manager.AnchorEntityLoaded -= DoLoadRoomAnchors;
            return _roomAnchors;
        }

        public async UniTask<IList<IAnchorData>> LoadGameAnchors()
        {
            if (_isLoadingGameAnchors)
            {
                return null;
            }
            _gameAnchors.Clear();
            PXR_Manager.AnchorEntityLoaded += DoLoadGameAnchors;
            PXR_MixedReality.LoadAnchorEntityByUuidFilter(out var taskId);
            _isLoadingGameAnchors = true;
            _loadGameAnchorTaskId = taskId;

            while (_isLoadingGameAnchors)
            {
                await UniTask.Yield();
            }
            PXR_Manager.AnchorEntityLoaded -= DoLoadGameAnchors;

            return _gameAnchors;
        }

        public async UniTask SaveGameAnchors(IList<IAnchorData> anchorDatas)
        {
            if (anchorDatas.Count <= 0)
                return;
            if (_isSavingGameAnchors)
                return;
            PXR_Manager.AnchorEntityPersisted += DoSavedGameAnchors;
            ulong[] handleList = anchorDatas.Select(x => x.Handle).ToArray();
            PXR_MixedReality.PersistAnchorEntity(handleList, PxrPersistLocation.Local, out var taskId);
            _savingGameAnchorTaskId = taskId;
            _isSavingGameAnchors = true;

            while (_isSavingGameAnchors)
            {
                await UniTask.Yield();
            }
            PXR_Manager.AnchorEntityPersisted -= DoSavedGameAnchors;
        }

        public async UniTask<IAnchorData> CreateAnchor(Transform transform)
        {
            var result = PXR_MixedReality.CreateAnchorEntity(transform.position, transform.rotation, out var taskId);
            
            CreateSingleAnchorTask.Add(taskId, null);

            while (CreateSingleAnchorTask[taskId] == null)
            {
                await UniTask.Yield();
            }

            var createdArgs = CreateSingleAnchorTask[taskId];
            CreateSingleAnchorTask.Remove(taskId);

            var anchorData = new AnchorData(createdArgs.Value.anchorHandle, createdArgs.Value.uuid);
            Debug.unityLogger.Log(TAG, $"Create Anchor, uuid: {anchorData.Uuid}, handle: {anchorData.Handle}");
            return anchorData;
        }

        public async UniTask DeleteAnchor(IAnchorData anchorData)
        {
            var result = PXR_MixedReality.DestroyAnchorEntity(anchorData.Handle);
            if (result == PxrResult.SUCCESS)
            {
                Debug.unityLogger.Log(TAG, "PXR_MRSample Destroy spatial anchor succeed with anchorHandle " + anchorData.Handle);
            }
            else
            {
                Debug.unityLogger.Log(TAG, "PXR_MRSample Destroy spatial anchor failed with result:" + result);
            }

            ulong[] anchors = { anchorData.Handle };
            PXR_MixedReality.UnPersistAnchorEntity(anchors, PxrPersistLocation.Local, out var taskId);
            DeleteSingleAnchorTask.Add(taskId, null);
            
            while (DeleteSingleAnchorTask[taskId] == null)
            {
                await UniTask.Yield();
            }

            var deleteTask = DeleteSingleAnchorTask[taskId];
            DeleteSingleAnchorTask.Remove(taskId);

            Debug.unityLogger.Log(TAG, $"Delete Anchor, uuid: {anchorData.Uuid}, handle: {anchorData.Handle}");
        }

        public async UniTask<IList<IAnchorData>> CreateAnchors(IList<Transform> transforms)
        {
            IList<UniTask<IAnchorData>> createAnchorTasks = new List<UniTask<IAnchorData>>();
            IList<IAnchorData> newAnchorDatas = new List<IAnchorData>();
            foreach (var transform in transforms)
            {
                var anchorTask = CreateAnchor(transform);
                createAnchorTasks.Add(anchorTask);
            }

            await UniTask.WhenAll(createAnchorTasks);
            foreach (var anchorTask in createAnchorTasks)
            {
                newAnchorDatas.Add(anchorTask.GetAwaiter().GetResult());
            }

            return newAnchorDatas;
        }

        public async UniTask DeleteAnchors(IList<IAnchorData> anchorDatas)
        {
            foreach (var anchorData in anchorDatas)
            {
                var result = PXR_MixedReality.DestroyAnchorEntity(anchorData.Handle);
                if (result != PxrResult.SUCCESS)
                {
                    Debug.unityLogger.Log(TAG, $"Delete anchor: {anchorData.Handle} error");
                }
            }

            ulong[] anchors = anchorDatas.Select(x => x.Handle).ToArray();
            PXR_MixedReality.UnPersistAnchorEntity(anchors, PxrPersistLocation.Local, out var taskId);
            DeleteSingleAnchorTask.Add(taskId, null);
            
            while (DeleteSingleAnchorTask[taskId] == null)
            {
                await UniTask.Yield();
            }

            var deleteTask = DeleteSingleAnchorTask[taskId];
            DeleteSingleAnchorTask.Remove(taskId);
            Debug.unityLogger.Log(TAG, $"Delete Anchors Finished");
        }

        public async UniTask DeleteAllAnchors()
        {
            if (_isDeleteAllGameAnchors)
                return ;
            PXR_MixedReality.ClearPersistedAnchorEntity(PxrPersistLocation.Local, out var taskId);
            _deleteAllGameAnchorsTaskId = taskId;
            _isDeleteAllGameAnchors = true;
            PXR_Manager.AnchorEntityCleared += DoDeleteAllGameAnchors;
            while (_isDeleteAllGameAnchors)
            {
                await UniTask.Yield();
            }
            PXR_Manager.AnchorEntityCleared -= DoDeleteAllGameAnchors;
        }


        #region Do Event

        private void DoLoadRoomAnchors(PxrEventAnchorEntityLoaded result)
        {
            var taskId = result.taskId;
            if (taskId != _loadRoomAnchorTaskId)
                return;
            PXR_MixedReality.GetAnchorEntityLoadResults(taskId, result.count, out var loadedAnchors);
            _roomAnchors.Clear();
            foreach (var anchor in loadedAnchors)
            {
                Debug.unityLogger.Log(TAG, $"Load Room Anchor Key: {anchor.Key}, Guid: {anchor.Value}");
                IAnchorData anchorData = new AnchorData(anchor.Key, anchor.Value);
                _roomAnchors.Add(anchorData);
            }
            _isLoadingRoomAnchors = false;
        }

        private void DoLoadGameAnchors(PxrEventAnchorEntityLoaded result)
        {
            var taskId = result.taskId;
            if (taskId != _loadGameAnchorTaskId)
                return;
            PXR_MixedReality.GetAnchorEntityLoadResults(taskId, result.count, out var loadedAnchors);
            Debug.unityLogger.Log(TAG, $"result: {result.result}, count: {result.count}, loadedAnchors: {loadedAnchors.Count}");
            _gameAnchors.Clear();
            foreach (var anchor in loadedAnchors)
            {
                Debug.unityLogger.Log(TAG, $"Load Game Anchor Key: {anchor.Key}, Guid: {anchor.Value}");
                IAnchorData anchorData = new AnchorData(anchor.Key, anchor.Value);
                _gameAnchors.Add(anchorData);
            }
            _isLoadingGameAnchors = false;
        }

        private void DoSavedGameAnchors(PxrEventAnchorEntityPersisted result)
        {
            Debug.unityLogger.Log(TAG, $"Save All Game Anchor result: {result}");
            _isSavingGameAnchors = false;
        }
        
        private void DoAnchorCreated(PxrEventAnchorEntityCreated result)
        {
            if (result.result != PxrResult.SUCCESS || !CreateSingleAnchorTask.ContainsKey(result.taskId))
            {
                Debug.unityLogger.LogError(TAG, $"Create Anchor error, taskId: {result.taskId}");
                return;
            }

            CreateSingleAnchorTask[result.taskId] = result;
        }
        
        private void DoAnchorDeleted(PxrEventAnchorEntityUnPersisted result)
        {
            if (result.result != PxrResult.SUCCESS || !CreateSingleAnchorTask.ContainsKey(result.taskId))
            {
                Debug.unityLogger.LogError(TAG, $"Create Anchor error, taskId: {result.taskId}");
                return;
            }

            DeleteSingleAnchorTask[result.taskId] = result;
        }
        
        private void DoDeleteAllGameAnchors(PxrEventAnchorEntityCleared result)
        {
            Debug.unityLogger.Log(TAG, $"Clear All Game Anchor: {result}");
            _isDeleteAllGameAnchors = false;
        }

        #endregion
    }
}