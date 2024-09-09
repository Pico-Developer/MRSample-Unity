/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using PicoMRDemo.Runtime.Data.Decoration;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Service;
using PicoMRDemo.Runtime.Runtime.ShootingGame;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using VContainer;

namespace PicoMRDemo.Runtime.UI
{
    public class ShootGamePage : MonoBehaviour
    {
        [FormerlySerializedAs("OpenToggle")] public Toggle openToggle;

        [FormerlySerializedAs("PlayButton")] public Button playButton;

        [Inject]
        public IDecorationDataLoader DecorationDataLoader;
        
        [Inject]
        private IShootingGameManager _shootingGameManager;
        
        [Inject]
        private IItemFactory _itemFactory;
        
        public void TogglePage()
        {
            if (openToggle.isOn) return;
            
            openToggle.isOn = true;
        }
        
        private void OnEnable()
        {
            RegisterEvent();
        }

        private void OnDisable()
        {
            UnregisterEvent();
        }


        private void RegisterEvent()
        {
            playButton.GetComponent<XRSimpleInteractable>().lastSelectExited.AddListener(OnShoot);
        }

        private void UnregisterEvent()
        {
            playButton.GetComponent<XRSimpleInteractable>().lastSelectExited.RemoveListener(OnShoot);
        }
        
        public delegate void Action<in T>(T obj); 
        private void OnShoot(SelectExitEventArgs selectExitEventArgs)
        {
            Debug.Log("OnShoot");
            var decorationDatas = DecorationDataLoader.LoadData(DecorationType.Item);
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
                bool isLeftController = ControllerManager.Instance.LeftControllerRoot == selectExitEventArgs.interactor.gameObject;
                if (_shootingGameManager.IsStart||ControllerManager.Instance.GetControllerState(isLeftController) != ControllerState.Normal)
                {
                    
                }
                else
                {
                    var resourceID = ((DecorationData)shootingGame[0]).ID;
                    var item = _itemFactory.CreateItem(resourceID, Vector3.zero, Quaternion.identity, ItemState.Float);
                    var gun = item as Gun;
                    if (gun != null)
                    {
                        _shootingGameManager.StartGame(gun);
                        Transform transform1;
                        (transform1 = gun.transform).SetParent(selectExitEventArgs.interactorObject.transform);
                        transform1.localPosition = Vector3.zero;
                        transform1.localRotation = Quaternion.identity;
                        ControllerManager.Instance.BingingTriggerHotKey(isLeftController, (args) =>
                        {
                            _shootingGameManager.Shoot(gun);
                        });
                        ControllerManager.Instance.BingingGripHotKey(isLeftController,(args) =>
                        {
                            _shootingGameManager.EndGame();
                            ControllerManager.Instance.UnBingingGameHotKey(isLeftController);
                            ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.Normal);

                        },null,null);
                        ControllerManager.Instance.SetControllerShow(isLeftController, false,false);
                        ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.ShootGame);
                    }
                }
            }
        }
       
    }
}