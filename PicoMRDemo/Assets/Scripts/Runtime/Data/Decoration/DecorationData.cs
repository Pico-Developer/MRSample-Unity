/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using UnityEngine;

namespace PicoMRDemo.Runtime.Data.Decoration
{
    [Flags]
    public enum ItemType
    {
        Nothing = 0,
        Normal = 1,
        Doodle = 2,
        ShootGame = 4,
        VirtualWorld = 8,
        BallDropBall = 16,
        BallDropBlock = 32,
        BallDropRoad = 64,
        PaintBall = 128,
    }
    
    [Serializable]
    public class DecorationData : IDecorationData
    {
        [SerializeField] private string _title;

        [SerializeField] private string _description;

        [SerializeField] private DecorationType _type = DecorationType.Item;

        [SerializeField] private ulong _id;

        [SerializeField] private ItemType _itemType = ItemType.Normal;
        [SerializeField] private Sprite _sprite;
        public ItemType ItemType => _itemType;

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

        public ulong ID
        {
            get => _id;
            set => _id = value;
        }

    }
}