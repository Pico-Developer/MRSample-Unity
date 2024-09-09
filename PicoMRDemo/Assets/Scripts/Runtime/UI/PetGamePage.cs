/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using PicoMRDemo.Runtime.Data.Decoration;
using PicoMRDemo.Runtime.Service;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using System.Linq;
using Honeti;
using PicoMRDemo.Runtime.Data;
using PicoMRDemo.Runtime.Data.Config;
using PicoMRDemo.Runtime.Entity;
using PicoMRDemo.Runtime.Runtime.Item;
using TMPro;
using UnityEngine.Serialization;

namespace PicoMRDemo.Runtime.UI
{
    public class PetGamePage : MonoBehaviour
    {
        
        [FormerlySerializedAs("DecorationButton")] public Button decorationButton;

        [FormerlySerializedAs("OpenToggle")] public Toggle openToggle;
        
        public Button resetButton;
        
        [Inject]
        public IDecorationDataLoader DecorationDataLoader;

        [Inject] 
        public IRoomService RoomService;

        public void TogglePage()
        {
            if (openToggle.isOn) return;
            ClearPage();
            openToggle.isOn = true;
        }

        private void OnEnable()
        {
            RegisterEvent();
            OnDecorationButton();
        }

        private void OnDisable()
        {
            UnregisterEvent();
        }


        private void RegisterEvent()
        {
            decorationButton.onClick.AddListener(OnDecorationButton);
            resetButton.onClick.AddListener(OnResetButton);
        }

        private void UnregisterEvent()
        {
            decorationButton.onClick.RemoveListener(OnDecorationButton);
            resetButton.onClick.RemoveListener(OnResetButton);
        }

        private void OnDecorationButton()
        {
            var decorationDatas = DecorationDataLoader.LoadData(DecorationType.Item);
            ClearPage();
            ShowPage(decorationDatas);
        }
        
        private void OnResetButton()
        {
            RoomService.ResetPetPosition();
        }
        
        #region Page Item逻辑

        public Transform Root;
        public GameObject Prefab;
        public Transform Pool;
     
        [Inject] 
        private IItemFactory _itemFactory;

        [Inject]
        private IResourceLoader _resourceLoader;

        [Inject]
        private IEntityManager _entityManager;
        
        [Inject]
        private IRoomService _roomService;
       
        public IList<Button> ShowButtons => _showButtons;

        private Queue<Button> _buttonPool = new Queue<Button>();
        private IList<Button> _showButtons = new List<Button>();
        
        private IAssetConfig _assetConfig;
        public void ShowPage(IList<IDecorationData> data)
        {
            foreach (var decorationData in data)
            {
                if (decorationData is DecorationData itemData)
                {
                    if ((itemData.ItemType & ~ItemType.Normal) > 0)
                    {
                        continue;
                    }
                }
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
                
                button.onClick.AddListener(async () =>
                {
                    if (decorationData.Type == DecorationType.Item)
                    {
                        if (Camera.main != null)
                        {
                            var mainCameraTransform = Camera.main.transform;
                            var resourceID = ((DecorationData)decorationData).ID;
                            var item = _itemFactory.CreateFloatItem(resourceID, mainCameraTransform.forward * 0.5f);
                            /*if (item != null)
                            {
                                var entity = await _entityManager.CreateAndAddEntity(item.GameObject);
                                item.Entity = entity;
                                item.EntityManager = _entityManager;
                            }*/
                        }
                    }
                });
                
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
                pressableButton.onClick.RemoveAllListeners();
                _buttonPool.Enqueue(pressableButton);
                pressableButton.transform.SetParent(Pool);
            }
            _showButtons.Clear();
        }

        #endregion
    }
}