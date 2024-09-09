/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using PicoMRDemo.Runtime.Data.Decoration;
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Game;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Service;
using PicoMRDemo.Runtime.Runtime.ShootingGame;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using VContainer;

namespace PicoMRDemo.Runtime.UI
{
    public class PaintShootPage : MonoBehaviour
    {
        public Toggle OpenToggle;

        public Button PlayButton;

        public GameObject spatialMeshPanel;

        public Toggle spatialMeshToggle;
        
        public GameObject spatialMeshTips;

        [Inject]
        public IDecorationDataLoader _decorationDataLoader;
        
        [Inject]
        private IPaintBallGameManager _paintBallGameManager;
        
        [Inject]
        private IEntityManager _entityManager;
        
        [Inject]
        private IItemFactory _itemFactory;
        
        public void TogglePage()
        {
            if (OpenToggle.isOn) return;
            
            OpenToggle.isOn = true;
        }
        
        private void OnEnable()
        {
#if UNITY_EDITOR
            spatialMeshPanel.SetActive(true);
            spatialMeshToggle.isOn = App.Instance.spatialMeshManager.activeSelf;
            spatialMeshToggle.onValueChanged.AddListener(OnSpatialMeshToggleChanged);
#else
            if (PXR_System.GetProductName() == "PICO 4 Ultra")
            {
                spatialMeshPanel.SetActive(true);
                spatialMeshToggle.isOn = App.Instance.spatialMeshManager.activeSelf;
                spatialMeshToggle.onValueChanged.AddListener(OnSpatialMeshToggleChanged);
            }
            else
            {
                spatialMeshPanel.SetActive(false);
            }
#endif
            
            RegisterEvent();
        }

        private void OnDisable()
        {
            if (PXR_System.GetProductName() == "PICO 4 Ultra")
            {
                spatialMeshToggle.onValueChanged.RemoveListener(OnSpatialMeshToggleChanged);
            }
            UnregisterEvent();
        }
        
        
        private void RegisterEvent()
        {
            PlayButton.GetComponent<XRSimpleInteractable>().lastSelectExited.AddListener(OnPaint);

        }

        private void UnregisterEvent()
        {
            PlayButton.GetComponent<XRSimpleInteractable>().lastSelectExited.RemoveListener(OnPaint);

        }
        
        private void OnSpatialMeshToggleChanged(bool isOn)
        {
            App.Instance.spatialMeshManager.SetActive(isOn);
            spatialMeshToggle.isOn = isOn;
            spatialMeshTips.SetActive(isOn);
            _entityManager.SetGameEntityRootVisiable(!isOn);
            _entityManager.SetRoomEntityRootVisiable(!isOn);
        }
        
        private void OnPaint(SelectExitEventArgs selectExitEventArgs)
        {
            Debug.Log("OnShoot");
            var decorationDatas = _decorationDataLoader.LoadData(DecorationType.Item);
            IList<IDecorationData> paintGame = new List<IDecorationData>();
            foreach (var decorationData in decorationDatas)
            {
                if (decorationData is DecorationData data)
                {
                    if ((data.ItemType & ItemType.PaintBall) > 0)
                    {
                        paintGame.Add(data);
                    }
                    
                }
            }

            if (paintGame.Count > 0)
            {
                bool isLeftController = ControllerManager.Instance.LeftControllerRoot == selectExitEventArgs.interactor.gameObject;
                if (_paintBallGameManager.IsStart||ControllerManager.Instance.GetControllerState(isLeftController) != ControllerState.Normal)
                {
                    
                }
                else
                {
                    var resourceID = ((DecorationData)paintGame[0]).ID;
                    var item = _itemFactory.CreateItem(resourceID, Vector3.zero, Quaternion.identity, ItemState.Float);
                    var gun = item as Gun;
                    if (gun != null)
                    {
                        _paintBallGameManager.StartGame(gun);
                        Transform transform1;
                        (transform1 = gun.transform).SetParent(selectExitEventArgs.interactorObject.transform);
                        transform1.localPosition = Vector3.zero;
                        transform1.localRotation = Quaternion.identity;
                        ControllerManager.Instance.BingingTriggerHotKey(isLeftController, (args) =>
                        {
                            _paintBallGameManager.Shoot(gun);
                        });
                        ControllerManager.Instance.BingingGripHotKey(isLeftController,(args) =>
                        {
                            _paintBallGameManager.EndGame();
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