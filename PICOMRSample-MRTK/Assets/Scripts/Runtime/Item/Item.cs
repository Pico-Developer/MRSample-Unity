/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using FSM;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using PicoMRDemo.Runtime.Entity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using CommonUsages = UnityEngine.XR.CommonUsages;

namespace PicoMRDemo.Runtime.Runtime.Item
{
    
    public class Item : GrabableComponent, IItem
    {
        public GameObject GameObject => gameObject;
        public IEntity Entity { get; set; }
        public IEntityManager EntityManager { get; set; }
        public ItemState ItemState => StateMachine.ActiveState.name;
        public ulong Id { get; set; }

        protected StatefulInteractable Interactable;
        protected StateMachine<ItemState> StateMachine;

        private bool _useGravity;
        private bool _hasSelected = false;

        protected override void Awake()
        {
            base.Awake();
            Interactable = GetComponentInChildren<StatefulInteractable>();
            _useGravity = Rigidbody.useGravity;
            InitStateMachine();
            CancelItemSpecialAction();
        }

        protected void OnEnable()
        {
            RegisterEvent();
        }

        protected void OnDisable()
        {
            UnregisterEvent();
        }
        
        protected virtual void InitStateMachine()
        {
            StateMachine = new StateMachine<ItemState>();
            StateMachine.AddState(ItemState.Normal, new State<ItemState>(onEnter:state =>
            {
                Rigidbody.useGravity = _useGravity;
            }));
            StateMachine.AddState(ItemState.Float, new State<ItemState>(onEnter: state =>
            {
                Rigidbody.useGravity = false;
            }));
            StateMachine.AddTransition(new Transition<ItemState>(ItemState.Float, ItemState.Normal, (transition) => _hasSelected));
        }

        private void RegisterEvent()
        {
            Interactable.selectExited.AddListener(OnHasSelected);
            Interactable.selectEntered.AddListener(OnSelectedEnter);
        }

        private void UnregisterEvent()
        {
            Interactable.selectEntered.RemoveListener(OnSelectedEnter);
            Interactable.selectExited.RemoveListener(OnHasSelected);
        }

        // TODO 这个比较麻烦后续处理，这里写这个方法主要是因为StatefulInteractable在被选则时会记录useGravity，Exited时会还原
        private void OnSelectedEnter(SelectEnterEventArgs args)
        {
            Rigidbody.useGravity = _useGravity;
        }

        private void OnHasSelected(SelectExitEventArgs args)
        {
            if (!_hasSelected)
            {
                _hasSelected = true;
                StateMachine.OnLogic();
            }
        }

        public void SetInitState(ItemState state)
        {
            StateMachine.SetStartState(state);
            StateMachine.Init();
            StateMachine.OnLogic();
        }
    }
}