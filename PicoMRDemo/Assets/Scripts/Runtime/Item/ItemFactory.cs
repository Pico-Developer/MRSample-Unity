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
using PicoMRDemo.Runtime.Utils;
using UnityEngine;
using VContainer;
using Vector3 = UnityEngine.Vector3;

namespace PicoMRDemo.Runtime.Runtime.Item
{
    public class ItemFactory : IItemFactory
    {
        [Inject]
        public IItemDataLoader ItemDataLoader;
        [Inject] 
        public ICatchableManager CatchableManager;
        
        private IAssetConfig _assetConfig;

        public IAssetConfig AssetConfig
        {
            get
            {
                if (_assetConfig == null)
                {
                    _assetConfig = ItemDataLoader.LoadIItemAssetConfig();
                }

                return _assetConfig;
            }
        }


        /// <summary>
        /// 限制数量的网格
        /// </summary>
        private static readonly Vector2Int FloatLimitSize = new Vector2Int(3, 3);

        public List<IItem> CreateFloatItems(ulong[] ids, Vector3 beginPos)
        {
            if (ids.Length <= 0) return null;
            
            int size = Mathf.CeilToInt(Mathf.Sqrt(ids.Length));
            List<IItem> items = new List<IItem>();
            for (int i = 0; i < size && items.Count < ids.Length; i++)
            {
                var temp = beginPos + Vector3.forward * ConstantProperty.SlotSize * i;
                for (int j = 0; j < size && items.Count < ids.Length; j++)
                {
                    temp += Vector3.right * ConstantProperty.SlotSize;
                    
                    if (!CanCreate(temp))
                    {
                        continue;
                    }
                    var item = CreateItemGameobject(ids[items.Count], temp, Quaternion.identity, ItemState.Float);
                    items.Add(item);
                }
            }

            return items;
        }
        
        public IItem CreateFloatItem(ulong id, Vector3 offset = default, Vector2Int limitNum = default)
        {
            if (limitNum == default || limitNum.x <= 0 || limitNum.y <= 0)
            {
                limitNum = FloatLimitSize;
            }

            if (Camera.main != null)
            {
                var mainCameraTransform = Camera.main.transform;
                var cameraPos = mainCameraTransform.position;
                var targetPos = cameraPos + offset;
                var success = false;
            
                for (int i = 0; i < limitNum.x && !success; i++)
                {
                    var temp = targetPos + Vector3.forward * ConstantProperty.SlotSize * i;
                    for (int j = 0; j < limitNum.y; j++)
                    {
                        temp += Vector3.right * ConstantProperty.SlotSize;
                    
                        if (!CanCreate(temp))
                        {
                            continue;
                        }                

                        targetPos = temp;
                        success = true;
                        break;
                    }
                }

                if (!success)
                {
                    return null;
                }
            
                return CreateItemGameobject(id, targetPos, Quaternion.identity, ItemState.Float);
            }
            return null;
        }

        public IItem CreateItem(ulong id, Vector3 pos, Quaternion rotation, ItemState itemState = ItemState.Normal)
        {
            return CreateItemGameobject(id, pos, rotation, itemState);
        }

        private bool CanCreate(Vector3 pos)
        {
            var colliders = Physics.OverlapSphere(pos, ConstantProperty.SlotSize / 2);

            if (colliders.Any(collider => collider.GetComponentInParent<IItem>() != null))
            {
                Debug.unityLogger.Log($"pos:{pos} has other item.not create.");
                return false;
            }
            
            return true;
        }
        
        private IItem CreateItemGameobject(ulong id, Vector3 pos, Quaternion rotation, ItemState itemState)
        {
            var prefab = AssetConfig.GetAssetByID<GameObject>(id);
            var obj = GameObject.Instantiate(prefab);
            obj.transform.position = pos;
            obj.transform.rotation = rotation;
            var item = obj.GetComponent<IItem>();
            if (item != null)
            {
                item.SetInitState(itemState);
                item.Id = id;
                if (item is CatchableItem catchableItem)
                {
                    catchableItem.OnDropItem += CatchableManager.SetCatchable;
                }
                item.GameObject.layer = 12;
            }
            return item;
        }
    }
}