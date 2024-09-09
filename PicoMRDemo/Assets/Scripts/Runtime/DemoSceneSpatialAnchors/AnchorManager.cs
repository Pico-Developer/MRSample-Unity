using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.InputSystem;

public class AnchorManager : MonoBehaviour
{
    [SerializeField]
    private InputActionReference rightGrip;
    [SerializeField]
    private GameObject anchorPreview;
    [SerializeField]
    private GameObject anchorPrefab;
    [SerializeField]
    private float maxDriftDelay = 0.5f;

    private float currrDriftDelay = 0f;

    private Dictionary<ulong, GameObject> anchorMap= new Dictionary<ulong, GameObject>();
    private Anchor _anchor;

    // Start is called before the first frame update
    void Start()
    {
        _anchor = anchorPreview.GetComponent<Anchor>();
        StartSpatialAnchorProvider();
    }
    
    private async void StartSpatialAnchorProvider()
    {
        var result0 = await PXR_MixedReality.StartSenseDataProvider(PxrSenseDataProviderType.SpatialAnchor);
        Debug.unityLogger.Log($"StartSenseDataProvider: {result0}");
    }
    
    // Update is called once per frame
    void Update()
    {
        if (anchorPreview.activeSelf)
        {
            _anchor.UpdateMeshLabel("Preview", anchorPreview.transform.position);
        }
    }

    private void OnEnable()
    {
        rightGrip.action.started += OnRightGripPressed;
        rightGrip.action.canceled += OnRightGripReleased;
    }

    private void OnDisable()
    {
        rightGrip.action.started -= OnRightGripPressed;
        rightGrip.action.canceled -= OnRightGripReleased;
    }

    private void FixedUpdate()
    {
        HandleSpatialDrift();
    }
    
    //called on action.started
    private void OnRightGripPressed(InputAction.CallbackContext callback)
    {
        ShowAnchorPreview();
    }

    //called on action.release
    private void OnRightGripReleased(InputAction.CallbackContext callback)
    {
        CreateAnchor();
    }

    private void ShowAnchorPreview()
    {
        //Show anchor
        anchorPreview.SetActive(true);
    }

    private async void CreateAnchor()
    {
        //hide anchor
        anchorPreview.SetActive(false);
        //Use Spatial Anchor Api to create anchor
        //This will  trigger AnchorEntityCreatedEvent
        var result = await PXR_MixedReality.CreateSpatialAnchorAsync(anchorPreview.transform.position, anchorPreview.transform.rotation);
        if (result.result == PxrResult.SUCCESS)
        {
            GameObject anchorObject = Instantiate(anchorPrefab);
            anchorObject.transform.rotation = anchorPreview.transform.rotation;
            anchorObject.transform.position = anchorPreview.transform.position;
            anchorObject.GetComponent<Anchor>().UpdateMeshLabel($"Handle:{result.anchorHandle}", anchorObject.transform.position);
            //Keep track of our anchors to handle spatial drift
            anchorMap.Add(result.anchorHandle, anchorObject);
        }
    }
    

    private void HandleSpatialDrift()
    {
        //if no anchors, dont need to handle spatial
        if (anchorMap.Count == 0)
            return;

        currrDriftDelay += Time.deltaTime;
        if(currrDriftDelay >= maxDriftDelay)
        {
            currrDriftDelay = 0;
            foreach (var handlePair in anchorMap)
            {
                var handle = handlePair.Key;
                var anchorObj = handlePair.Value;

                if (handle == UInt64.MinValue)
                {
                    continue;
                }
                PXR_MixedReality.LocateAnchor(handle, out var position, out var rotation);
                anchorObj.transform.rotation = rotation;
                anchorObj.transform.position = position;
            }
        }
    }

}
