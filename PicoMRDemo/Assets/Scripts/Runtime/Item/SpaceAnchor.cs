/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;

public class SpaceAnchor : MonoBehaviour
{
    private XRBaseInteractable interactable;
    [SerializeField]
    public TextMeshProUGUI labelText;

    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    // Start is called before the first frame update
    protected void OnEnable()
    {
        interactable = GetComponent<XRBaseInteractable>();
        interactable.firstHoverEntered.AddListener(OnFirstHoverEntered);
        interactable.lastHoverExited.AddListener(OnLastHoverExited);
    }
    private void Update()
    {
        UpdateMeshLabel(this.transform.position);
    }

    public void UpdateMeshLabel(Vector3 vertices)
    {
        string xVal = vertices.x.ToString("F3");
        string yVal = vertices.y.ToString("F3");
        string zVal = vertices.z.ToString("F3");

        string newLabel = $"{tag}\n({xVal}, {yVal}, {zVal})";
        labelText.text = newLabel;
    }
    
    protected virtual void OnFirstHoverEntered(HoverEnterEventArgs args) => UpdateColor();

    protected virtual void OnLastHoverExited(HoverExitEventArgs args) => UpdateColor();
    
    protected void UpdateColor()
    {
        if (interactable.isHovered)
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.material.SetColor(EmissionColor, Color.yellow);
            }
        }
        else
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.material.SetColor(EmissionColor, Color.clear);
            }
        }
    }
}
