/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace PicoMRDemo.Runtime.Data.Decoration
{
    [Serializable]
    public class PersistentData
    {
        [JsonProperty("anchorData")]
        public UUID2ItemId[] AnchorData;
        [JsonProperty("themeData")] 
        public ThemeData[] ThemeDatas;
    }
    public class PersistentLoader : IPersistentLoader
    {
        private Dictionary<Guid, UUID2ItemId> _uuid2ItemId = new Dictionary<Guid, UUID2ItemId>();

        private Dictionary<DecorationType, DecorationData> _decorationDatas =
            new Dictionary<DecorationType, DecorationData>();

        private readonly string TAG = nameof(PersistentLoader);
        private readonly string DataPath = Path.Combine(Application.persistentDataPath, "uuid.json");
        
        public bool TryGetItemInfo(Guid uuid, out UUID2ItemId itemInfo)
        {
            return _uuid2ItemId.TryGetValue(uuid, out itemInfo);
        }

        public IList<DecorationData> GetAllThemeDatas()
        {
            return _decorationDatas.Select(x => x.Value).ToList();
        }

        public async UniTask LoadAllData()
        {
            Debug.unityLogger.Log(TAG, $"Load json file from DataPath: {DataPath}");
            if (File.Exists(DataPath))
            {
                Debug.unityLogger.Log(TAG, "Read json begin");
                _uuid2ItemId.Clear();
                _decorationDatas.Clear();
                var uuidDataJson = await File.ReadAllTextAsync(DataPath);
                Debug.unityLogger.Log(TAG, "Read json "+uuidDataJson);
                var persistentData = JsonConvert.DeserializeObject<PersistentData>(uuidDataJson);
                if (persistentData != null)
                {
                    var anchorData = persistentData.AnchorData;
                    if (anchorData is not null)
                    {
                        foreach (var uuid2ItemId in anchorData)
                        {
                            Debug.unityLogger.Log(TAG, $"Read json begin _uuid2ItemId {uuid2ItemId.Uuid} uuid2ItemId {uuid2ItemId}");
                            _uuid2ItemId.Add(uuid2ItemId.Uuid, uuid2ItemId);
                        }
                    }
                    else
                    {
                        Debug.unityLogger.Log(TAG, $"Read json anchorData is null");
                    }

                    var themeDatas = persistentData.ThemeDatas;
                    if (themeDatas is not null)
                    {
                        foreach (var themeData in themeDatas)
                        {
                            var decorationData = new DecorationData()
                            {
                                Type = themeData.Type,
                                ID = themeData.Id
                            };
                            Debug.unityLogger.Log(TAG, $"Read json begin decorationData {themeData.Type} decorationData {decorationData}");
                            _decorationDatas.Add(themeData.Type, decorationData);
                        }
                    }
                }
                Debug.unityLogger.Log(TAG, "Read json finished");
            }
            else
            {
                Debug.unityLogger.Log(TAG, $"uuid data is empty");
            }
        }

        
        public void StageAllItemData(IList<UUID2ItemId> data)
        {
            _uuid2ItemId.Clear();
            foreach (var uuid2ItemId in data)
            {
                _uuid2ItemId.Add(uuid2ItemId.Uuid, uuid2ItemId);
            }
        }

        public void StageAllThemeData(IList<DecorationData> decorationData)
        {
            _decorationDatas.Clear();
            foreach (var data in decorationData)
            {
                _decorationDatas.Add(data.Type, data);
            }
        }
        public async UniTask SaveAllData()
        {
            var persistentData = new PersistentData()
            {
                AnchorData = _uuid2ItemId.Select(x=>x.Value).ToArray(),
                ThemeDatas = _decorationDatas.Select(x => new ThemeData()
                {
                    Type = x.Value.Type,
                    Id = x.Value.ID
                }).ToArray()
            };
            
            Debug.unityLogger.Log(TAG, $"PersistentData: AnchorData.Length: {persistentData.AnchorData.Length}");
            
            string json = JsonConvert.SerializeObject(persistentData);

            Debug.unityLogger.Log(TAG, $"Start Write PersistentData, DataPath: {DataPath}");
            Debug.unityLogger.Log(TAG, $"Start Write Json: {json}");
            await File.WriteAllTextAsync(DataPath, json);
            Debug.unityLogger.Log(TAG, $"End Write PersistentData");
        }

        public UniTask DeleteAllData()
        {
            Debug.unityLogger.Log(TAG, $"DataPath: {DataPath}");
            if (File.Exists(DataPath))
            {
                File.Delete(DataPath);
            }

            return UniTask.CompletedTask;
        }

        public void ClearAllItemData()
        {
            _uuid2ItemId.Clear();
        }

        public void ClearAllThemeData()
        {
            _decorationDatas.Clear();
        }
    }
}