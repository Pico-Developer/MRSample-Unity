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
using PicoMRDemo.Runtime.Data.Anchor;
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Service;
using UnityEngine;
using VContainer;
using PicoMRDemo.Runtime.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace PicoMRDemo.Runtime.Runtime.BallDrop
{
    public class BallDropGameManager : IBallDropGameManager
    {
        private bool _isStart;

        private HashSet<IItem> Roads { get; } = new HashSet<IItem>();
        
        private HashSet<GameObject> Blocks { get; } = new HashSet<GameObject>();
        
        private HashSet<GameObject> Balls { get; } = new HashSet<GameObject>();
        
        [Inject]
        private IResourceLoader _resourceLoader;

        [Inject] 
        private ILocationService _locationService;
        
        [Inject] 
        private IEntityManager _entityManager;
        
        [Inject]
        private IItemFactory _itemFactory;
        
        /// <summary>
        /// Delete Object
        /// </summary>
        /// <param name="isLeftController">Boolean value, indicating whether to use a left-hand controller or a right-hand controller</param>
        public void DeleteObj(bool isLeftController)
        {
            if ((isLeftController?ControllerManager.Instance.LeftControllerRoot:ControllerManager.Instance.RightControllerRoot).GetComponent<XRRayInteractor>()
                .TryGetCurrent3DRaycastHit(out var hit))
            {
                if (hit.collider.CompareTag("BallDropBall"))
                {
                    Object.Destroy(hit.collider.gameObject);
                    foreach (var ball in Balls)
                    {
                        if (hit.collider.gameObject == ball)
                        {
                            Balls.Remove(ball);
                            return;
                        }
                    }
                }
                if (hit.collider.CompareTag("BallDropBlock"))
                {
                    Object.Destroy(hit.collider.gameObject);
                    foreach (var block in Blocks)
                    {
                        if (hit.collider.gameObject == block)
                        {
                            Blocks.Remove(block);
                            return;
                        }
                    }
                }
                if (hit.collider.CompareTag("BallDropRoad"))
                {
                    var gameEntities = _entityManager.GetGameEntities().ToArray();
                    foreach (var gameEntity in gameEntities)
                    {
                        if (hit.collider.transform.parent.gameObject == gameEntity.GameObject)
                        {
                            _entityManager.DeleteEntity(gameEntity);
                        }
                    }
                    foreach (var road in Roads)
                    {
                        if (hit.collider.transform.parent.gameObject == road.GameObject)
                        {
                            //_entityManager.DeleteEntity(road.Entity);
                            Roads.Remove(road);
                            return;
                        }
                    }
                }
            }
        }
        
        private void CreateObj( GameObject prefab,bool isLeft,out GameObject gameObject)
        {
            gameObject = Object.Instantiate(prefab, (isLeft?ControllerManager.Instance.LeftControllerPreviewPoint:ControllerManager.Instance.RightControllerPreviewPoint).transform.position, (isLeft?ControllerManager.Instance.LeftControllerPreviewPoint:ControllerManager.Instance.RightControllerPreviewPoint).transform.rotation,_entityManager.GetGameEntityRoot());
        }
        public void CreateBallObj(bool isLeftController)
        {
            CreateObj(_resourceLoader.AssetSetting.ballPrefab,isLeftController,out var ballPrefab);
            Balls.Add(ballPrefab);
        }

        public async void CreateRoadObj(bool isLeftController)
        {
            var item = _itemFactory.CreateItem(201, (isLeftController?ControllerManager.Instance.LeftControllerPreviewPoint:ControllerManager.Instance.RightControllerPreviewPoint).transform.position,(isLeftController? ControllerManager.Instance.LeftControllerPreviewPoint: ControllerManager.Instance.RightControllerPreviewPoint).transform.rotation,ItemState.Normal);
            if (item == null) return;
            var entity = await _entityManager.CreateAndAddEntity(item.GameObject);
            item.Entity = entity;
            item.EntityManager = _entityManager;
            Roads.Add(item);
        }
        
        
        public void CreateBlockObj(bool isLeftController)
        {
            CreateObj(_resourceLoader.AssetSetting.blockPrefab,isLeftController,out var blockPrefab);
            Blocks.Add(blockPrefab);
        }

        public void ClearBalls()
        {
            foreach (var ball in Balls)
            {
                Object.Destroy(ball);
            }
            Balls.Clear();
        }

        public void ClearBlocks()
        {
            foreach (var block in Blocks)
            {
                Object.Destroy(block);
            }
            Blocks.Clear();
        }

        public void ClearRoads()
        {
            var gameEntities = _entityManager.GetGameEntities().ToArray();
            foreach (var gameEntity in gameEntities)
            {
                if (gameEntity.GameObject.CompareTag("BallDropRoad"))
                {
                    _entityManager.DeleteEntity(gameEntity);
                }
            }
            Roads.Clear();
        }

        public void ClearAll()
        {
            ClearBalls();
            ClearBlocks();
            ClearRoads();
        }
        
        public async void ShowDemoGameData()
        {
            _locationService.TryGetFloorPosition(out var floorPosition);
            var item = _itemFactory.CreateItem(201,new Vector3(-0.23399999737739564f,floorPosition.y + 1.1490000486373902f,0.4440000057220459f), new Quaternion(0.087155781686306f,0.0f,0.0f,0.9961947202682495f),ItemState.Normal);
            if (item != null)
            {
                var entity = await _entityManager.CreateAndAddEntity(item.GameObject);
                item.Entity = entity;
                item.EntityManager = _entityManager;
                Roads.Add(item);
            }
            
            item = _itemFactory.CreateItem(201,new Vector3(-0.23400002717971803f,floorPosition.y + 1.0260000228881837f,0.9739999771118164f), new Quaternion(0.13052615523338319f,0.0f,0.0f,0.9914448857307434f),ItemState.Normal);
            if (item != null)
            {
                var entity = await _entityManager.CreateAndAddEntity(item.GameObject);
                item.Entity = entity;
                item.EntityManager = _entityManager;
                Roads.Add(item);
            }
            item = _itemFactory.CreateItem(201,new Vector3(-0.3640000820159912f,floorPosition.y + 0.8690000176429749f,1.2940000295639039f), new Quaternion(0.03084351122379303f,-0.7064337730407715f,0.1530459225177765f,0.6903455257415772f),ItemState.Normal);
            if (item != null)
            {
                var entity = await _entityManager.CreateAndAddEntity(item.GameObject);
                item.Entity = entity;
                item.EntityManager = _entityManager;
                Roads.Add(item);
            }
            item = _itemFactory.CreateItem(201,new Vector3(-0.724000096321106f,floorPosition.y + 0.6570000052452087f,1.24399995803833f), new Quaternion(-0.1926659345626831f,0.3614530563354492f,-0.013098709285259247f,0.9121732115745544f),ItemState.Normal);
            if (item != null)
            {
                var entity = await _entityManager.CreateAndAddEntity(item.GameObject);
                item.Entity = entity;
                item.EntityManager = _entityManager;
                Roads.Add(item);
            }
            item = _itemFactory.CreateItem(201,new Vector3(-0.940000057220459f,floorPosition.y + 0.3499999940395355f,0.8860000371932983f), new Quaternion(-0.25881901383399966f,0.0f,0.0f,0.9659258723258972f),ItemState.Normal);
            if (item != null)
            {
                var entity = await _entityManager.CreateAndAddEntity(item.GameObject);
                item.Entity = entity;
                item.EntityManager = _entityManager;
                Roads.Add(item);
            }
            var blockPrefab = Object.Instantiate(_resourceLoader.AssetSetting.blockPrefab,new Vector3(-0.9480037689208984f,floorPosition.y + 0.18999096751213075f,0.466230571269989f), new Quaternion(0.00020853457681369036f,-0.010748459957540036f,-0.00010646878945408389f,0.9999422430992127f),_entityManager.GetGameEntityRoot());
            Blocks.Add(blockPrefab);
            blockPrefab = Object.Instantiate(_resourceLoader.AssetSetting.blockPrefab,new Vector3(-0.9479211568832398f,floorPosition.y + 0.1899910420179367f,0.32015034556388857f), new Quaternion(0.00020814068557228893f,-0.006836191285401583f,-0.00010725719766924158f,0.9999765753746033f),_entityManager.GetGameEntityRoot());
            Blocks.Add(blockPrefab);
            blockPrefab = Object.Instantiate(_resourceLoader.AssetSetting.blockPrefab,new Vector3(-0.9471774101257324f,floorPosition.y + 0.18999098241329194f,0.16558890044689179f), new Quaternion(0.00020760088227689266f,-0.0006391423521563411f,-0.00010874417785089463f,0.9999997615814209f),_entityManager.GetGameEntityRoot());
            Blocks.Add(blockPrefab);
        }
    }
}