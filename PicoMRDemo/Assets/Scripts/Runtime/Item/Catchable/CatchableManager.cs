/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using PicoMRDemo.Runtime.Pet;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace PicoMRDemo.Runtime.Runtime.Item
{
    public class CatchableManager : ICatchableManager
    {
        public event Action<ICatchable> OnSetCatchable;
        public void Catch(GameObject owner, ICatchable catchable)
        {
            if (catchable.IsCatch)
            {
                Uncatch(catchable);
            }
            catchable.CatchObject = owner;
            catchable.PetAgent = owner.GetComponent<PetAgent>();
            var rig = catchable.GameObject.GetComponent<Rigidbody>();
            var collider = catchable.GameObject.GetComponent<XRGrabInteractable>().colliders[0];
            collider.enabled = false;
            rig.isKinematic = true;
        }
        

        public void Uncatch(ICatchable catchable)
        {
            var rig = catchable.GameObject.GetComponent<Rigidbody>();
            var collider = catchable.GameObject.GetComponent<XRGrabInteractable>().colliders[0];
            var owner = catchable.CatchObject;
            rig.isKinematic = false;
            catchable.CatchObject = null;
            collider.enabled = true;
            var vel = owner.transform.TransformDirection(new Vector3(0f, 0f, 1f));
            rig.velocity = vel;
        }

        public void SetCatchable(ICatchable catchable)
        {
            OnSetCatchable?.Invoke(catchable);
        }
    }
}