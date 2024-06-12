/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.PXR;
using UnityEngine;

namespace PicoMRDemo.Runtime.Data.Anchor
{
    public class AnchorData : IAnchorData
    {
        private ulong _handle;
        private Guid _uuid;
        private IList<PxrAnchorComponentTypeFlags> _componentTypeFlags;
        
        public AnchorData(ulong handle, Guid uuid)
        {
            _handle = handle;
            _uuid = uuid;
        }

        public ulong Handle => _handle;

        public Guid Uuid => _uuid;

        public Vector3 Position
        {
            get
            {
                var result = PXR_MixedReality.GetAnchorPose(_handle, out var rot, out var pos);
                return pos;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                var result = PXR_MixedReality.GetAnchorPose(_handle, out var rot, out var pos);
                return rot;
            }
        }

        public PxrSceneLabel SceneLabel
        {
            get
            {
                PXR_MixedReality.GetAnchorSceneLabel(_handle, out var label);
                return label;
            }
        }

        public IList<PxrAnchorComponentTypeFlags> ComponentTypeFlagsList
        {
            get
            {
                if (_componentTypeFlags != null)
                    return _componentTypeFlags;
                PXR_MixedReality.GetAnchorComponentFlags(_handle, out var flags);
                if (flags != null)
                    _componentTypeFlags = flags.ToList();
                return _componentTypeFlags;
            }
        }

        public VolumeInfo VolumeInfo
        {
            get
            {
                PXR_MixedReality.GetAnchorVolumeInfo(_handle, out var center, out var extent);
                return new VolumeInfo()
                {
                    Center = center,
                    Extent = extent
                };
            }
        }

        public PlaneBoundaryInfo PlaneBoundaryInfo
        {
            get
            {
                PXR_MixedReality.GetAnchorPlaneBoundaryInfo(_handle, out var center, out var extent);
                return new PlaneBoundaryInfo()
                {
                    Center = center,
                    Extent = extent
                };
            }
        }

        public PlanePolygonInfo PlanePolygonInfo
        {
            get
            {
                PXR_MixedReality.GetAnchorPlanePolygonInfo(_handle, out var vertices);
                return new PlanePolygonInfo()
                {
                    Vertices = vertices
                };
            }
        }
    }
}