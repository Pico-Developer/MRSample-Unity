/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace PicoMRDemo.Runtime.Runtime.Item
{
    public class CatchableItem : Item
    {
        [HideInInspector] 
        private ICatchable Catchable;

        private XRGrabInteractable _grabInteractable;
        
        public event Action<ICatchable> OnDropItem;
        protected override void Awake()
        {
            base.Awake();
            Catchable = GetComponent<ICatchable>();
            InitController();
        }
        
        private void OnDestroy()
        {
            ReleaseController();
        }

        private void InitController()
        {
            _grabInteractable = gameObject.GetComponent<XRGrabInteractable>();
            if (_grabInteractable != null)
            {
                _grabInteractable.selectEntered.AddListener(OnSelectEnter);
                _grabInteractable.selectExited.AddListener(OnSelectExited);
            }
            
        }

        private void ReleaseController()
        {
            if (_grabInteractable != null)
            {
                _grabInteractable.selectEntered.RemoveListener(OnSelectEnter);
                _grabInteractable.selectExited.RemoveListener(OnSelectExited);
            }
        }
        private void OnSelectEnter(SelectEnterEventArgs arg)
        {

        }
        
        private void OnSelectExited(SelectExitEventArgs arg)
        {
            DropItem();
        }
        public void DropItem()
        {
            OnDropItem?.Invoke(Catchable);
        }
    }
}