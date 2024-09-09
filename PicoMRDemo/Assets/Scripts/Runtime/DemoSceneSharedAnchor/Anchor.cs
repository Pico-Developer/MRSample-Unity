/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Runtime.InteropServices;
using Pico.Platform;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.XR.PXR;
using UnityEngine.PlayerLoop;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(XRSimpleInteractable))]
public class SharedAnchor : MonoBehaviour
{
    private XRBaseInteractable interactable;

    [HideInInspector]
    public ulong anchorHandle;
    [HideInInspector]
    public Guid anchorUuid;
    [SerializeField]
    private Text anchorID;
    [SerializeField]
    private TextMeshProUGUI uuidText;
    [SerializeField]
    private TextMeshProUGUI posText;
    [SerializeField]
    private TextMeshProUGUI playerIDText;

    [SerializeField]
    private Image saveIcon;
    [SerializeField]
    private Image shareIcon;
    [SerializeField]
    private Image alignIcon;

    [SerializeField]
    private GameObject uiCanvas;
    [SerializeField]
    private GameObject uiCanvas2;

    [SerializeField]
    private Color grayColor;

    [SerializeField]
    private Color greenColor;
    
    private float _delayCloseTime = 5f;
    private float _closeTime = -1f;
        


    [SerializeField] private Button btnSaveAnchorLocal;
    [SerializeField] private Button btnSaveAnchorCloud;
    [SerializeField] private Button btnDestroyAnchor;
    [SerializeField] private Button btnDeleteAnchor;
    [SerializeField] private Button btnAlignAnchor;
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    private void Awake()
    {
        uiCanvas.SetActive(false);
        uiCanvas.GetComponent<Canvas>().worldCamera = Camera.main;

        btnSaveAnchorLocal.onClick.AddListener(OnBtnPressedPersistLocal);
        btnSaveAnchorCloud.onClick.AddListener(OnBtnPressedPersistCloud);
        btnDestroyAnchor.onClick.AddListener(OnBtnPressedDestroy);
        btnDeleteAnchor.onClick.AddListener(OnBtnPressedUnPersist);
        btnAlignAnchor.onClick.AddListener(OnBtnPressedAlign);
    }

    protected void OnEnable()
    {
        interactable = GetComponent<XRBaseInteractable>();
        interactable.firstHoverEntered.AddListener(OnFirstHoverEntered);
        interactable.lastHoverExited.AddListener(OnLastHoverExited);
    }


    private void Update()
    {
        if (uiCanvas.activeSelf)
        {
            var position = uiCanvas.transform.position;
            var position2 = Camera.main.transform.position;
            uiCanvas.transform.LookAt(new Vector3(position.x * 2 - position2.x, position.y * 2 - position2.y, position.z * 2 - position2.z), Vector3.up);
            if (_closeTime >= 0)
            {
                _closeTime += Time.deltaTime;
                if (_closeTime > _delayCloseTime)
                {
                    uiCanvas.SetActive(interactable.isHovered);
                    _closeTime = -1f;
                }
            }
        }

        if (uiCanvas2.activeSelf)
        {
            var position = uiCanvas2.transform.position;
            var position2 = Camera.main.transform.position;
            uiCanvas2.transform.LookAt(new Vector3(position.x * 2 - position2.x, position.y * 2 - position2.y, position.z * 2 - position2.z), Vector3.up);
        }

        var position1 = transform.position;
        string posLog = $"Position:({position1.x:F3}, {position1.y:F3}, {position1.z:F3})";
        posText.text = posLog;
    }

    private void LateUpdate()
    {
#if !UNITY_EDITOR
        var result = PXR_MixedReality.LocateAnchor(anchorHandle, out var position, out var rotation);
        if (result == PxrResult.SUCCESS)
        {
            transform.position = position;
            transform.rotation = rotation;
            string rotLog = $"LocateAnchor Position:({transform.position.x:F3}, {transform.position.y:F3}, {transform.position.z:F3})";
        }
#endif
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
            uiCanvas.SetActive(interactable.isHovered);
            _closeTime = -1f;
        }
        else
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.material.SetColor(EmissionColor, Color.clear);
            }
            _closeTime = 0f;
        }
    }
    

    private async void OnBtnPressedPersistLocal()
    {
        var result = await PXR_MixedReality.PersistSpatialAnchorAsync(anchorHandle);
        PXR_SharedAnchorManager.Instance.SetExecuteResult("PersistSpatialAnchorAsync:" + result.ToString());
        if (result == PxrResult.SUCCESS)
        {
            IsSavedLocally = true;
        }
    }

    private async void OnBtnPressedPersistCloud()
    {
        PXR_SharedAnchorManager.Instance.SetLoadingImg(true);
        var result = await PXR_MixedReality.UploadSpatialAnchorAsync(anchorHandle);
        PXR_SharedAnchorManager.Instance.SetExecuteResult($"ShareSpatialAnchorAsync:{result.ToString()} uuid:{result.uuid.ToByteArray()}");
        if (result.result == PxrResult.SUCCESS)
        {
            PXR_SharedAnchorManager.Instance.ShareAnchorToOthers(result.uuid);
        }
        PXR_SharedAnchorManager.Instance.SetLoadingImg(false);
        IsSelectedForShare = true;
    }

    private void OnBtnPressedDestroy()
    {
        PXR_SharedAnchorManager.Instance.DestroySpatialAnchor(anchorHandle);
    }

    private async void OnBtnPressedUnPersist()
    {
        var result = await PXR_MixedReality.UnPersistSpatialAnchorAsync(anchorHandle);
        PXR_SharedAnchorManager.Instance.SetExecuteResult("UnPersistSpatialAnchorAsync:" + result.ToString());
        if (result == PxrResult.SUCCESS)
        {
            OnBtnPressedDestroy();
        }
    }

    private void OnBtnPressedAlign()
    {
        PXR_AlignPlayer.Instance.SetAlignmentAnchor(this);
    }

    public void SetAnchorHandle(ulong handle)
    {
        anchorHandle = handle;
        anchorID.text = "Handle: " + anchorHandle;
    }

    public void SetAnchorUuid(Guid uuid)
    {
        anchorUuid = uuid;
        uuidText.text = anchorUuid.ToString();
    }
    
    public void SetAnchorSource(string name)
    {
        playerIDText.text = name;
    }


    public bool IsSavedLocally
    {
        set
        {
            if (saveIcon != null)
            {
                saveIcon.color = value ? greenColor : grayColor;
            }
        }
    }

    private bool IsSelectedForShare
    {
        set
        {
            if (shareIcon != null)
            {
                shareIcon.color = value ? greenColor : grayColor;
            }
        }
    }

    public bool IsSelectedForAlign
    {
        set
        {
            if (alignIcon != null)
            {
                alignIcon.color = value ? greenColor : grayColor;
            }
        }
    }

}
