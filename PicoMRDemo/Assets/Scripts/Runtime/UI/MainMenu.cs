/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Cysharp.Threading.Tasks;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Runtime.ShootingGame;
using PicoMRDemo.Runtime.Runtime.BallDrop;
using PicoMRDemo.Runtime.Service;
using UnityEngine;
using VContainer;
using UnityEngine.UI;

namespace PicoMRDemo.Runtime.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Main Page")]
        public HomePage HomePage;
        public DecorateRoomPage DecorateRoomPage;
        public DoodlePage DoodlePage;
        public ShootGamePage ShootPage;
        public VirtualWorldPage VirtualWorldPage;
        public BallDropPage BallDropPage;
        public PaintShootPage PaintShootPage;
        public PetGamePage PetGamePage;
        public DebugMainPage DebugMainPage;
        
        [Header("Activity Buttons")]
        public Toggle HomeButton;
        public Toggle DecorateRoomButton;
        public Toggle DoodleButton;
        public Toggle ShootButton;
        public Toggle VirtualWorldButton;
        public Toggle BallDropButton;
        public Toggle PaintShootButton;
        public Toggle PetGameButton;
        public Toggle DebugMainPageButton;

        [Header("Buttons")] 
        public Button LogWindowButton;

        [Inject]
        private IItemFactory _itemFactory;

        [Inject]
        private IDecorationDataLoader _decorationDataLoader;

        [Inject]
        private IShootingGameManager _shootingGameManager;

        [Inject]
        private IVirtualWorldManager _virtualWorldManager;
        
        [Inject]
        private IBallDropGameManager _ballDropGameManager;

        private Vector3 _cameraPosition;

        public event Action OnClose;
        private void Awake()
        {
            this.gameObject.SetActive(false);
        }
        public void Open()
        {
            gameObject.SetActive(true);
            if (Camera.main != null)
            {
                var mainCameraTransform = Camera.main.transform;
                var cameraPos = mainCameraTransform.position;
                var targetPos = cameraPos + mainCameraTransform.forward * 0.8f;
                gameObject.transform.position = targetPos;
                var directionToTarget = targetPos - cameraPos;
                var orientation = Quaternion.LookRotation(directionToTarget);
                gameObject.transform.rotation = orientation;
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
            OnClose?.Invoke();
        }

        private void Start()
        {
           
        }

        private void OnEnable()
        {
            _cameraPosition = Camera.main.transform.position;
            HomeButton.onValueChanged.AddListener(OnHome);
            DecorateRoomButton.onValueChanged.AddListener(OnDecorateRoom);
            DoodleButton.onValueChanged.AddListener(OnDoodle);
            ShootButton.onValueChanged.AddListener(OnShoot);
            VirtualWorldButton.onValueChanged.AddListener(OnVirtualWorld);
            BallDropButton.onValueChanged.AddListener(OnBallDrop);
            PaintShootButton.onValueChanged.AddListener(OnPaintShoot);
            PetGameButton.onValueChanged.AddListener(OnPetGame);
            DebugMainPageButton.onValueChanged.AddListener(OnDebugMainPage);
            CheckIfDeActive().Forget();
        }

        private void OnDisable()
        {
            DebugMainPageButton.onValueChanged.RemoveListener(OnDebugMainPage);
            PetGameButton.onValueChanged.RemoveListener(OnPetGame);
            PaintShootButton.onValueChanged.RemoveListener(OnPaintShoot);
            BallDropButton.onValueChanged.RemoveListener(OnBallDrop);
            VirtualWorldButton.onValueChanged.RemoveListener(OnVirtualWorld);
            ShootButton.onValueChanged.RemoveListener(OnShoot);
            DoodleButton.onValueChanged.RemoveListener(OnDoodle);
            DecorateRoomButton.onValueChanged.RemoveListener(OnDecorateRoom);
            HomeButton.onValueChanged.RemoveListener(OnHome);
        }
        
        private void OnHome(bool isOn)
        {
            if (isOn)
            {
                HomePage.TogglePage();
            }
            
        }
        
        private void OnDecorateRoom(bool isOn)
        {
            if (isOn)
            {
                DecorateRoomPage.Open();
            }
            
        }

        private void OnDoodle(bool isOn)
        {
            if (isOn)
            {
                DoodlePage.TogglePage();
            }
        }
        
        private void OnShoot(bool isOn)
        {
            if (isOn)
            {
                ShootPage.TogglePage();
            }
        }
        
        private void OnVirtualWorld(bool isOn)
        {
            if (isOn)
            {
                VirtualWorldPage.TogglePage();
            }
        }
        
        private void OnBallDrop(bool isOn)
        {
            if (isOn)
            {
                BallDropPage.TogglePage();
            }
        }
        
        private void OnPaintShoot(bool isOn)
        {
            if (isOn)
            {
                PaintShootPage.TogglePage();
            }
        }
        
        private void OnPetGame(bool isOn)
        {
            if (isOn)
            {
                PetGamePage.TogglePage();
            }
        }
        

        private void OnDebugMainPage(bool isOn)
        {
            if (isOn)
            {
                DebugMainPage.Open();
            }
        }


        private async UniTaskVoid CheckIfDeActive()
        {
            while (true)
            {
                await UniTask.Delay(1000);
                var mainCameraPos = Camera.main.transform.position;
                //if (_interactable.isSelected || _interactable.isHovered)
                //{
                //    _cameraPosition = mainCameraPos;
                //}
                //else
                //{
                    var dis = Vector3.Distance(_cameraPosition, mainCameraPos);
                    if (dis > 2f)
                    {
                        Close();
                        break;
                    }
                //}
                
            }
        }
    }
}