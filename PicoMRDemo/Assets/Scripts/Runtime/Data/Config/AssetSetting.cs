/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace PicoMRDemo.Runtime.Data.Config
{
    public enum PresetType
    {
        Window,
        Door,
        DomeLight,
        Picture,
        Timepiece,
    }
    public class AssetSetting : ScriptableObject
    {
        [Header("Prefab")]
        public GameObject WhiteBoard;
        public GameObject Balloon;
        public GameObject Bullet;
        public GameObject PaintBall;
        public GameObject ballPreview;
        public GameObject ballPrefab;
        public GameObject roadPreview;
        public GameObject blockPreview;
        public GameObject blockPrefab;
        public GameObject AnchorPreviewPrefab;
        
        [FormerlySerializedAs("Nian")] [Header("Pet Prefab")] 
        public GameObject Robot;
        public GameObject AStar;
        
        [Header("Material")]
        public Material RoomEntityMaterial;
        public Material Mirror;

        [Header("Id2Prefab")] 
        public List<Id2PrefabData> Id2PrefabDatas;

        [Header("Id2Material")]
        public List<Id2Material> Id2Materials;

        [Header("Id2PresetData")]
        public List<Id2PresetData> Id2PresetDatas;

        [Header("Prefab")] 
        public GameObject Skybox;
        
        
    }
    
    [Serializable]
    public class Id2PrefabData
    {
        public ulong ID;
        public GameObject Prefab;
    }

    [Serializable]
    public class Id2PresetData
    {
        public ulong ID;
        public GameObject Prefab;
        public PresetType Type;
        public ulong ThemeId;
    }

    [Serializable]
    public class Id2Material
    {
        public ulong ID;
        public Material Material;
    }
}