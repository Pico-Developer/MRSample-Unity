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
using PicoMRDemo.Runtime.Runtime.BallDrop;

namespace PicoMRDemo.Runtime.UI
{
    public class BallDropPage : MonoBehaviour
    {
        public Toggle OpenToggle;
        
        public Button ClearAllButton;
        
        public Button DemoButton;
        [Inject]
        public IDecorationDataLoader _decorationDataLoader;
        
        [Inject]
        private IBallDropGameManager _ballDropGameManager;
        
        [Inject]
        private IItemFactory _itemFactory;
        
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
            DemoButton.onClick.AddListener(OnShowDemo);
            ClearAllButton.onClick.AddListener(OnClearAll);
            var decorationDatas = _decorationDataLoader.LoadData(DecorationType.DropBallItem);
            IList<IDecorationData> dropItemDatas = new List<IDecorationData>();
            foreach (var decorationData in decorationDatas)
            {
                if (decorationData is DecorationData data)
                {
                    if ((data.ItemType & ItemType.BallDropBall) > 0||
                        (data.ItemType & ItemType.BallDropBlock) > 0||
                        (data.ItemType & ItemType.BallDropRoad) > 0)
                    {
                        dropItemDatas.Add(data);
                    }
                }
            }
            ClearPage();
            ShowPage(dropItemDatas);
            
        }

        private void UnregisterEvent()
        {
            DemoButton.onClick.RemoveListener(OnShowDemo);
            ClearAllButton.onClick.RemoveListener(OnClearAll);
        }
        
        public delegate void Action<in T>(T obj);
        
        private void OnShowDemo()
        {
            _ballDropGameManager.ShowDemoGameData();
        }
        private void OnClearBall()
        {
            _ballDropGameManager.ClearBalls();
        }
        private void OnClearBlock()
        {
            _ballDropGameManager.ClearBlocks();
        }
        private void OnClearRoad()
        {
            _ballDropGameManager.ClearRoads();
        }
        private void OnClearAll()
        {
            _ballDropGameManager.ClearAll();
        }
        #region Page Item逻辑

        public Transform Root;
        public GameObject Prefab;
        public Transform Pool;

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
                Button clearAllBtn = button.transform.GetComponentsInChildren<Button>()
                    .First(x => x.gameObject.name == "ClearAllButton");
                var itemType = ((DecorationData)decorationData).ItemType;
                if (itemType == ItemType.BallDropBall)
                {
                   
                    clearAllBtn.onClick.RemoveAllListeners();
                    clearAllBtn.onClick.AddListener(OnClearBall);
                }
                else if (itemType == ItemType.BallDropBlock)
                {
                   
                    clearAllBtn.onClick.RemoveAllListeners();
                    clearAllBtn.onClick.AddListener(OnClearBlock);
                }
                else if (itemType == ItemType.BallDropRoad)
                {
                   
                    clearAllBtn.onClick.RemoveAllListeners();
                    clearAllBtn.onClick.AddListener(OnClearRoad);
                }
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
                        // binding delect event
                        ControllerManager.Instance.BingingSecondaryHotKey(isLeftController,(args)=>
                        {
                            _ballDropGameManager.DeleteObj(isLeftController);
                        },null,null);
                        if (itemType == ItemType.BallDropBall)
                        {
                            ControllerManager.Instance.ShowAnchorPreview( _resourceLoader.AssetSetting.ballPreview,isLeftController);
                            ControllerManager.Instance.BingingTriggerHotKey(isLeftController,(args) =>
                            {
                                _ballDropGameManager.CreateBallObj(isLeftController);
                            });
                            ControllerManager.Instance.BingingGripHotKey(isLeftController,
                                (args) =>
                                {
                                    ControllerManager.Instance.HideAnchorPreview(isLeftController);
                                    ControllerManager.Instance.UnBingingGameHotKey(isLeftController);
                                    ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.Normal);
                                },null,null);
                            ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.BallDrop);
                        }
                        else if (itemType == ItemType.BallDropBlock)
                        {
                            ControllerManager.Instance.ShowAnchorPreview( _resourceLoader.AssetSetting.blockPreview,isLeftController);
                            ControllerManager.Instance.BingingTriggerHotKey(isLeftController,(args) =>
                            {
                                _ballDropGameManager.CreateBlockObj(isLeftController);
                            });
                            ControllerManager.Instance.BingingGripHotKey(isLeftController,
                                (args) =>
                                {
                                    ControllerManager.Instance.HideAnchorPreview(isLeftController);
                                    ControllerManager.Instance.UnBingingGameHotKey(isLeftController);
                                    ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.Normal);
                                },null,null);
                            ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.BallDrop);
                        }
                        else if (itemType == ItemType.BallDropRoad)
                        {
                            ControllerManager.Instance.ShowAnchorPreview( _resourceLoader.AssetSetting.roadPreview,isLeftController);
                            ControllerManager.Instance.BingingTriggerHotKey(isLeftController,(args) =>
                            {
                                _ballDropGameManager.CreateRoadObj(isLeftController);
                            });
                            ControllerManager.Instance.BingingGripHotKey(isLeftController,
                                (args) =>
                                {
                                    ControllerManager.Instance.HideAnchorPreview(isLeftController);
                                    ControllerManager.Instance.UnBingingGameHotKey(isLeftController);
                                    ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.Normal);
                                },null,null);
                            ControllerManager.Instance.SetControllerState(isLeftController, ControllerState.BallDrop);
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
                if (pressableButton.GetComponent<XRSimpleInteractable>() != null)
                {
                    pressableButton.GetComponent<XRSimpleInteractable>().lastSelectExited.RemoveAllListeners();
                    _buttonPool.Enqueue(pressableButton);
                    pressableButton.transform.SetParent(Pool);
                }
                
            }
            _showButtons.Clear();
        }

        #endregion
    }
}