/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace PicoDemoUI
{
    public class ScrollRectListVertical : MonoBehaviour
{
    
    [Header("InitObj")]
    [Tooltip("The pool of list item Prefab.")]
    [SerializeField]
    private GameObject prefab;

    [SerializeField]
    private ScrollRect scrollRect;

    [Tooltip("If using a Vertical layout, this represents the number of Columns for the layout")]
    [SerializeField]
    private int layoutRowsOrColumns = 1;

    [Tooltip("(Optional) The size of each layout cell. If an axis is 0, VirtualizedList will pull this dimension directly from the prefab's RectTransform.")]
    [SerializeField]
    private Vector2 cellSize;

    [Tooltip("This is the spacing between each layout cell in local units.")]
    [SerializeField]
    private float gutter;

    [Tooltip("This is the spacing between the area where the layout cells are, and the boundaries of the ScrollRect's Content area.")]
    [SerializeField]
    private float margin;

    [Tooltip("When scrolling to the end of the list, it can be nice to show some empty space to indicate the end of the list. This is in local units.")]
    [SerializeField]
    private float trailingSpace;

    // Header State

    [Header("State")]
    [Tooltip("This is mostly for debug and inspection. Item Count should be driven by using SetItemCount in the API.")]
    [SerializeField]
    private int itemCount = 10;

    private float scroll = 0;
    private float requestScroll;

    private int screenCount;
    private float viewSize;
    private float contentStart;
    private float layoutPrefabSize;
    private bool initialized = false;

    private int visibleStart;
    private int visibleEnd;
    private bool visibleValid;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private Dictionary<int, GameObject> poolDict = new Dictionary<int, GameObject>();
    
    /// </summary>
    public float Scroll
    {
        get => scroll;
        set
        {
            requestScroll = value;
            UpdateScrollView(requestScroll);
        }
    }
    
    public int ItemCount => itemCount;
    
    public int RowsOrColumns => layoutRowsOrColumns;
    
    public int PartiallyVisibleCount => Mathf.CeilToInt(viewSize / layoutPrefabSize) * layoutRowsOrColumns;
    
    public int TotallyVisibleCount => Mathf.FloorToInt(viewSize / layoutPrefabSize) * layoutRowsOrColumns;
    
    public float ScreenItemCountF => (viewSize / layoutPrefabSize) * layoutRowsOrColumns;
    
    public float MaxScroll => ((scrollRect.content.rect.height - viewSize) / layoutPrefabSize) * layoutRowsOrColumns;
    
    public Action<GameObject, int> OnVisible { get; set; }
    
    public Action<GameObject, int> OnInvisible { get; set; }
    
    
    private Vector3 ItemLocation(int index) =>new Vector3(
            scrollRect.content.rect.xMin + (margin + (cellSize.x + gutter) * (index % layoutRowsOrColumns)),
            contentStart - (margin + (index / layoutRowsOrColumns) * layoutPrefabSize),
            0);
    
    private float PosToScroll(float pos) => ((pos - (margin - gutter)) / layoutPrefabSize) * layoutRowsOrColumns;
    
    private float ScrollToPos(float scroll) => (scroll * (layoutPrefabSize / layoutRowsOrColumns)) + (margin - gutter);
    
    private void OnValidate()
    {
        if (initialized == false) { return; }

        if (margin < gutter) { margin = gutter; }

        visibleValid = false;
        Initialize();
    }

    private void Start()
    {
        visibleValid = false;
        scrollRect = scrollRect == null ? GetComponent<ScrollRect>() : scrollRect;
        BakeCachedValues();
        
        StartCoroutine(EndOfFrameInitialize());
    }

    private IEnumerator EndOfFrameInitialize()
    {
        yield return new WaitForEndOfFrame();

        Initialize();
    }

    private void BakeCachedValues()
    {
        RectTransform prefabRect = prefab.GetComponent<RectTransform>();
        if (cellSize.x == 0) cellSize.x = prefabRect.rect.width;
        if (cellSize.y == 0) cellSize.y = prefabRect.rect.height;
        layoutPrefabSize = cellSize.y + gutter;
    }

    private void Initialize()
    {
        BakeCachedValues();


        Vector2 topCenter = new Vector2(0.5f, 1);
        scrollRect.content.anchorMin = topCenter;
        scrollRect.content.anchorMax = topCenter;
        scrollRect.content.pivot = topCenter;
        scrollRect.content.sizeDelta = new Vector2(
            2 * margin + layoutRowsOrColumns * cellSize.x + (layoutRowsOrColumns - 1) * gutter,
            2 * margin + (itemCount / layoutRowsOrColumns) * layoutPrefabSize + trailingSpace);
        viewSize = scrollRect.viewport.rect.height;
        contentStart = scrollRect.content.rect.yMax;
       
        screenCount = Mathf.CeilToInt(viewSize / layoutPrefabSize) * layoutRowsOrColumns;

        InitializePool();

        initialized = true;
        scrollRect.onValueChanged.AddListener(v => UpdateScroll(PosToScroll(scrollRect.content.localPosition.y)));

        UpdateScrollView(requestScroll);
    }

    private void InitializePool()
    {
        // Support resetting everything from OnValidate
        foreach (int i in poolDict.Keys.ToArray())
        {
            MakeInvisible(i);
        }
        poolDict.Clear();
        while (pool.Count > 0)
        {
            Destroy(pool.Dequeue());
        }
        visibleStart = -1;
        visibleEnd = -1;

        // Create the pool of prefabs
        int poolSize = screenCount + layoutRowsOrColumns;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject go = Instantiate(prefab, ItemLocation(-(i + 1)), Quaternion.identity, scrollRect.content);
            pool.Enqueue(go);
        }
    }

    private void UpdateScrollView(float newScroll)
    {
        newScroll = Mathf.Clamp(newScroll, 0, MaxScroll);

        Vector3 pos = scrollRect.content.localPosition;
        scrollRect.content.localPosition = new Vector3(pos.x, ScrollToPos(newScroll), pos.z);
        UpdateScroll(newScroll);
    }

    private void UpdateScroll(float newScroll)
    {
        if ((scroll == newScroll && visibleValid == true) || initialized == false) { return; }
        scroll = newScroll;
        visibleValid = true;

        // Based on this scroll, calculate the new relevant ranges of
        // indices
        float paddedScroll = newScroll - (margin / layoutPrefabSize);
        int newVisibleStart = Math.Max(0, ((int)paddedScroll / layoutRowsOrColumns) * layoutRowsOrColumns);
        int newVisibleEnd = Math.Min(itemCount, Mathf.CeilToInt(paddedScroll / layoutRowsOrColumns + (viewSize / layoutPrefabSize)) * layoutRowsOrColumns);

        // If it's the same as we already have, then we can just stop here!
        if (newVisibleStart == visibleStart &&
            newVisibleEnd == visibleEnd) return;

        // Demote all items that are no longer relevant
        for (int i = visibleStart; i < visibleEnd; i++)
        {
            bool wasVisible = i >= visibleStart && i < visibleEnd;
            bool remainsVisible = i >= newVisibleStart && i < newVisibleEnd;
            if (wasVisible == true && remainsVisible == false) { MakeInvisible(i); }
        }

        // Promote all items that are now relevant
        for (int i = newVisibleStart; i < newVisibleEnd; i++)
        {
            bool wasVisible = i >= visibleStart && i < visibleEnd;
            bool nowVisible = i >= newVisibleStart && i < newVisibleEnd;
            if (wasVisible == false && nowVisible == true) { MakeVisible(i); }
        }

        // These are now the current index ranges!
        visibleStart = newVisibleStart;
        visibleEnd = newVisibleEnd;
    }

    private void MakeInvisible(int i)
    {
        if (TryGetVisible(i, out GameObject go) == false) { return; }

        OnInvisible?.Invoke(go, i);
        poolDict.Remove(i);
        go.SetActive(false);
        pool.Enqueue(go);
    }

    private void MakeVisible(int i)
    {
        GameObject go = pool.Dequeue();
        go.transform.localPosition = ItemLocation(i);
        go.transform.localRotation = Quaternion.identity;
        go.SetActive(true);
        poolDict.Add(i, go);
        OnVisible?.Invoke(go, i);
    }
    
    public bool TryGetVisible(int i, out GameObject visibleObject)
    {
        if (i >= visibleStart && i < visibleEnd)
        {
            visibleObject = poolDict[i];
            return true;
        }
        visibleObject = null;
        return false;
    }
    
    public void SetItemCount(int newCount)
    {
        itemCount = newCount;
        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, margin + itemCount * layoutPrefabSize + trailingSpace);
        contentStart = scrollRect.content.rect.yMax;

        visibleValid = false;
        UpdateScrollView(scroll);
    }
}
}

