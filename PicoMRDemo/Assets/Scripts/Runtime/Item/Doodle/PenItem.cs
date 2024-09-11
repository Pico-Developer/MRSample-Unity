/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using PicoMRDemo.Runtime.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using PicoMRDemo.Runtime.UI;

namespace PicoMRDemo.Runtime.Runtime.Item
{
    public enum PenType
    {
        Normal = 0,
        Roller = 1,
        Brush = 2,
        Erase = 3,
    }
    public class PenItem : Item
    {
        public DoodleInteractor doodleInteractor;
        public ScrollRect ScrollRect;
        public Transform ColorRoot;

        public int BrushSize = 1;
        
        [Tooltip("颜色列表单侧预显示个数")]
        public int PredisplayCount = 1; 
        [Tooltip("颜色列表一次移动的距离")]
        public float ColorSlottStep = 150;

        public MeshRenderer PenMesh;
        public MeshRenderer DoodleMesh;
        
        public GameObject curDoodleRoot;
        public GameObject[] Pens;
        public GameObject[] DoodleRoots;
        private PenType CurPen;

        private int _colorIndex;
        private TweenerCore<Vector2, Vector2, VectorOptions> _tweenerCore;
        private bool _isLeftController;
        protected override void Awake()
        {
            base.Awake();
            SwitchPenType(PenType.Normal);
            InitColorView();
        }

        public void SwitchPenType(PenType penType)
        {
            CurPen = penType;
            var idx = (int)penType;
            for (int i = 0; i < Pens.Length; i++)
            {
                Pens[i].SetActive(i == idx);
            }

            var curPen = Pens[idx];
            curDoodleRoot = DoodleRoots[idx];
            PenMesh = curPen.GetComponent<MeshRenderer>();
            DoodleMesh = curDoodleRoot.GetComponent<MeshRenderer>();
            doodleInteractor = curDoodleRoot.GetComponent<DoodleInteractor>();
            if (penType == PenType.Erase)
            {
                CloseColorView();
            }
            else
            {
                var length = ConstantProperty.BasicColors.Length;
                doodleInteractor.ChangeColor(ConstantProperty.BasicColors[_colorIndex]);
                PenMesh.material.color = ConstantProperty.BasicColors[(_colorIndex + length) % length];
                DoodleMesh.material.color = ConstantProperty.BasicColors[(_colorIndex + length) % length];
            }
        }

        private void Update()
        {
            var controller = ControllerManager.Instance.GetController(_isLeftController);
            Ray ray = new Ray(controller.transform.position, controller.transform.forward);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, LayerMask.GetMask("Water")))
            {
                curDoodleRoot.transform.position = hit.point;
                curDoodleRoot.SetActive(true);
            }
            else
            {
                curDoodleRoot.SetActive(false);
            }
        }

        public void RegisterControllerEvent(bool isLeftController)
        {
            _isLeftController = isLeftController;
            ControllerManager.Instance.BingingRotateAnchorHorizontalActionHotKey(_isLeftController, ChangeColor);
            ControllerManager.Instance.BingingRotateAnchorVerticalActionHotKey(_isLeftController, ChangeColor);
            ColorRoot.gameObject.SetActive(true);
            CancelItemSpecialAction();
            ControllerManager.Instance.BingingSecondaryHotKey(_isLeftController, StartItemSpecialAction, null,
                CancelItemSpecialAction);
        }
        
        public void UnregisterControllerEvent()
        {
            ColorRoot.gameObject.SetActive(false);
            ControllerManager.Instance.UnBingingRotateAnchorHorizontalInputAction();
            ControllerManager.Instance.UnBingingRotateAnchorVerticalInputAction();
            CancelItemSpecialAction();
            if (_isLeftController)
            {
                ControllerManager.Instance.UnBingingSecondaryInputActionLeft();
            }
            else
            {
                ControllerManager.Instance.UnBingingSecondaryInputActionRight();
            }
        }

        private void StartItemSpecialAction(InputAction.CallbackContext callback)
        {
            doodleInteractor.enabled = true;
        }

        private void CancelItemSpecialAction(InputAction.CallbackContext callback)
        {
            doodleInteractor.enabled = false;
        }

        private void CancelItemSpecialAction()
        {
            doodleInteractor.enabled = false;
        }
        #region 颜色面板相关逻辑
        private void InitColorView()
        {
            var length = ConstantProperty.BasicColors.Length;
            var prefab = ColorRoot.GetChild(0);
            prefab.gameObject.SetActive(false);

            for (int i = - PredisplayCount - 1; i <= length + PredisplayCount; i++)
            {
                var go = Instantiate(prefab, ColorRoot);
                var image = go.GetComponent<Image>();
                image.color = ConstantProperty.BasicColors[(i + length) % length];
                go.gameObject.SetActive(true);
            }
            
            ColorRoot.gameObject.SetActive(false);
            ScrollRect.content.anchoredPosition = new Vector2((-PredisplayCount-_colorIndex) * ColorSlottStep, ScrollRect.content.anchoredPosition.y);
            doodleInteractor.ChangeColor(ConstantProperty.BasicColors[_colorIndex]);
            PenMesh.material.color = ConstantProperty.BasicColors[(_colorIndex + length) % length];
            doodleInteractor.BrushSize = BrushSize;
        }

        private void CloseColorView()
        {
            ScrollRect.gameObject.SetActive(false);
        }
        
        private void ChangeColor(InputAction.CallbackContext obj)
        {
            if (CurPen == PenType.Erase)
            {
                return;
            }
            
            var dir = obj.ReadValue<Vector2>();
            if (dir.x == 0) return; 
            
            ChangeColorIndex(dir.x > 0 ? 1 : -1);
        }

        private void ChangeColorIndex(int increment)
        {
            var content = ScrollRect.content;
            var length = ConstantProperty.BasicColors.Length;

            if (_tweenerCore is { active: true })
            {
                _tweenerCore.Complete();
            }

            _colorIndex += increment;
            _tweenerCore = content.DOAnchorPosX((-PredisplayCount - _colorIndex) * ColorSlottStep,
                0.2f).OnComplete(() =>
            {
                if (_colorIndex >= length || _colorIndex < 0)
                {
                    _colorIndex = (_colorIndex + length) % length;
                    content.anchoredPosition = new Vector2((-PredisplayCount-_colorIndex) * ColorSlottStep, content.anchoredPosition.y);
                }
            });

            doodleInteractor.ChangeColor(ConstantProperty.BasicColors[(_colorIndex + length) % length]);
            PenMesh.material.color = ConstantProperty.BasicColors[(_colorIndex + length) % length];
            DoodleMesh.material.color = ConstantProperty.BasicColors[(_colorIndex + length) % length];
        }

        #endregion
    }
}