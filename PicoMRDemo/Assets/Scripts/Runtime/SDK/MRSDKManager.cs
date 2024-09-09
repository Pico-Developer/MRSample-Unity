/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
        private IList<IAnchorData> _roomAnchors = new List<IAnchorData>();
        
        // Load Game Anchor
        private bool _isLoadingGameAnchors = false;
        private IList<IAnchorData> _gameAnchors = new List<IAnchorData>();
        
        #endregion
        
        public async UniTask<IList<IAnchorData>> LoadRoomAnchors()
        {
            _roomAnchors.Clear();
            _isLoadingRoomAnchors = true;
            var result = await PXR_MixedReality.QuerySceneAnchorAsync(default);
            Debug.unityLogger.Log($"LoadSceneDataAsync: {result.anchorDictionary.Count} ,{result.result}");
            if (result.result == PxrResult.SUCCESS)
            {
                if (result.anchorDictionary.Count > 0)
                {
                    foreach (var item in result.anchorDictionary)
                    {
                        IAnchorData anchorData = new AnchorData(item.Key, item.Value);
                        _roomAnchors.Add(anchorData);
                    }
                }
            }
            _isLoadingRoomAnchors = false;
            return _roomAnchors;
        }

        public async UniTask<IList<IAnchorData>> LoadGameAnchors()
        {
            _gameAnchors.Clear();
            _isLoadingGameAnchors = true;
            var result = await PXR_MixedReality.QuerySpatialAnchorAsync();
            Debug.unityLogger.Log($"LoadSpatialAnchorAsync: {result.result} , {result.anchorHandleList.Count}");
            if (result.result == PxrResult.SUCCESS)
            {
                if (result.anchorHandleList.Count > 0)
                {
                    
                    foreach (var anchor in result.anchorHandleList)
                    {
                        var result1 = PXR_MixedReality.GetAnchorUuid(anchor, out var uuid);
                        Debug.unityLogger.Log(TAG, $"Load Game Anchor Key: {anchor}, Guid: {uuid}");
                        IAnchorData anchorData = new AnchorData(anchor, uuid);
                        _gameAnchors.Add(anchorData);
                    }
                }
            }
            _isLoadingGameAnchors = false;
            return _gameAnchors;
        }

        public async UniTask SaveGameAnchorsToLocal(IList<IAnchorData> anchorDatas)
        {
            Debug.unityLogger.Log(TAG, $"Start SaveGameAnchorsToLocal "+ anchorDatas.Count);
            if (anchorDatas.Count <= 0)
                return;
            ulong[] handleList = anchorDatas.Select(x => x.Handle).ToArray();
            foreach (var anchor in handleList)
            {
                Debug.unityLogger.Log(TAG, $"Start SaveGameAnchorsToLocal anchor "+ anchor);
                await PXR_MixedReality.PersistSpatialAnchorAsync(anchor);
            }
        }
        
        public async UniTask ClearGameAnchorsToLocal(IList<IAnchorData> anchorDatas)
        {
            if (anchorDatas.Count <= 0)
                return;
            ulong[] handleList = anchorDatas.Select(x => x.Handle).ToArray();
            foreach (var anchor in handleList)
            {
                Debug.unityLogger.Log(TAG, $"Start ClearGameAnchorsToLocal anchor "+ anchor);

                await PXR_MixedReality.UnPersistSpatialAnchorAsync(anchor);
            }
        }
        

        public async UniTask<IAnchorData> CreateAnchor(Transform transform)
        {
            var result = await PXR_MixedReality.CreateSpatialAnchorAsync(transform.position, transform.rotation);
            //Debug.unityLogger.Log( $"CreateSpatialAnchorAsync: {result.ToString()} Position:({transform.position.x:F3}, {transform.position.y:F3}, {transform.position.z:F3})");

            if (result.result == PxrResult.SUCCESS)
            {
                var anchorData = new AnchorData(result.anchorHandle, result.uuid);
                Debug.unityLogger.Log(TAG, $"Create Anchor, uuid: {anchorData.Uuid}, handle: {anchorData.Handle}");
                _gameAnchors.Add(anchorData);
                return anchorData;
            }

            return null;
        }

        public async UniTask DeleteAnchor(IAnchorData anchorData)
        {
            var result = PXR_MixedReality.DestroyAnchor(anchorData.Handle);
            if (result == PxrResult.SUCCESS)
            {
                Debug.unityLogger.Log(TAG, "PXR_MRSample Destroy spatial anchor succeed with anchorHandle " + anchorData.Handle);
            }
            else
            {
                Debug.unityLogger.Log(TAG, "PXR_MRSample Destroy spatial anchor failed with result:" + result);
            }
            Debug.unityLogger.Log(TAG, $"Start DeleteAnchor anchor "+ anchorData.Handle);

            await PXR_MixedReality.UnPersistSpatialAnchorAsync(anchorData.Handle);
            Debug.unityLogger.Log(TAG, $"Delete Anchor, uuid: {anchorData.Uuid}, handle: {anchorData.Handle}");
        }

        public async UniTask DeleteAllAnchors()
        {
            var result = await PXR_MixedReality.QuerySpatialAnchorAsync();
            Debug.unityLogger.Log($"LoadSpatialAnchorAsync: {result.result}");
            if (result.result == PxrResult.SUCCESS)
            {
                ulong[] handleList = result.anchorHandleList.ToArray();
                foreach (var anchor in handleList)
                {
                    Debug.unityLogger.Log(TAG, $"Start DeleteAllAnchors anchor "+ anchor);

                    await PXR_MixedReality.UnPersistSpatialAnchorAsync(anchor);
                }
            }
        }
        
        public async UniTask<IList<IAnchorData>> DoSceneAnchorDataUpdated()
        {
            
            Debug.unityLogger.Log($"Call DoSceneAnchorDataUpdated");
            if (_isLoadingRoomAnchors)
                return null;
            Debug.unityLogger.Log($"Start DoSceneAnchorDataUpdated");
            _isLoadingRoomAnchors = true;
            var result = await PXR_MixedReality.QuerySceneAnchorAsync(default);
            Debug.unityLogger.Log($"DoSceneAnchorDataUpdated: {result.anchorDictionary.Count}");
            
            if (result.result == PxrResult.SUCCESS)
            {
                if (result.anchorDictionary.Count > 0)
                {
                    foreach (var item in result.anchorDictionary)
                    {
                        IAnchorData anchorData = new AnchorData(item.Key, item.Value);
                        if (!_roomAnchors.Contains(anchorData))
                        {
                            _roomAnchors.Add(anchorData);
                        }
                    }
                }
            }
            _isLoadingRoomAnchors = false;
            return _roomAnchors;
        }
        
        public async UniTask<IList<IAnchorData>> DoSpatialAnchorDataUpdated()
        {
            
            Debug.unityLogger.Log($"Call DoSpatialAnchorDataUpdated");
            if (_isLoadingGameAnchors)
                return null;
            Debug.unityLogger.Log($"Start DoSpatialAnchorDataUpdated");
            _isLoadingGameAnchors = true;
            var result = await PXR_MixedReality.QuerySpatialAnchorAsync();
            Debug.unityLogger.Log($"DoSpatialAnchorDataUpdated: {result.anchorHandleList.Count}");
            
            if (result.result == PxrResult.SUCCESS)
            {
                if (result.anchorHandleList.Count > 0)
                {
                    foreach (var anchor in result.anchorHandleList)
                    {
                        var result1 = PXR_MixedReality.GetAnchorUuid(anchor, out var uuid);
                        IAnchorData anchorData = new AnchorData(anchor, uuid);
                        if (!_gameAnchors.Contains(anchorData))
                        {
                            _gameAnchors.Add(anchorData);
                        }
                    }
                }
            }
            _isLoadingGameAnchors = false;
            return _gameAnchors;
        }
    }
}