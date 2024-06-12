﻿/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Linq;
using UnityEngine;
using LightType = UnityEngine.LightType;

namespace PicoMRDemo.Runtime.Service
{
    public class LightManager : ILightManager
    {
        public Light MainLight => Light.FindObjectOfType<Light>();
        public Light SpotLight => Light.FindObjectsByType<Light>(FindObjectsSortMode.None).First(x => x.type == LightType.Spot);
        public readonly float MinHeight = 2.8f;
        public readonly float MaxHeight = 6f;
        public readonly float MinIntensity = 10;
        public readonly float MaxIntensity = 25;
        public void SetMainLightPositionByCeilingPosition(Vector3 ceilingPosition)
        {
            SpotLight.gameObject.transform.position = ceilingPosition + new Vector3(0f, 0.5f, 0f);
        }

        public void SetMainLightIntensityByCeilingHeight(float ceilingHeight)
        {
            var intensity = (Mathf.Clamp(ceilingHeight, MinHeight, MaxHeight) - MinHeight) / (MaxHeight - MinHeight) * (MaxIntensity - MinIntensity) + MinIntensity;
            SpotLight.intensity = intensity;
        }
    }
}