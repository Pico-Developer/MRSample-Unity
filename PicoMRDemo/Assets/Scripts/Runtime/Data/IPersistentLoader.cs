/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using PicoMRDemo.Runtime.Data.Decoration;
using PicoMRDemo.Runtime.Runtime.Item;

namespace PicoMRDemo.Runtime.Data
{
    [Serializable]
    public class UUID2ItemId
    {
        [JsonProperty("uuid")]
        public Guid Uuid;
        [JsonProperty("itemId")]
        public ulong ItemId;
        [JsonProperty("itemState")]
        public ItemState ItemState;
    }

    [Serializable]
    public class ThemeData
    {
        [JsonProperty("Type")]
        public DecorationType Type;
        [JsonProperty("Id")]
        public ulong Id;
    }

    public interface IPersistentLoader
    {
        bool TryGetItemInfo(Guid uuid, out UUID2ItemId itemInfo);
        IList<DecorationData> GetAllThemeDatas();
        UniTask LoadAllData();
        UniTask SaveAllData();
        void StageAllItemData(IList<UUID2ItemId> data);
        void StageAllThemeData(IList<DecorationData> decorationData);
        UniTask DeleteAllData();
        void ClearAllItemData();
        void ClearAllThemeData();
    }
}