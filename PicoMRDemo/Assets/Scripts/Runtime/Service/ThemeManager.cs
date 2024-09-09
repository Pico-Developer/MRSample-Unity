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
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Runtime.PresetDecoration;
using PicoMRDemo.Runtime.Runtime.Theme;
using Unity.XR.PXR;
using UnityEngine;
using VContainer;

namespace PicoMRDemo.Runtime.Service
{
    public class ThemeManager : IThemeManager
    {
        [Inject]
        private IThemeLoader _themeLoader;
        [Inject]
        private IEntityManager _entityManager;
        [Inject] 
        private IResourceLoader _resourceLoader;
        [Inject]
        private IPresetDecorationManager _presetDecorationManager;

        private readonly string TAG = nameof(ThemeManager);

        private Dictionary<PxrSemanticLabel, IDecorationData> _currentThemes =
            new Dictionary<PxrSemanticLabel, IDecorationData>();

        private MultiDecorationData DefaultThemeData = new MultiDecorationData()
        {
            ThemeId = 100,
            Title = "极简",
            Type = DecorationType.Theme,
            Description = "测试",
            IDList = new List<ulong>()
            {
                1000,2000,3000
            }
        };
        
        public void SwitchTheme(IDecorationData data)
        {
            var decorationData = data as DecorationData;
            var resourceID = decorationData.ID;
            var material = _themeLoader.GetMaterial(resourceID);
            var entities = _entityManager.GetRoomEntities();
            Debug.unityLogger.Log(TAG, $"Get id {resourceID}, material: {material}");
            var decorationLabel = GetMappingSceneLabel(decorationData.Type);
            if (_currentThemes.ContainsKey(decorationLabel))
            {
                _currentThemes[decorationLabel] = data;
            }
            else
            {
                _currentThemes.Add(decorationLabel, data);
            }
            foreach (var entity in entities)
            {                
                if (entity.GetRoomLabel() == decorationLabel||(decorationData.Type == DecorationType.Wall&&entity.GetRoomLabel() == PxrSemanticLabel.VirtualWall))
                {
                    Debug.unityLogger.Log(TAG, $"Change label{entity.AnchorData.SceneLabel}-{entity.AnchorData.Handle}");
                    var renderer = entity.GameObject.GetComponentInChildren<Renderer>();
                    renderer.material = material;

                    if (entity.GetRoomLabel() == PxrSemanticLabel.Wall
                        || entity.GetRoomLabel() == PxrSemanticLabel.VirtualWall)
                    {
                        // 特殊处理踢脚线
                        var newMaterialId = resourceID + 3000;
                        var skirtingLineMaterial = _themeLoader.GetMaterial(newMaterialId);
                        var renderers = entity.GameObject.transform.GetComponentsInChildren<Renderer>();
                        foreach (var renderer1 in renderers)
                        {
                            if (renderer1.gameObject.name.Equals("SkirtingLine"))
                            {
                                renderer1.material = skirtingLineMaterial;
                                break;
                            }
                        }
                    }
                }
            }
            
            // process presetDecorations
            RemoveRoomPresetDecoration(decorationLabel);
            CreateRoomPresetDecoration(decorationLabel);
        }

        public void SwitchAllTheme(IDecorationData data)
        {
            if (data.Type != DecorationType.Theme)
            {
                Debug.unityLogger.LogWarning(TAG, $"data: {data} is not theme");
                return;
            }

            var themeData = data as MultiDecorationData;
            var decorationDatas = GetDecorationDatas(themeData);
            foreach (var decorationData in decorationDatas)
            {
                SwitchTheme(decorationData);
            }
        }

        public void SwitchToDefaultTheme()
        {
            SwitchAllTheme(DefaultThemeData);
        }

        public IList<DecorationData> GetCurrentThemes()
        {
            return _currentThemes.Select(x => x.Value as DecorationData).ToList();
        }

