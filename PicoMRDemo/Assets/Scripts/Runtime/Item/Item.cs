/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using FSM;
using PicoMRDemo.Runtime.Entity;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace PicoMRDemo.Runtime.Runtime.Item
{
    
    public class Item :MonoBehaviour, IItem
    {
        public GameObject GameObject => gameObject;
        public IEntity Entity { get; set; }
        public IEntityManager EntityManager { get; set; }
        public ItemState ItemState => StateMachine.ActiveState.name;
        public ulong Id { get; set; }
        
        protected XRSimpleInteractable simpleInteractable;
        protected XRGrabInteractable grabInteractable;
        protected StateMachine<ItemState> StateMachine;

        private bool _useGravity;
        private bool _hasSelected;

        protected virtual void Awake()
        {
            simpleInteractable = GetComponentInChildren<XRSimpleInteractable>();
            if(simpleInteractable == null)
                grabInteractable = GetComponentInChildren<XRGrabInteractable>();
            StateMachine = new StateMachine<ItemState>();
            if (this.GetComponent<Rigidbody>())
            {
                _useGravity = this.GetComponent<Rigidbody>().useGravity;
            }
            InitStateMachine();
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
            
            StateMachine.AddState(ItemState.Normal, new State<ItemState>(onEnter:state =>
            {
                if (this.GetComponent<Rigidbody>())
                {
                    this.GetComponent<Rigidbody>().useGravity = _useGravity;
                }
            }));
            StateMachine.AddState(ItemState.Float, new State<ItemState>(onEnter: state =>
            {
                if (this.GetComponent<Rigidbody>())
                {
                    this.GetComponent<Rigidbody>().useGravity = false;
                }
            }));
            StateMachine.AddTransition(new Transition<ItemState>(ItemState.Float, ItemState.Normal, (transition) => _hasSelected));
        }

        private void RegisterEvent()
        {
            if (simpleInteractable != null)
            {
                simpleInteractable.selectExited.AddListener(OnHasSelected);
                simpleInteractable.selectEntered.AddListener(OnSelectedEnter);
            }else if (grabInteractable != null)
            {
                grabInteractable.selectEntered.AddListener(OnSelectedEnter);
                grabInteractable.selectExited.AddListener(OnHasSelected);
            }
            
        }

        private void UnregisterEvent()
        {
            if (simpleInteractable != null)
            {
                simpleInteractable.selectEntered.RemoveListener(OnSelectedEnter);
                simpleInteractable.selectExited.RemoveListener(OnHasSelected);
            }
            else if (grabInteractable != null)
            {
                grabInteractable.selectEntered.RemoveListener(OnSelectedEnter);
                grabInteractable.selectExited.RemoveListener(OnHasSelected);
            }
        }
        
        private void OnSelectedEnter(SelectEnterEventArgs args)
        {
            if (this.GetComponent<Rigidbody>())
            {
                this.GetComponent<Rigidbody>().useGravity = _useGravity;
            }
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