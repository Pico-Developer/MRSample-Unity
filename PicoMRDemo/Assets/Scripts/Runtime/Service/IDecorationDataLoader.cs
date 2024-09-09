/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Data.Config;
using PicoMRDemo.Runtime.Data.Decoration;
using UnityEngine;
using VContainer;

namespace PicoMRDemo.Runtime.Service
{
    public interface IDecorationDataLoader
    {
        IList<IDecorationData> LoadAllData();

        IList<IDecorationData> LoadData(DecorationType type);
    }

    public interface IItemDataLoader
    {
        IAssetConfig LoadIItemAssetConfig();
    }

    public interface IMaterialLoader
    {
        IAssetConfig LoadMaterialConfig();
    }

    public interface IPresetDecorationLoader
    {
        IAssetConfig LoadPresetDecoration();
    }

    public class DecorationDataLoader : IDecorationDataLoader
    {
        [Inject]
        public IResourceLoader ResourceLoader;
        public IList<IDecorationData> LoadAllData()
        {
            // TODO config
            return new List<IDecorationData>();
        }

        public IList<IDecorationData> LoadData(DecorationType type)
        {
            IList<IDecorationData> result = new List<IDecorationData>();
            var originDatas = ResourceLoader.ItemTable.ItemDatas.Select(x => x as IDecorationData);
            if (type == DecorationType.Wall)
                originDatas = ResourceLoader.WallThemeTable.WallThemeDatas.Select(x => x as IDecorationData);
            if (type == DecorationType.Floor)
                originDatas = ResourceLoader.FloorThemeTable.FloorThemeDatas.Select(x => x as IDecorationData);
            if (type == DecorationType.Ceiling)
                originDatas = ResourceLoader.CeilingThemeTable.CeilingThemeDatas.Select(x => x as IDecorationData);
            if (type == DecorationType.Theme)
                originDatas = ResourceLoader.ThemeTable.ThemeDatas.Select(x => x as IDecorationData);
            if (type == DecorationType.DropBallItem)
                originDatas = ResourceLoader.BallDropItemTable.ItemDatas.Select(x => x as IDecorationData);
            
            foreach (var originData in originDatas)
            {
                result.Add(originData);
            }
            return result;
        }
    }

    public class ItemDataLoader : IItemDataLoader
    {
        [Inject]
        public IResourceLoader ResourceLoader;
        public IAssetConfig LoadIItemAssetConfig()
        {
            IList<IAssetRowConfig<GameObject>> rowConfigs = new List<IAssetRowConfig<GameObject>>();
            var originDatas = ResourceLoader.AssetSetting.Id2PrefabDatas;
            foreach (var id2PrefabData in originDatas)
            {
                IAssetRowConfig<GameObject> assetRowConfig = new PrefabRowConfig()
                {
                    ID = id2PrefabData.ID,
                    Asset = id2PrefabData.Prefab
                };
                rowConfigs.Add(assetRowConfig);
            }
            IAssetConfig assetConfig = new AssetConfig<GameObject>(rowConfigs);
            return assetConfig;
        }
    }
    
    public class MaterialLoader : IMaterialLoader
    {
        [Inject]
        public IResourceLoader ResourceLoader;
        public IAssetConfig LoadMaterialConfig()
        {
            IList<IAssetRowConfig<Material>> rowConfigs = new List<IAssetRowConfig<Material>>();
            var originDatas = ResourceLoader.AssetSetting.Id2Materials;
            foreach (var id2MaterialData in originDatas)
            {
                IAssetRowConfig<Material> assetRowConfig = new MaterialRowConfig()
                {
                    ID = id2MaterialData.ID,
                    Asset = id2MaterialData.Material
                };
                rowConfigs.Add(assetRowConfig);
            }
            IAssetConfig assetConfig = new AssetConfig<Material>(rowConfigs);
            return assetConfig;
        }
    }

    public class PresetDecorationLoader : IPresetDecorationLoader
    {
        [Inject]
        public IResourceLoader ResourceLoader;
        public IAssetConfig LoadPresetDecoration()
        {
            IList<IAssetRowConfig<GameObject>> rowConfigs = new List<IAssetRowConfig<GameObject>>();
            var originDatas = ResourceLoader.AssetSetting.Id2PresetDatas;
            foreach (var id2PresetData in originDatas)
            {
                IAssetRowConfig<GameObject> assetRowConfig = new PresetRowConfig()
                {
                    ID = id2PresetData.ID,
                    Asset = id2PresetData.Prefab,
                    Type = id2PresetData.Type,
                    ThemeId = id2PresetData.ThemeId
                };
                rowConfigs.Add(assetRowConfig);
            }
            IAssetConfig assetConfig = new AssetConfig<GameObject>(rowConfigs);
            return assetConfig;
        }
    }
}