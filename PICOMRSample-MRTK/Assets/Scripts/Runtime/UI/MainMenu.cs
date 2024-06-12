/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using PicoMRDemo.Runtime.Data.Decoration;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Runtime.ShootingGame;
using PicoMRDemo.Runtime.Service;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using VContainer;

namespace PicoMRDemo.Runtime.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Main Page")] 
        public DecorateRoomPage DecorateRoomPage;
        public DoodlePage DoodlePage;
        public DebugMainPage DebugMainPage;
        
        [Header("Activity Buttons")]
        public PressableButton DecorateRoomButton;
        public PressableButton DoodleButton;
        public PressableButton ShootButton;
        public PressableButton PetButton;
        public PressableButton DebugMainPageButton;

        [Header("Buttons")] 
        public PressableButton LogWindowButton;

        [Inject][HideInInspector]
        public DialogPool DialogPool;

        [Inject]
        private IItemFactory _itemFactory;

        [Inject]
        private IDecorationDataLoader _decorationDataLoader;

        [Inject]
        private IShootingGameManager _shootingGameManager;

        [Inject]
        private IVirtualWorldManager _virtualWorldManager;

        private Vector3 _cameraPosition;
        private StatefulInteractable _interactable;

        public event Action OnClose;

        public void Open()
        {
            gameObject.SetActive(true);
            var mainCameraTransform = Camera.main.transform;
            var cameraPos = mainCameraTransform.position;
            var targetPos = cameraPos + mainCameraTransform.forward * 0.8f;
            gameObject.transform.position = targetPos;
            var directionToTarget = targetPos - cameraPos;
            var orientation = Quaternion.LookRotation(directionToTarget);
            gameObject.transform.rotation = orientation;
        }

        public void Close()
        {
            gameObject.SetActive(false);
            OnClose?.Invoke();
        }

        private void Start()
        {
            _interactable = GetComponent<StatefulInteractable>();
        }

        private void OnEnable()
        {
            _cameraPosition = Camera.main.transform.position;
            DecorateRoomButton.OnClicked.AddListener(OnDecorateRoom);
            DoodleButton.OnClicked.AddListener(OnDoodle);
            ShootButton.lastSelectExited.AddListener(OnShoot);
            PetButton.lastSelectExited.AddListener(OnVirtualWorld);
            DebugMainPageButton.OnClicked.AddListener(OnDebugMainPage);
            CheckIfDeActive().Forget();
        }

        private void OnDisable()
        {
            DebugMainPageButton.OnClicked.RemoveListener(OnDebugMainPage);
            PetButton.lastSelectExited.RemoveListener(OnVirtualWorld);
            ShootButton.lastSelectExited.RemoveListener(OnShoot);
            DoodleButton.OnClicked.RemoveListener(OnDoodle);
            DecorateRoomButton.OnClicked.RemoveListener(OnDecorateRoom);
        }

        private void OnDecorateRoom()
        {
            DecorateRoomPage.Open();
        }

        private void OnDoodle()
        {
            DoodlePage.TogglePage();
        }

        private void OnShoot(SelectExitEventArgs selectExitEventArgs)
        {
            var decorationDatas = _decorationDataLoader.LoadData(DecorationType.Item);
            IList<IDecorationData> shootingGame = new List<IDecorationData>();
            foreach (var decorationData in decorationDatas)
            {
                if (decorationData is DecorationData data)
                {
                    if ((data.ItemType & ItemType.ShootGame) > 0)
                    {
                        shootingGame.Add(data);
                    }
                    
                }
            }

            if (shootingGame.Count > 0)
            {
                var resourceID = ((DecorationData)shootingGame[0]).ID;
                var item = _itemFactory.CreateItem(resourceID, Vector3.zero, Quaternion.identity, ItemState.Normal);
                var gun = item as Gun;
                item.OnGrab += () =>
                {
                    _shootingGameManager.StartGame(gun);
                };
                item.OnDrop += () =>
                {
                    _shootingGameManager.EndGame(gun);
                };
                item.OnTriggerEnter += () =>
                {
                    _shootingGameManager.Shoot(gun);
                };
                item.GrabItem(selectExitEventArgs.interactorObject);
            }
        }

        private void OnVirtualWorld(SelectExitEventArgs selectExitEventArgs)
        {
            var decorationDatas = _decorationDataLoader.LoadData(DecorationType.Item);
            IList<IDecorationData> virtualWorld = new List<IDecorationData>();
            foreach (var decorationData in decorationDatas)
            {
                if (decorationData is DecorationData data)
                {
                    if ((data.ItemType & ItemType.VirtualWorld) > 0)
                    {
                        virtualWorld.Add(data);
                    }
                    
                }
            }
            if (virtualWorld.Count > 0)
            {
                var resourceID = ((DecorationData)virtualWorld[0]).ID;
                var item = _itemFactory.CreateItem(resourceID, Vector3.zero, Quaternion.identity, ItemState.Normal);
                var stick = item as MagicStick;
                stick.SwitchState(_virtualWorldManager.IsOpen ? MagicStick.StickState.Open : MagicStick.StickState.Close);
                item.OnTriggerEnter += () =>
                {
                    if (!_virtualWorldManager.IsOpeningOrClosing)
                    {
                        if (_virtualWorldManager.IsOpen)
                        {
                            stick.SwitchState(MagicStick.StickState.Close);
                            _virtualWorldManager.CloseWorldAsync(this.GetCancellationTokenOnDestroy());
                        }
                        else
                        {
                            _virtualWorldManager.OpenWorldAsync(this.GetCancellationTokenOnDestroy());
                            stick.SwitchState(MagicStick.StickState.Open);
                        }
                    }
                };
                item.GrabItem(selectExitEventArgs.interactorObject);
            }
        }

        private void OnDebugMainPage()
        {
            DebugMainPage.Open();
        }


        private async UniTaskVoid CheckIfDeActive()
        {
            while (true)
            {
                await UniTask.Delay(1000);
                var mainCameraPos = Camera.main.transform.position;
                if (_interactable.isSelected || _interactable.isHovered)
                {
                    _cameraPosition = mainCameraPos;
                }
                else
                {
                    var dis = Vector3.Distance(_cameraPosition, mainCameraPos);
                    if (dis > 2f)
                    {
                        Close();
                        break;
                    }
                }
                
            }
        }
    }
}