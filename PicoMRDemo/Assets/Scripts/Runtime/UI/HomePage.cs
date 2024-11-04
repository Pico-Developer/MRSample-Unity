/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Entity;
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
    public class HomePage : MonoBehaviour
    {
        public Toggle OpenToggle;

        public Button SaveThemeDataButton;
        
        public Button ClearThemeDataButton;
        
        public Button ReCaptureRoomButton;
        
        public Button SpatialAnchorButton;
        [Inject]
        private IResourceLoader _resourceLoader;
        [Inject]
        public IDecorationDataLoader _decorationDataLoader;
        
        [Inject]
        private IShootingGameManager _shootingGameManager;
        
        [Inject]
        private IItemFactory _itemFactory;
        
        [Inject]
        private IEntityManager _entityManager;
        [Inject]
        private IPersistentLoader _persistentLoader;
        [Inject]
        private IThemeManager _themeManager;
        [Inject]
        private IRoomService _roomService;
        
        public void TogglePage()
        {
            if (OpenToggle.isOn) return;
            
            OpenToggle.isOn = true;
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
            SaveThemeDataButton.onClick.AddListener(OnSaveThemeDataButton);
            ClearThemeDataButton.onClick.AddListener(OnClearThemeDataButton);
            ReCaptureRoomButton.onClick.AddListener(OnReCaptureRoomButton);
            SpatialAnchorButton.GetComponent<XRSimpleInteractable>().lastSelectExited.AddListener(OnCreateAnchor);
        }

        private void UnregisterEvent()
        {
            SaveThemeDataButton.onClick.RemoveListener(OnSaveThemeDataButton);
            ClearThemeDataButton.onClick.RemoveListener(OnClearThemeDataButton);
            ReCaptureRoomButton.onClick.RemoveListener(OnReCaptureRoomButton);
            SpatialAnchorButton.GetComponent<XRSimpleInteractable>().lastSelectExited.RemoveListener(OnCreateAnchor);
        }
        private async void OnSaveThemeDataButton()
        {
            await _entityManager.SaveGameEntities();
            _persistentLoader.StageAllThemeData(_themeManager.GetCurrentThemes());
            await _persistentLoader.SaveAllData();
        }
        
        private async void OnClearThemeDataButton()
        {
            await _entityManager.ClearGameEntities();
            _persistentLoader.ClearAllThemeData();
            await _persistentLoader.DeleteAllData();
            _themeManager.SwitchToDefaultTheme();
        }
        private async void OnReCaptureRoomButton()
        {
            _roomService.QuitRoom();
            var result = await PXR_MixedReality.StartSceneCaptureAsync();
            Debug.unityLogger.Log($"OnReCaptureRoomButton: {result}");
            await _entityManager.LoadRoomEntities();
            _roomService.EnterRoom();
            _entityManager.LoadGameEntities();
        }
        
        private void OnCreateAnchor(SelectExitEventArgs selectExitEventArgs)
        {
            Debug.Log("OnCreateAnchor");

            bool isLeftController = ControllerManager.Instance.LeftControllerRoot == selectExitEventArgs.interactor.gameObject;
            if (_roomService.IsAnchorCreate()||ControllerManager.Instance.GetControllerState(isLeftController) != ControllerState.Normal)
            {
                
            }
            else
            {
                _roomService.SetAnchorCreate(true);
                ControllerManager.Instance.ShowAnchorPreview(_resourceLoader.AssetSetting.AnchorPreviewPrefab,isLeftController);
                ControllerManager.Instance.SetControllerState(isLeftController,ControllerState.AnchorCreate);
                ControllerManager.Instance.BingingTriggerHotKey(isLeftController, async(args) =>
                {
                    var item = _itemFactory.CreateItem(8, (isLeftController?ControllerManager.Instance.LeftControllerPreviewPoint:ControllerManager.Instance.RightControllerPreviewPoint).transform.position,(isLeftController? ControllerManager.Instance.LeftControllerPreviewPoint: ControllerManager.Instance.RightControllerPreviewPoint).transform.rotation,ItemState.Normal);
                    if (item != null)
                    {
                        var entity = await _entityManager.CreateAndAddEntity(item.GameObject);
                        item.Entity = entity;
                        item.EntityManager = _entityManager;
                    }

                });
                ControllerManager.Instance.BingingSecondaryHotKey(isLeftController, (args) =>
                {
                    if ((isLeftController?ControllerManager.Instance.LeftControllerRoot:ControllerManager.Instance.RightControllerRoot).GetComponent<XRRayInteractor>()
                        .TryGetCurrent3DRaycastHit(out var hit))
                    {
                        if (hit.collider.CompareTag("SpaceAnchor"))
                        {
                            
                            foreach (var entity in _entityManager.GetGameEntities())
                            {
                                if (hit.collider.gameObject.transform.parent.gameObject == entity.GameObject)
                                {
                                    _entityManager.DeleteEntity(entity);
                                    return;
                                }
                            }
                        }
                    }
                },null,null);
                ControllerManager.Instance.BingingGripHotKey(isLeftController,
                    (args) =>
                    {
                        _roomService.SetAnchorCreate(false);
                        ControllerManager.Instance.HideAnchorPreview(isLeftController);
                        ControllerManager.Instance.UnBingingGameHotKey(isLeftController);
                        ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.Normal);
                    },null,null);
            }
        }
       
    }
}