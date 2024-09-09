/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
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
        private readonly ulong _handle;
        private IList<PxrSceneComponentType> _componentTypeFlags;
        
        public AnchorData(ulong handle, Guid uuid)
        {
            _handle = handle;
            Uuid = uuid;
        }

        public ulong Handle => _handle;

        public Guid Uuid { get; }

        public Vector3 Position
        {
            get
            {
                PXR_MixedReality.LocateAnchor(Handle, out var anchorPosition, out var anchorRotation);
                return anchorPosition;
            }
        }

        public Quaternion Rotation
        {
            get
            {
                PXR_MixedReality.LocateAnchor(Handle, out var anchorPosition, out var anchorRotation);
                return anchorRotation;
            }
        }

        public PxrSemanticLabel SceneLabel
        {
            get
            {
                PXR_MixedReality.GetSceneSemanticLabel(_handle, out var label);
                return label;
            }
        }

        public IList<PxrSceneComponentType> ComponentTypeFlagsList
        {
            get
            {
                if (_componentTypeFlags != null)
                    return _componentTypeFlags;
                var result0 = PXR_MixedReality.GetSceneAnchorComponentTypes(_handle, out var types);
                if (result0 == PxrResult.SUCCESS)
                {
                    _componentTypeFlags = types.ToList();
                }
                return _componentTypeFlags;
            }
        }
        
        public SceneBox2DData SceneBox2DData
        {
            get
            {
                PXR_MixedReality.GetSceneBox2DData(_handle, out var center, out var extent);
                return new SceneBox2DData()
                {
                    Center = center,
                    Extent = extent
                };
            }
        }
        
        public SceneBox3DData SceneBox3DData
        {
            get
            {
                PXR_MixedReality.GetSceneBox3DData(_handle, out var center,out var rotation,out var extent);
                return new SceneBox3DData()
                {
                    Center = center,
                    Rotation = rotation,
                    Extent = extent
                };
            }
        }
        
        public ScenePolygonData ScenePolygonData
        {
            get
            {
                PXR_MixedReality.GetScenePolygonData(_handle, out var vertices);
                var verVector3S = Array.ConvertAll(vertices, v2 => new Vector3(v2.x, v2.y, 0f));
                return new ScenePolygonData()
                {
                    Vertices = verVector3S
                };
            }
        }
    }
}