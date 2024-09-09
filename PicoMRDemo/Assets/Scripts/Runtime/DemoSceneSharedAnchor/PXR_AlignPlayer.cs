/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PXR_AlignPlayer : MonoBehaviour
{
    [SerializeField]
    public UnityEvent onAlign;

    [SerializeField]
    private Transform player;

    public static PXR_AlignPlayer Instance;
    private SharedAnchor _currentAlignmentAnchor;
    private Coroutine _realignCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    /// <summary>
    /// Set up alignment anchor points and start aligning
    /// </summary>
    /// <param name="anchor">The Cloud anchor point to be set</param>
    public void SetAlignmentAnchor(SharedAnchor anchor)
    {
        if (_realignCoroutine!= null)
        {
            StopCoroutine(_realignCoroutine);
        }
        
        if (anchor)
        {
            _realignCoroutine = StartCoroutine(RealignRoutine(anchor));
        }
    }


    private IEnumerator RealignRoutine(SharedAnchor anchor)
    {
        if (_currentAlignmentAnchor != null)
        {
            _currentAlignmentAnchor.IsSelectedForAlign = false;

            player.position = Vector3.zero;
            player.eulerAngles = Vector3.zero;

            yield return null;
        }

        var anchorTransform = anchor.transform;

        if (player)
        {

            player.SetParent(anchor.transform);
            player.localPosition = anchorTransform.InverseTransformPoint(Vector3.zero);
            player.localEulerAngles = new Vector3(0, -anchorTransform.eulerAngles.y, 0);
        }
        
        anchor.IsSelectedForAlign = true;
        _currentAlignmentAnchor = anchor;

        onAlign?.Invoke();
    }
}
