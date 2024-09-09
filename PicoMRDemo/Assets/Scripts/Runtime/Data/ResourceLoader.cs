/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using PicoMRDemo.Runtime.Data.Config;
using UnityEngine;

namespace PicoMRDemo.Runtime.Data
{
    public class ResourceLoader : MonoBehaviour, IResourceLoader
    {

        [SerializeField]
        private AssetSetting _assetSetting;
        [SerializeField]
        private ItemTable _itemTable;
        [SerializeField]
        private WallThemeTable _wallThemeTable;
        [SerializeField] 
        private FloorThemeTable _floorThemeTable;
        [SerializeField]
        private CeilingThemeTable _ceilingThemeTable;
        [SerializeField]
        private ThemeTable _themeTable;
        [SerializeField]
        private BallDropItemTable _ballDropItemTable;


        public AssetSetting AssetSetting => _assetSetting;
        public ItemTable ItemTable => _itemTable;
        public WallThemeTable WallThemeTable => _wallThemeTable;
        public FloorThemeTable FloorThemeTable => _floorThemeTable;
        public CeilingThemeTable CeilingThemeTable => _ceilingThemeTable;
        public ThemeTable ThemeTable => _themeTable;
        public BallDropItemTable BallDropItemTable => _ballDropItemTable;
    }
}