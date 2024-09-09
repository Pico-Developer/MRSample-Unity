/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Linq;
using Honeti;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Data.Decoration;
using PicoMRDemo.Runtime.Runtime.Item;
using PicoMRDemo.Runtime.Service;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using VContainer;

namespace PicoMRDemo.Runtime.UI
{
    public class DoodlePage : MonoBehaviour
    {
        public Toggle OpenToggle;

        public Button PenButton;

        [Inject]
        public IDecorationDataLoader DecorationDataLoader;
        
        public void TogglePage()
        {
            if (OpenToggle.isOn) return;
            
            ClearPage();
            OpenToggle.isOn = true;
        }
        
        private void OnEnable()
        {
            RegisterEvent();
            OnPenButton();
        }

        private void OnDisable()
        {
            UnregisterEvent();
        }
        
        
        private void RegisterEvent()
        {
            PenButton.onClick.AddListener(OnPenButton);
        }

        private void UnregisterEvent()
        {
            PenButton.onClick.RemoveListener(OnPenButton);
        }
        
        private void OnPenButton()
        {
            var decorationDatas = DecorationDataLoader.LoadData(DecorationType.Item);
            IList<IDecorationData> doodleDatas = new List<IDecorationData>();
            foreach (var decorationData in decorationDatas)
            {
                if (decorationData is DecorationData data)
                {
                    if ((data.ItemType & ItemType.Doodle) > 0)
                    {
                        doodleDatas.Add(data);
                    }
                    
                }
            }
            ClearPage();
            ShowPage(doodleDatas);
        }


        #region Page Item逻辑

        public Transform Root;
        public GameObject Prefab;
        public Transform Pool;
     
        [Inject] 
        private IItemFactory _itemFactory;

        [Inject]
        private IResourceLoader _resourceLoader;

       
        public IList<Button> ShowButtons => _showButtons;

        private Queue<Button> _buttonPool = new Queue<Button>();
        private IList<Button> _showButtons = new List<Button>();

        public void ShowPage(IList<IDecorationData> data)
        {
            foreach (var decorationData in data)
            {
                Button button = null;
                if (_buttonPool.Count > 0)
                {
                    button = _buttonPool.Dequeue();
                }
                else
                {
                    button = GameObject.Instantiate(Prefab).GetComponent<Button>();
                }

                IList<TMP_Text> texts = button.transform.GetComponentsInChildren<TMP_Text>();

                if (texts.Count >= 2)
                {
                    texts[0].text = I18N.instance.getValue(decorationData.Title);
                    texts[1].text = I18N.instance.getValue(decorationData.Description);
                }
                
                Image showImage = button.transform.GetComponentsInChildren<Image>()
                    .First(x => x.gameObject.name == "ShowImage");
                if (showImage != null)
                {
                    showImage.sprite = decorationData.Sprite;
                }
                
                button.GetComponent<XRSimpleInteractable>().lastSelectExited.AddListener((arg =>
                {
                    bool isLeftController = ControllerManager.Instance.LeftControllerRoot == arg.interactor.gameObject;
                    if (ControllerManager.Instance.GetControllerState(isLeftController) != ControllerState.Normal)
                    {
                        
                    }
                    else
                    {
                        var resourceID = ((DecorationData)decorationData).ID;
                        var item = _itemFactory.CreateItem(resourceID, Vector3.zero, Quaternion.identity, ItemState.Float);
                        if (item is PenItem penItem)
                        {
                            var typeIdx = resourceID - 100;
                            penItem.SwitchPenType((PenType)typeIdx);
                            penItem.RegisterControllerEvent(isLeftController);
                            Transform transform1;
                            (transform1 = penItem.transform).SetParent(arg.interactorObject.transform);
                            transform1.localPosition = Vector3.zero;
                            transform1.localRotation = Quaternion.identity;
                            ControllerManager.Instance.BingingGripHotKey(isLeftController,
                                (args) =>
                                {
                                    ControllerManager.Instance.UnBingingGameHotKey(isLeftController);
                                    penItem.UnregisterControllerEvent();
                                    ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.Normal);
                                    GameObject.Destroy(penItem.GameObject);
                                },null,null);
                            ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.Doodle);
                            ControllerManager.Instance.SetControllerShow(isLeftController, false,false);
                        }
                    }
                    
                } ));

                button.transform.SetParent(Root, false);
                button.gameObject.SetActive(true);
                _showButtons.Add(button);
            }
        }

        public void ClearPage()
        {
            var buttons = Root.GetComponentsInChildren<Button>();
            foreach (var pressableButton in buttons)
            {
                pressableButton.GetComponent<XRSimpleInteractable>().lastSelectExited.RemoveAllListeners();
                _buttonPool.Enqueue(pressableButton);
                pressableButton.transform.SetParent(Pool);
            }
            _showButtons.Clear();
        }

        #endregion
       
    }
}