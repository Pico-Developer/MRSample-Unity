/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace PicoMRDemo.Runtime.Utils
{
    public static class ConstantProperty
    {
        /// <summary>
        /// 用于创建道具，表示道具占据单位
        /// </summary>
        public const float SlotSize = 0.3f;

        /// <summary>
        /// 用于提示，当玩家在房间外时候的提示
        /// </summary>
        public const string OutsideRoomTip = "^OUTSIDE_TIP";
        
        /// <summary>
        /// 用于提示，当玩家在房间外时候的提示
        /// </summary>
        public const string EnterRoomTip = "^ENTERROOM_TIP";
        
        /// <summary>
        /// 基础色：红，橙，黄，绿，青，蓝，紫，灰，粉，黑，白，棕, 消除
        /// </summary>
        public static readonly Color32[] BasicColors = new Color32[]
        {
            Color.red,
            new Color32(255, 128,0,255),
            new Color32(255, 255,0,255),
            Color.green, 
            Color.cyan,
            Color.blue,
            new Color32(128, 0,255,255),
            Color.grey,
            new Color32(255, 192,203,255),
            Color.black,
            Color.white,     
            new Color32(166, 125,61,255),
        };
    }
}