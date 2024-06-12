/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using System.Linq;
using Honeti;
using Microsoft.MixedReality.Toolkit.UX;
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

        public PressableButton PenButton;

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
        
        
        public void RegisterEvent()
        {
            PenButton.OnClicked.AddListener(OnPenButton);
        }

        public void UnregisterEvent()
        {
            PenButton.OnClicked.RemoveListener(OnPenButton);
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

       
        public IList<PressableButton> ShowButtons => _showButtons;

        private Queue<PressableButton> _buttonPool = new Queue<PressableButton>();
        private IList<PressableButton> _showButtons = new List<PressableButton>();

        public void ShowPage(IList<IDecorationData> data)
        {
            foreach (var decorationData in data)
            {
                PressableButton button = null;
                if (_buttonPool.Count > 0)
                {
                    button = _buttonPool.Dequeue();
                }
                else
                {
                    button = GameObject.Instantiate(Prefab).GetComponent<PressableButton>();
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

                button.selectEntered.AddListener((arg =>
                {
                    var resourceID = ((DecorationData)decorationData).ID;
                    var item = _itemFactory.CreateItem(resourceID, Vector3.zero, Quaternion.identity, ItemState.Normal);
                    if (item is PenItem penItem)
                    {
                        var typeIdx = resourceID - 100;
                        penItem.SwitchPenType((PenType)typeIdx);
                    }
                    item.GrabItem(arg.interactorObject);
                } ));

                button.transform.SetParent(Root, false);
                button.gameObject.SetActive(true);
                _showButtons.Add(button);
            }
        }

    

        public void ClearPage()
        {
            var buttons = Root.GetComponentsInChildren<PressableButton>();
            foreach (var pressableButton in buttons)
            {
                pressableButton.selectEntered.RemoveAllListeners();
                _buttonPool.Enqueue(pressableButton);
                pressableButton.transform.SetParent(Pool);
            }
            _showButtons.Clear();
        }

        #endregion
       
    }
}