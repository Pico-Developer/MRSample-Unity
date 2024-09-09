/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Linq;
using PicoMRDemo.Runtime.Data.Config;
using PicoMRDemo.Runtime.Service;
using UnityEngine;
using VContainer;

namespace PicoMRDemo.Runtime.Runtime.PresetDecoration
{
    public class PresetDecorationData
    {
        public GameObject GameObject;
        public PresetType PresetType;
    }
    public class PresetDecorationManager : IPresetDecorationManager
    {
        [Inject]
        private IPresetDecorationLoader _presetDecorationLoader;
        
        private IList<PresetRowConfig> _presetRowConfigs;

        private IAssetConfig _assetConfig;

        private Dictionary<PresetType, IList<GameObject>> _presetDecorationsDict = new Dictionary<PresetType, IList<GameObject>>();
        
        public IList<PresetRowConfig> PresetRowConfigs
        {
            get
            {
                if (_presetRowConfigs == null || _assetConfig == null)
                {
                    var assetConfig = _presetDecorationLoader.LoadPresetDecoration() as AssetConfig<GameObject>;
                    _assetConfig = assetConfig;
                    if (assetConfig != null)
                    {
                        var allAssetConfig = assetConfig.GetAllAssetConfig();
                        _presetRowConfigs = allAssetConfig.Select(x => x as PresetRowConfig).ToList();
                    }
                }

                return _presetRowConfigs;
            }
        }

        private Dictionary<PresetDecorationData, GameObject> _decToWall = new Dictionary<PresetDecorationData, GameObject>();
        public GameObject CreateDecoration(PresetType type, ulong themeId, GameObject wall)
        {
            foreach (var presetRowConfig in PresetRowConfigs)
            {
                if (presetRowConfig.Type == type && presetRowConfig.ThemeId == themeId)
                {
                    GameObject obj = GameObject.Instantiate(presetRowConfig.Asset);
                    AddDecoration(type, obj);
                    var presetDecorationData = new PresetDecorationData()
                    {
                        GameObject = obj,
                        PresetType = type
                    };
                    _decToWall.Add(presetDecorationData, wall);
                    return obj;
                }
            }
            return null;
        }

        private void AddDecoration(PresetType type, GameObject gameObject)
        {
            if (_presetDecorationsDict.TryGetValue(type, out var list))
            {
                list.Add(gameObject);
            }
            else
            {
                if (_presetDecorationsDict.TryAdd(type, new List<GameObject>()))
                {
                    _presetDecorationsDict[type].Add(gameObject);
                }
            }
        }

        private List<PresetDecorationData> _tempList = new List<PresetDecorationData>();
        public void RemoveDecoration(PresetType type)
        {
            if (_presetDecorationsDict.TryGetValue(type, out var list))
            {
                foreach (var obj in list)
                {
                    GameObject.Destroy(obj);
                }
                list.Clear();
                foreach (var data in _decToWall)
                {
                    if (data.Key.PresetType == type)
                    {
                        _tempList.Add(data.Key);
                    }
                }

                foreach (var decorationData in _tempList)
                {
                    _decToWall.Remove(decorationData);
                }
                _tempList.Clear();
            }
        }

        public bool HasDecoration(PresetType type, GameObject entityObject)
        {
            foreach (var data in _decToWall)
            {
                if (data.Key.PresetType == type && data.Value == entityObject)
                {
                    return true;
                }
            }

            return false;
        }
    }
}