        public IDecorationData GetCurrentTheme(PxrSemanticLabel label)
        {
            if (_currentThemes.TryGetValue(label, out var theme))
            {
                return theme;
            }
            return null;
        }

        public ulong GetThemeId(IDecorationData data)
        {
            if (data.Type == DecorationType.Theme)
                return (data as MultiDecorationData).ThemeId;
            var decorationData = data as DecorationData;
            var themeDatas = _resourceLoader.ThemeTable.ThemeDatas;
            foreach (var multiDecorationData in themeDatas)
            {
                var ids = multiDecorationData.IDList;
                foreach (var labelId in ids)
                {
                    if (labelId == decorationData.ID)
                    {
                        return multiDecorationData.ThemeId;
                    }
                }
            }
            return 0;
        }

        private IList<DecorationData> GetDecorationDatas(MultiDecorationData data)
        {
            IList<DecorationData> result = new List<DecorationData>();
            foreach (var id in data.IDList)
            {
                var decorationData = GetDecorationData(id);
                if (decorationData != null)
                    result.Add(decorationData);
            }

            return result;
        }

        private DecorationData GetDecorationData(ulong id)
        {
            var floorDatas = _resourceLoader.FloorThemeTable.FloorThemeDatas;
            var ceilingDatas = _resourceLoader.CeilingThemeTable.CeilingThemeDatas;
            var wallDatas = _resourceLoader.WallThemeTable.WallThemeDatas;

            foreach (var floorData in floorDatas)
            {
                if (floorData.ID == id)
                    return floorData;
            }

            foreach (var ceilingData in ceilingDatas)
            {
                if (ceilingData.ID == id)
                    return ceilingData;
            }

            foreach (var wallData in wallDatas)
            {
                if (wallData.ID == id)
                    return wallData;
            }
            return null;
        }

        private PxrSemanticLabel GetMappingSceneLabel(DecorationType type)
        {
            switch (type)
            {
                case DecorationType.Wall:
                    return PxrSemanticLabel.Wall;
                case DecorationType.Floor:
                    return PxrSemanticLabel.Floor;
                case DecorationType.Ceiling:
                    return PxrSemanticLabel.Ceiling;
                default:
                    return PxrSemanticLabel.Unknown;
            }
        }

