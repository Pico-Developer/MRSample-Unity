/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PicoMRDemo.Runtime.Data.Config
{
    public interface IAssetRowConfig<out T>
    {
        ulong ID { get; }
        T Asset { get; }
    }
    public interface IAssetConfig
    {
        T GetAssetByID<T>(ulong id);
    }

    public class PrefabRowConfig : IAssetRowConfig<GameObject>
    {
        public ulong ID { get; set; }
        public GameObject Asset { get; set; }
    }

    public class PresetRowConfig : IAssetRowConfig<GameObject>
    {
        public ulong ID { get; set; }
        public GameObject Asset { get; set; }
        public PresetType Type { get; set; }
        
        public ulong ThemeId { get; set; }
    }

    public class MaterialRowConfig : IAssetRowConfig<Material>
    {
        public ulong ID { get; set; }
        public Material Asset { get; set; }
    }
    
    public class AssetConfig<T> : IAssetConfig
    {
        private Dictionary<ulong, IAssetRowConfig<T>> _assets;

        private readonly string TAG = nameof(AssetConfig<T>);

        public AssetConfig(IList<IAssetRowConfig<T>> rowConfigs)
        {
            _assets = new Dictionary<ulong, IAssetRowConfig<T>>();
            foreach (var itemAssetRowConfig in rowConfigs)
            {
                var id = itemAssetRowConfig.ID;
                if (!_assets.ContainsKey(id))
                {
                    _assets.Add(id, itemAssetRowConfig);
                }
                else
                {
                    Debug.unityLogger.LogWarning(TAG, $"Duplication key ID: {id}");
                }
            }
        }
        
        public TAsset GetAssetByID<TAsset>(ulong id)
        {
            TAsset result = default(TAsset);

            if (_assets.TryGetValue(id, out var assetConfig))
            {
                var asset = assetConfig.Asset;
    
                if (asset is TAsset tAsset)
                {
                    result = tAsset;
                }
                else
                {
                    Debug.unityLogger.LogError(TAG, $"Can't find asset, ID: {id}");
                }
            }
            else
            {
                Debug.unityLogger.LogError(TAG, $"Can't find asset, ID: {id}");
            }
            
            return result;
        }

        public IList<IAssetRowConfig<T>> GetAllAssetConfig()
        {
            return _assets.Select(x => x.Value).ToList();
        }
    }
}