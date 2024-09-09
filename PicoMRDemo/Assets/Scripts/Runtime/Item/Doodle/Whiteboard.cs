/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using Unity.Collections;
using UnityEngine.XR.Interaction.Toolkit;

namespace PicoMRDemo.Runtime.Runtime.Item
{
    public interface IGraffitiable
    {
        Transform transform { get; }
        Texture2D Drawingboard { get; }
        void ClearAllGraffitiable();
    }
   
    public class Whiteboard : XRBaseInteractable, IGraffitiable
    {
        public int TextureSize = 512;

        public Texture2D Drawingboard => _texture;

        private Texture2D _texture;
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

        private void Start()
        {
            _texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
            _texture.hideFlags = HideFlags.HideAndDontSave;
            Renderer rend = GetComponent<Renderer>();
            rend.material.SetTexture(BaseMap, _texture);
            ClearAllGraffitiable();
        }
        
        [ContextMenu("ClearAllGraffitiable")]
        public void ClearAllGraffitiable()
        {
            Color32 color = new Color32(0, 0, 0, 0);
            NativeArray<Color32> data = _texture.GetRawTextureData<Color32>();
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = color;
            }
            _texture.Apply(true);

        }
        
        protected override void OnDestroy()
        {
            Destroy(_texture);
            base.OnDestroy();
        }
    }
}