        private void CreateRoomPresetDecoration(PxrSemanticLabel label)
        {
            var walls = _entityManager.GetRoomEntities(PxrSemanticLabel.Wall);
            if (label == PxrSemanticLabel.Wall)
            {
                // 没有门窗，添加门
                var doorOrWindows = _entityManager.GetRoomEntities(PxrSemanticLabel.Door);
                if (doorOrWindows == null || doorOrWindows.Count == 0)
                {
                    IEntity selectedWall = null;
                    foreach (var wall in walls)
                    {
                        var extent = wall.AnchorData.SceneBox2DData.Extent;
                        if (extent.x > 1.8f && extent.y > 2.5f)
                        {
                            selectedWall = wall;
                            break;
                        }
                    }
    
                    if (selectedWall != null)
                    {
                        var planeInfo = selectedWall.AnchorData.SceneBox2DData;
                        var theme = GetCurrentTheme(PxrSemanticLabel.Wall);
                        var themeId = GetThemeId(theme);
                        var door = _presetDecorationManager.CreateDecoration(PresetType.Door, themeId, selectedWall.GameObject);
                        if (door != null)
                        {
                            var pos = selectedWall.GameObject.transform.position - new Vector3(0, planeInfo.Extent.y * 0.5f, 0);
                            door.transform.position = pos;
                            door.transform.rotation = selectedWall.AnchorData.Rotation;
                            walls.Remove(selectedWall);
                        }
                    }
                }
                else
                {
                    var tempWalls = new List<IEntity>();
                    foreach (var wall in walls)
                    {
                        foreach (var doorOrWindow in doorOrWindows)
                        {
                            if (wall.AnchorData.Rotation == doorOrWindow.AnchorData.Rotation)
                            {
                                tempWalls.Add(wall);
                                break;
                            }
                        }
                    }
                    foreach (var tempWall in tempWalls)
                    {
                        walls.Remove(tempWall);
                    }
                    tempWalls.Clear();
                }
                // todo 墙上无挂件，随机添加窗户，挂画，钟表 
                bool DoNotGenerateWindow = true;
                foreach (var wall in walls)
                {
                    if (HasDoorOrWindow(wall, doorOrWindows))
                    {
                        continue;
                    }
                    var extent = wall.AnchorData.SceneBox2DData.Extent;
                    if (extent.x > 1.8f && extent.y > 2.5f)
                    {
                        var planeInfo = wall.AnchorData.SceneBox2DData;
                        var theme = GetCurrentTheme(PxrSemanticLabel.Wall);
                        var themeId = GetThemeId(theme);
                        List<PresetType> randomList;
                        if (DoNotGenerateWindow)
                        {
                            randomList = new List<PresetType>()
                            {
                                PresetType.Picture,
                                PresetType.Timepiece,
                            };
                            DoNotGenerateWindow = false;
                        }
                        else
                        {
                            randomList = new List<PresetType>()
                            {
                                PresetType.Picture,
                                PresetType.Timepiece,
                                PresetType.Window
                            };
                        }
                        var presetType = randomList[Random.Range(0, randomList.Count)];
                        var decoration = _presetDecorationManager.CreateDecoration(presetType, themeId, wall.GameObject);
                        if (decoration != null)
                        {
                            var pos = wall.GameObject.transform.position;
                            decoration.transform.position = pos;
                            decoration.transform.rotation = wall.AnchorData.Rotation;
                        }
                    }
                }
            }

            if (label == PxrSemanticLabel.Ceiling)
            {
                var ceilings = _entityManager.GetRoomEntities(PxrSemanticLabel.Ceiling);
                var ceiling = ceilings[0];
                
                var theme = GetCurrentTheme(PxrSemanticLabel.Ceiling);
                var themeId = GetThemeId(theme);
                var decoration = _presetDecorationManager.CreateDecoration(PresetType.DomeLight, themeId, ceiling.GameObject);
                if (decoration != null)
                {
                    decoration.transform.position = ceiling.AnchorData.Position;
                }
            }
        }

        private void RemoveRoomPresetDecoration(PxrSemanticLabel label)
        {
            if (label == PxrSemanticLabel.Wall)
            {
                _presetDecorationManager.RemoveDecoration(PresetType.Door);
                _presetDecorationManager.RemoveDecoration(PresetType.Picture);
                _presetDecorationManager.RemoveDecoration(PresetType.Timepiece);
                _presetDecorationManager.RemoveDecoration(PresetType.Window);
            }

            if (label == PxrSemanticLabel.Ceiling)
            {
                _presetDecorationManager.RemoveDecoration(PresetType.DomeLight);
            }
        }

        private bool HasDoorOrWindow(IEntity wall, IList<IEntity> doorOrWindowList)
        {
            if (wall.GetRoomLabel() != PxrSemanticLabel.Wall || doorOrWindowList.Count <= 0)
            {
                return false;
            }

            foreach (var doorOrWindow in doorOrWindowList)
            {
                Vector3 doorPos = doorOrWindow.GameObject.transform.position;
                Vector3 doorPosInWall = wall.GameObject.transform.InverseTransformPoint(doorPos);
                var planeBoundaryInfo = wall.AnchorData.SceneBox2DData;
                var extent = planeBoundaryInfo.Extent;
                
                if (Mathf.Abs(doorPosInWall.z) <= 0.5f && Mathf.Abs(doorPosInWall.x) <= extent.x && Mathf.Abs(doorPosInWall.y) <= extent.y)
                {
                    return true;
                }
            }
            return false;
        }
    }
}