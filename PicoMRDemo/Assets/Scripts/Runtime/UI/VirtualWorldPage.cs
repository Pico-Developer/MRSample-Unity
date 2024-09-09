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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using VContainer;
using Cysharp.Threading.Tasks;
using UnityEngine.Serialization;

namespace PicoMRDemo.Runtime.UI
{
    public class VirtualWorldPage : MonoBehaviour
    {
        [FormerlySerializedAs("OpenToggle")] public Toggle openToggle;

        [FormerlySerializedAs("PlayButton")] public Button playButton;

        [Inject]
        private IDecorationDataLoader _decorationDataLoader;
        
        [Inject]
        private IVirtualWorldManager _virtualWorldManager;
        
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
            playButton.GetComponent<XRSimpleInteractable>().lastSelectExited.AddListener(OnVirtualWorld);
        }

        private void UnregisterEvent()
        {
            playButton.GetComponent<XRSimpleInteractable>().lastSelectExited.AddListener(OnVirtualWorld);
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
                bool isLeftController = selectExitEventArgs != null && ControllerManager.Instance.LeftControllerRoot == selectExitEventArgs.interactor.gameObject;
                if (_virtualWorldManager.IsStart||ControllerManager.Instance.GetControllerState(isLeftController) != ControllerState.Normal)
                {
                    
                }
                else
                {
                    
                    var resourceID = ((DecorationData)virtualWorld[0]).ID;
                    var item = _itemFactory.CreateItem(resourceID, Vector3.zero, Quaternion.identity, ItemState.Normal);
                    var stick = item as MagicStick;
                    if (stick != null)
                    {
                        stick.SwitchState(_virtualWorldManager.IsOpen
                            ? MagicStick.StickState.Open
                            : MagicStick.StickState.Close);
                        if (stick != null)
                        {
                            _virtualWorldManager.StartGame(stick);
                            Transform transform1;
                            (transform1 = stick.transform).SetParent(selectExitEventArgs.interactorObject.transform);
                            transform1.localPosition = Vector3.zero;
                            transform1.localRotation = Quaternion.identity;
                            ControllerManager.Instance.BingingTriggerHotKey(isLeftController, (args) =>
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
                                        _virtualWorldManager.OpenWorldAsync(this.GetCancellationTokenOnDestroy(),isLeftController);
                                        stick.SwitchState(MagicStick.StickState.Open);
                                    }
                                }
                            });
                            ControllerManager.Instance.BingingGripHotKey(isLeftController, (args) =>
                            {
                                _virtualWorldManager.EndGame();
                                ControllerManager.Instance.UnBingingGameHotKey(isLeftController);
                                ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.Normal);
                            }, null, null);
                            ControllerManager.Instance.SetControllerShow(isLeftController, false,false);
                            ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.VirtualWorld);
                        }
                    }
                }
            }
        }
       
    }
}