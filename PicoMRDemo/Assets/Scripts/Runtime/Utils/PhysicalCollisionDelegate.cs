/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;

namespace PicoMRDemo.Runtime.Utils
{
    public class PhysicalCollisionDelegate: MonoBehaviour
    {
        public delegate void CollisionDelegateHandler(Collision collision);
        public event CollisionDelegateHandler OnCollisionEnterEvent;
        public event CollisionDelegateHandler OnCollisionStayEvent;
        public event CollisionDelegateHandler OnCollisionExitEvent;

        public delegate void TriggerDelegateHandler(Collider collider, PhysicalCollisionDelegate collisionDelegate);
        public event TriggerDelegateHandler OnTriggerEnterEvent;
        public event TriggerDelegateHandler OnTriggerStayEvent;
        public event TriggerDelegateHandler OnTriggerExitEvent;
        
        private void OnCollisionEnter(Collision collision)
        {
            OnCollisionEnterEvent?.Invoke(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            OnCollisionStayEvent?.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            OnCollisionExitEvent?.Invoke(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            OnTriggerEnterEvent?.Invoke(other, this);
        }

        private void OnTriggerStay(Collider other)
        {
            OnTriggerStayEvent?.Invoke(other, this);
        }

        private void OnTriggerExit(Collider other)
        {
            OnTriggerExitEvent?.Invoke(other, this);
        }
        
        public void AddCollisionEvent(CollisionDelegateHandler enter = null, CollisionDelegateHandler stay = null, CollisionDelegateHandler exit = null)
        {
            OnCollisionEnterEvent += enter;
            OnCollisionStayEvent += stay;
            OnCollisionExitEvent += exit;
        }

        public void RemoveCollisionEvent(CollisionDelegateHandler enter = null, CollisionDelegateHandler stay = null, CollisionDelegateHandler exit = null)
        {
            OnCollisionEnterEvent -= enter;
            OnCollisionStayEvent -= stay;
            OnCollisionExitEvent -= exit;
        }

        public void AddTriggerEvent(TriggerDelegateHandler enter = null, TriggerDelegateHandler stay = null, TriggerDelegateHandler exit = null)
        {
            OnTriggerEnterEvent += enter;
            OnTriggerStayEvent += stay;
            OnTriggerExitEvent += exit;
        }

        public void RemoveTriggerEvent(TriggerDelegateHandler enter = null, TriggerDelegateHandler stay = null, TriggerDelegateHandler exit = null)
        {
            OnTriggerEnterEvent -= enter;
            OnTriggerStayEvent -= stay;
            OnTriggerExitEvent -= exit;
        }
    }
}