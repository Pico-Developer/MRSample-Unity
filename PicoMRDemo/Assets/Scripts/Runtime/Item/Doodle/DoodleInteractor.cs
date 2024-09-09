/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace PicoMRDemo.Runtime.Runtime.Item
{
    public class DoodleInteractor : XRBaseInteractor
    {
        public Color32 drawingColor = new Color32(0, 0, 0, 255);

        public int BrushSize = 1;
        
        private HashSet<IGraffitiable> graffitiables = new HashSet<IGraffitiable>();
        
        private Dictionary<IGraffitiable, Vector2> lastPositions = new Dictionary<IGraffitiable, Vector2>();

        protected override void OnDisable()
        {
            base.OnDisable();
            lastPositions.Clear();
        }

        public override void ProcessInteractor(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractor(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                foreach (var target in graffitiables)
                {
                    Draw(target);
                }
            }
        }
        
        public void ChangeColor(Color32 color)
        {
            drawingColor = color;
        }

        public void Draw(IGraffitiable graffitiable)
        {
            var board = graffitiable.Drawingboard;                     
            var localTouchPosition = graffitiable.transform.InverseTransformPoint(attachTransform.position);                   

            NativeArray<Color32> data = board.GetRawTextureData<Color32>();
            
            Vector2 uvTouchPosition = new Vector2(localTouchPosition.x + 0.5f, localTouchPosition.y + 0.5f);
    
            Vector2 pixelCoordinate = Vector2.Scale(new Vector2(board.width, board.height), uvTouchPosition);

            if (!lastPositions.TryGetValue(graffitiable, out Vector2 lastPosition))
            {
                lastPosition = pixelCoordinate;
            }
                       
            for (int i = 0; i < Vector2.Distance(pixelCoordinate, lastPosition); i++)
            {
                DrawSplat(Vector2.Lerp(lastPosition, pixelCoordinate, i / Vector2.Distance(pixelCoordinate, lastPosition)), data, board.width);
            }
            
            lastPositions[graffitiable] = pixelCoordinate;

            board.Apply(false);
           
        }
        
        private void DrawSplat(Vector2 pixelCoordinate, NativeArray<Color32> data, int textureWidth)
        {
            var pixelIndexX = Mathf.RoundToInt(pixelCoordinate.x);
            var pixelIndexY = Mathf.RoundToInt(pixelCoordinate.y);
            int pixelIndex = pixelIndexX + textureWidth * pixelIndexY;
        
            for (int y = -1 * BrushSize; y < BrushSize + 1; y++)
            {
                for (int x = -1 * BrushSize; x < BrushSize + 1; x++)
                {
                    var targetPixelIndexX = x;
                    if (pixelIndexX+x >= textureWidth)
                    {
                        targetPixelIndexX = textureWidth - 1 - pixelIndexX;
                    }
                    else if (pixelIndexX + x < 0)
                    {
                        targetPixelIndexX = -pixelIndexX;
                    }
                    data[Mathf.Clamp(pixelIndex + targetPixelIndexX + (textureWidth * y), 0, data.Length - 1)] = drawingColor;
                }
            }
        }
        
        void OnTriggerEnter(Collider other)
        {
            if (interactionManager.TryGetInteractableForCollider(other, out var associatedInteractable))
            {
                if (associatedInteractable.transform.TryGetComponent<IGraffitiable>(out var graffitiable))
                {
                    Debug.Log($"Add grafftiable {graffitiable.transform.parent.parent.parent.name} ");
                    graffitiables.Add(graffitiable);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (interactionManager.TryGetInteractableForCollider(other, out var associatedInteractable))
            {
                
                if (associatedInteractable.transform.TryGetComponent<IGraffitiable>(out var graffitiable))
                {
                    if (lastPositions.ContainsKey(graffitiable))
                    {
                        lastPositions.Remove(graffitiable);
                    }
                    graffitiables.Remove(graffitiable);
                }
            }
        }
    }
}
