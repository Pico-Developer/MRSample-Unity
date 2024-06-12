/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using CommonUsages = UnityEngine.XR.CommonUsages;

namespace PicoMRDemo.Runtime.Runtime.Item
{
    public class GrabableComponent : MonoBehaviour
    {
        #region 交互逻辑
                
        private SolverHandler _solverHandler;
        private ObjectManipulator _objectManipulator;
        protected IXRSelectInteractor _interactor;
        protected ArticulatedHandController _articulatedHandController;
        protected Rigidbody Rigidbody;

        public event Action OnGrab;
        public event Action OnDrop;
        public event Action OnTriggerEnter;

        protected bool _isGrab;

        private bool _isTrigger;

        protected virtual void Awake()
        {
            InitController();
        }
        
        protected virtual void Update()
        {
            if (_isGrab)
            {
                CheckTrigger();
            }
        }
        
        private void OnDestroy()
        {
            ReleaseController();
        }
        
        private void InitController()
        {
            Rigidbody = GetComponentInChildren<Rigidbody>();
            _objectManipulator = gameObject.GetComponent<ObjectManipulator>();
            _solverHandler = gameObject.GetComponent<SolverHandler>();
           
            
            _solverHandler.UpdateSolvers = false;
            _solverHandler.TrackedTargetType = TrackedObjectType.CustomOverride;
            _solverHandler.AdditionalOffset = Vector3.zero;
            _solverHandler.TransformOverride = null;

            _objectManipulator.selectEntered.AddListener(OnSelectEnter);
            _objectManipulator.selectExited.AddListener(OnSelectExited);

        }

        private void ReleaseController()
        {
            _objectManipulator.selectEntered.RemoveListener(OnSelectEnter);
            _objectManipulator.selectExited.RemoveListener(OnSelectExited);
        }

        protected void CheckTrigger()
        {
            var device = InputDevices.GetDeviceAtXRNode(_articulatedHandController.HandNode);
            device.TryGetFeatureValue(CommonUsages.triggerButton, out var isDown);
            // 检测鼠标左键是否点击
#if UNITY_EDITOR
            var mouse = Mouse.current;
            isDown = mouse.leftButton.isPressed && Keyboard.current.spaceKey.isPressed;
#endif
            if (isDown == _isTrigger) return;

            _isTrigger = isDown;
            if (_isTrigger)
            {
                StartItemSpecialAction();
            }
            else
            {
                CancelItemSpecialAction();
            }
            
        }
        
        protected virtual void StartItemSpecialAction()
        {
            OnTriggerEnter?.Invoke();
        }
        
        protected virtual void CancelItemSpecialAction()
        {
            
        }

        private bool _isTriggerPressed = false;
        private void OnSelectEnter(SelectEnterEventArgs arg)
        {
            _isTriggerPressed = IsTriggerPressed(arg.interactorObject);
            if (_isTriggerPressed)
                return;
            if (_interactor != null && _interactor.transform.parent == arg.interactorObject.transform.parent)
            {
                _objectManipulator.AllowedManipulations = ~TransformFlags.None;
                DropItem();
                return;
            }

            _objectManipulator.AllowedManipulations = TransformFlags.None;
            GrabItem(arg.interactorObject);
        }
        
        private void OnSelectExited(SelectExitEventArgs arg)
        {
            if (_isTriggerPressed)
                return;
            _objectManipulator.AllowedManipulations = TransformFlags.None;
        }

        public virtual void GrabItem(IXRSelectInteractor curController)
        {    
            if (_interactor != null)
            {
                DropItem();
            }
            
            _isGrab = true;
            _interactor = curController;
            _articulatedHandController = _interactor.transform.GetComponentInParent<ArticulatedHandController>();
            _solverHandler.UpdateSolvers = true;
            _solverHandler.TransformOverride = _interactor.transform;
            if (Rigidbody != null)
                Rigidbody.isKinematic = true;
            OnGrab?.Invoke();
        }

        public virtual void DropItem()
        {
            _interactor = null;
            _articulatedHandController = null;
            _solverHandler.UpdateSolvers = false;
            _isTrigger = false;
            _isGrab = false;
            if (Rigidbody != null)
                Rigidbody.isKinematic = false;
            OnDrop?.Invoke();
        }


        #endregion

        protected bool IsTriggerPressed(IXRSelectInteractor interactor)
        {
            var actionBaseController = interactor.transform.GetComponentInParent<ArticulatedHandController>();
            bool isTrigger = actionBaseController.selectAction.action.activeControl.displayName.Equals("trigger");
            return isTrigger;
        }
    }
}