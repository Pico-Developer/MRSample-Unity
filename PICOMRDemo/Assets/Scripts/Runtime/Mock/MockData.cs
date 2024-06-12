/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PicoMRDemo.Runtime.Data.Anchor;
using PicoMRDemo.Runtime.Entity;
using Unity.XR.PXR;
using UnityEngine;

namespace PicoMRDemo.Runtime.Mock
{
    public class MockAnchorData : IAnchorData
    {
        private Guid _uuid;
        public MockAnchorData(ulong handle, Guid guid)
        {
            Handle = handle;
            _uuid = guid;
        }
        public ulong Handle { get; private set; }
        public Guid Uuid { get; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public PxrSceneLabel SceneLabel { get; set; }
        public IList<PxrAnchorComponentTypeFlags> ComponentTypeFlagsList { get; }
        public VolumeInfo VolumeInfo { get; set; }
        public PlaneBoundaryInfo PlaneBoundaryInfo { get; set; }
        public PlanePolygonInfo PlanePolygonInfo { get; set; }
    }
}