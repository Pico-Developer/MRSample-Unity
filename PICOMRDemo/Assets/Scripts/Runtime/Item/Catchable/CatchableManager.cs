/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

namespace PicoMRDemo.Runtime.Runtime.Item
{
    public class CatchableManager : ICatchableManager
    {
        public event Action<ICatchable> OnSetCatchable;
        public void Catch(GameObject owner, ICatchable catchable)
        {
            catchable.CatchObject = owner;
            var handler = catchable.GameObject.GetComponent<SolverHandler>();
            var rig = catchable.GameObject.GetComponent<Rigidbody>();
            var manipulator = catchable.GameObject.GetComponent<ObjectManipulator>();
            var orbital = catchable.GameObject.GetComponent<Orbital>();
            var collider = manipulator.colliders[0];
            collider.enabled = false;
            manipulator.AllowedManipulations = TransformFlags.None;
            rig.isKinematic = true;
            handler.UpdateSolvers = true;
            handler.TransformOverride = owner.transform;
            orbital.LocalOffset = new Vector3(0f, 0.1f, 0.1f);
        }

        public void Uncatch(ICatchable catchable)
        {
            var handler = catchable.GameObject.GetComponent<SolverHandler>();
            var rig = catchable.GameObject.GetComponent<Rigidbody>();
            var manipulator = catchable.GameObject.GetComponent<ObjectManipulator>();
            var orbital = catchable.GameObject.GetComponent<Orbital>();
            var collider = manipulator.colliders[0];
            var owner = catchable.CatchObject;
            handler.TransformOverride = null;
            handler.UpdateSolvers = false;
            rig.isKinematic = false;
            manipulator.AllowedManipulations = ~TransformFlags.None;
            catchable.CatchObject = null;
            collider.enabled = true;
            orbital.LocalOffset = Vector3.zero;
            var vel = owner.transform.TransformDirection(new Vector3(0f, 0f, 1f));
            rig.velocity = vel;
        }

        public void SetCatchable(ICatchable catchable)
        {
            OnSetCatchable?.Invoke(catchable);
        }
    }
}