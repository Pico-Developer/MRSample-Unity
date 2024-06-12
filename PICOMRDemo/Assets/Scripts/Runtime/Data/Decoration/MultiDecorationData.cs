﻿/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PicoMRDemo.Runtime.Data.Decoration
{

    [Serializable]
    public class MultiDecorationData : IDecorationData
    {
        [SerializeField]
        private ulong _themeId;
        [SerializeField]
        private string _title;

        [SerializeField]
        private string _description;

        [SerializeField]
        private DecorationType _type = DecorationType.Item;

        [SerializeField]
        private List<ulong> _idList = new List<ulong>();

        [SerializeField] 
        private Sprite _sprite;

        public ulong ThemeId
        {
            get => _themeId;
            set => _themeId = value;
        }
        
        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public string Description
        {
            get => _description;
            set => _description = value;
        }

        public DecorationType Type
        {
            get => _type;
            set => _type = value;
        }

        public Sprite Sprite
        {
            get => _sprite;
            set => _sprite = value;
        }

        public IList<ulong> IDList
        {
            get => _idList;
            set => _idList = value as List<ulong>;
        }
    }
}