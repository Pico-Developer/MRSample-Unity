/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2024 PICO Developer
// SPDX-License-Identifier: MIT
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Pico.Platform;
using Pico.Platform.Models;
using PicoMRDemo.Runtime.UI;
using UnityEngine.UI;
using Unity.XR.PXR;
using UnityEngine;

public class PXR_SharedAnchorManager : MonoBehaviour
{
    private static PXR_SharedAnchorManager _instance = null;

    public static PXR_SharedAnchorManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PXR_SharedAnchorManager>();
            }

            return _instance;
        }
    }


    public Text ExecuteResult;
    public Text loadingTip;
    public GameObject anchorPrefab;
    public GameObject multiPlayer;
    public GameObject loading;
    private float durationTimes = 5.0f;
    private float anchorPoseUpdateTime = 1.0f;
    private bool IsCreateAnchorMode = false;
    private Dictionary<ulong, SharedAnchor> anchorList = new Dictionary<ulong, SharedAnchor>();
    public Dictionary<ulong, Guid> persistDict = new Dictionary<ulong, Guid>();
    [SerializeField] private Transform anchorRoot;

    [SerializeField] private GameObject anchorPreview;
    [SerializeField] private GameObject menuPanel;

    [SerializeField] private Button btnCreateAnchor;
    [SerializeField] private Button btnLoadAllAnchors;
    [SerializeField] private Button btnStartMatch;
    [SerializeField] private Button btnLeaveRoom;
    [SerializeField] private Button btnLoadAnchorByUuid;
    [SerializeField] private InputField inputUuid;

    private string accessToken = "";
    private string userId = "";
    private string displayName = "";
    private string appId = "";
    private string _poolKeyName = "pool1";
    private uint _maxUsers = 2;
    public const string TAG = "PXRSharedAnchorSample";

    private bool isShowUI = true;


    public enum CurState
    {
        None,
        Inited,
        EnqueueSend,
        EnqueueResultRecved,
        MatchmakingFound,
        RoomJoinSend,
        RoomJoined,
        RoomUpdatingSend,
        RoomUpdatingRecved,
        RoomGetCurrentSend,
        RoomLeaveSend,
        RoomLeaveRecved,
        SimplestTestEnd,
    }

    private const int logMaxCount = 7;
    private CurState _curState = CurState.None;
    private Queue<string> _logMessages = new Queue<string>();
    private int _curLogIndex = 0;
    Room _matchRoom;

    void Awake()
    {
        btnCreateAnchor.onClick.AddListener(OnBtnPressedCreateAnchor);
        btnLoadAllAnchors.onClick.AddListener(OnBtnPressedLoadLocalAnchors);
        btnStartMatch.onClick.AddListener(OnBtnPressedStartMatch);
        btnLeaveRoom.onClick.AddListener(OnBtnPressedLeaveRoom);
        btnLoadAnchorByUuid.onClick.AddListener(OnBtnPressedLoadAnchorByUuid);

    }

    private async void Start()
    {
        //Turn on MR mode
        //SetExecuteResult("Turn on MR mode");
        PXR_Manager.EnableVideoSeeThrough = true;
        ControllerManager.Instance.BingingGripHotKey(false, (args) =>
        {
            menuPanel.SetActive(!isShowUI);
            isShowUI = !isShowUI;
        }, null, null);
        StartSpatialAnchorProvider();
#if !UNITY_EDITOR
        InitPlatformService();

        MatchmakingService.SetMatchFoundNotificationCallback(ProcessMatchmakingMatchFound);
        NetworkService.SetNotification_Game_ConnectionEventCallback(OnGameConnectionEvent);
#endif
    }

    private async void StartSpatialAnchorProvider()
    {
        var result = await PXR_MixedReality.StartSenseDataProvider(PxrSenseDataProviderType.SpatialAnchor);
        //SetExecuteResult("StartSenseDataProvider:" + result);
    }

    // Update is called once per frame
    void Update()
    {
        if (CoreService.Initialized)
        {
            // Loop to check the current state
            CheckState();
        }
    }


    void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            PXR_Manager.EnableVideoSeeThrough = true;
        }
    }

    void OnGameConnectionEvent(Message<GameConnectionEvent> msg)
    {
        var state = msg.Data;
        LogHelper.LogInfo(TAG, $"OnGameConnectionEvent: {state}");
        if (state == GameConnectionEvent.Connected)
        {
            LogHelper.LogInfo(TAG, "GameConnection: success  ");
        }
        else if (state == GameConnectionEvent.Closed)
        {
            Uninitialize();
            LogHelper.LogInfo(TAG, "GameConnection: fail  Please re-initialize  ");
        }
        else if (state == GameConnectionEvent.GameLogicError)
        {
            Uninitialize();
            LogHelper.LogInfo(TAG,
                "GameConnection: fail  After successful reconnection, the logic state is found to be wrong  Please re-initialize  ");
        }
        else if (state == GameConnectionEvent.Lost)
        {
            LogHelper.LogInfo(TAG, "GameConnection: Reconnecting, please wait  ");
        }
        else if (state == GameConnectionEvent.Resumed)
        {
            LogHelper.LogInfo(TAG, "GameConnection: successful reconnection  ");
        }
        else if (state == GameConnectionEvent.KickedByRelogin)
        {
            Uninitialize();
            LogHelper.LogInfo(TAG, "GameConnection: Repeat login! Please reinitialize  ");
        }
        else if (state == GameConnectionEvent.KickedByGameServer)
        {
            Uninitialize();
            LogHelper.LogInfo(TAG, "GameConnection: Server kicks people! Please reinitialize  ");
        }
        else
        {
            LogHelper.LogInfo(TAG, "GameConnection: unknown error  ");
        }
    }

    void OnDestroy()
    {
        Uninitialize();
    }

    void ProcessMatchmakingMatchFound(Message<Room> message)
    {
        if (!message.IsError)
        {
            _matchRoom = message.Data;
            SetExecuteResult("Match success -> Found room : " + _matchRoom.RoomId);
            _curState = CurState.MatchmakingFound;
        }
        else
        {
            var error = message.GetError();
            SetExecuteResult($"Match failed : {error.Message}");
        }
    }

    private void CheckState()
    {
        TestMatchmakingAndRoom();
    }

    void TestMatchmakingAndRoom()
    {
        switch (_curState)
        {
            case CurState.Inited:
                StartEnqueue();
                break;
            case CurState.MatchmakingFound:
                StartJoinRoom();
                break;
            case CurState.RoomJoinSend:
                break;
            case CurState.RoomJoined:
                StartRoomUpdating();
                break;
            case CurState.RoomUpdatingSend:
                break;
            case CurState.RoomUpdatingRecved:
                break;
            case CurState.RoomGetCurrentSend:
                break;
            case CurState.RoomLeaveSend:
                break;
            case CurState.RoomLeaveRecved:
                EndTest();
                break;
            default:
                break;
        }
    }

    void StartEnqueue()
    {
        //SetExecuteResult($"Start enqueue");
        MatchmakingOptions options = new MatchmakingOptions();
        options.SetCreateRoomMaxUsers(_maxUsers);
        var rst = MatchmakingService.Enqueue2(_poolKeyName, options).OnComplete(ProcessMatchmakingEnqueue);
        var result = rst.TaskId;
        //SetExecuteResult("Match queue result = " + result);
        if (0 != result)
        {
            //SetExecuteResult("Current state  EnqueueSend");
            _curState = CurState.EnqueueSend;
        }


    }

    void EndTest()
    {
        _curState = CurState.SimplestTestEnd;
        SetExecuteResult("Leave Room Success! \n");
    }

    // enter the room
    void StartJoinRoom()
    {
        SetExecuteResult("StartJoinRoom...");
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.SetTurnOffUpdates(false); // receive updates
        roomOptions.SetRoomId(_matchRoom.RoomId);
        SetExecuteResult($"Enter room : {_matchRoom.RoomId}");
        var rst = RoomService.Join2(_matchRoom.RoomId, roomOptions).OnComplete(ProcessRoomJoin2);
        var result = rst.TaskId;
        //SetExecuteResult("Enter room result = " + result);
        if (0 != result)
        {
            //SetExecuteResult("Current state  room joined");
            _curState = CurState.RoomJoinSend;
        }
    }

    void ProcessMatchmakingEnqueue(Message<MatchmakingEnqueueResult> message)
    {
        if (!message.IsError)
        {
            var result = message.Data;
            _curState = CurState.EnqueueResultRecved;
            //SetExecuteResult($"Join the matching queue pool name: {result.Pool}  set current state: EnqueueResultReceived");
            SetExecuteResult($"Join the matching queue pool success,wait for other player enter ");
        }
        else
        {
            var error = message.GetError();
            SetExecuteResult($"Join the matching queue error : {error.Message}");
        }
    }

    void ProcessRoomJoin2(Message<Room> message)
    {
        if (!message.IsError)
        {
            var room = message.Data;
            _curState = CurState.RoomJoined;
            //SetExecuteResult($"join room Room.ID: {room.RoomId}, set current state: RoomJoined");
            SetExecuteResult($"Join room {room.RoomId} Success ");
            CloudAnchorData anchorData = new CloudAnchorData();
            anchorData.messageType = RoomMessageType.JoinedRoom;
            anchorData.uuid = Guid.NewGuid();
            anchorData.displayName = displayName;
            var sender = ConvertStructToBytes(anchorData);
            NetworkService.SendPacketToCurrentRoom(sender);
        }
        else
        {
            var error = message.GetError();
            SetExecuteResult($"join room error: {error.Message}");
        }
    }

    void StartRoomUpdating()
    {
        // read packet
        var packet = NetworkService.ReadPacket();
        while (packet != null)
        {
            var sender = packet.SenderId;
            var bytes = new byte[packet.Size];
            var bytesSize = packet.GetBytes(bytes);
            CloudAnchorData anchorData = ConvertBytesToStruct(bytes);

            switch (anchorData.messageType)
            {
                case RoomMessageType.JoinedRoom:
                    SetExecuteResult($"Start Played With {anchorData.displayName}");
                    break;
                case RoomMessageType.UpdateAnchor:
                    SetExecuteResult($"Received Anchor Uuid: {anchorData.uuid} FromName: {anchorData.displayName}");
                    loading.SetActive(true);
                    loadingTip.text = "Downloading";
                    DownSpatialAnchor(anchorData);
                    break;
            }

            packet.Dispose();
            packet = NetworkService.ReadPacket();
        }
    }

    private async void DownSpatialAnchor(CloudAnchorData anchorData)
    {
        SetLoadingImg(true);
        var result = await PXR_MixedReality.DownloadSharedSpatialAnchorAsync(anchorData.uuid);
        SetLoadingImg(false);
        SetExecuteResult("DownloadSharedSpatialAnchorAsync:" + result.ToString());
        if (result == PxrResult.SUCCESS)
        {
            var uuids = new[] { anchorData.uuid };
            var result1 = await PXR_MixedReality.QuerySpatialAnchorAsync(uuids);
            SetExecuteResult("QuerySpatialAnchorAsync:" + result1.result.ToString());
            if (result1.result == PxrResult.SUCCESS)
            {
                foreach (var key in result1.anchorHandleList)
                {
                    if (!anchorList.ContainsKey(key))
                    {
                        //Debug.Log("PXR_MRSample SpatialAnchorLoaded handle:" + key);
                        GameObject anchorObject = Instantiate(anchorPrefab);
                        SharedAnchor anchor = anchorObject.GetComponent<SharedAnchor>();
                        anchor.SetAnchorHandle(key);
                        anchor.SetAnchorUuid(anchorData.uuid);
                        anchor.SetAnchorSource(anchorData.displayName);

                        PXR_MixedReality.LocateAnchor(key, out var position, out var orientation);
                        anchor.transform.position = position;
                        anchor.transform.rotation = orientation;
                        anchorList.Add(key, anchor);
                        anchorList[key].IsSavedLocally = true;
                    }
                }
            }
        }

    }

    void StartRoomLeave()
    {
        SetExecuteResult("leave the room...");
        SetExecuteResult("roomId = " + _matchRoom.RoomId);
        var rst = RoomService.Leave(_matchRoom.RoomId).OnComplete(ProcessRoomLeave);
        var result = rst.TaskId;
        //SetExecuteResult("leave the room result = " + result);
        if (0 != result)
        {
            _curState = CurState.RoomLeaveSend;
            //SetExecuteResult("set current state  RoomLeaveSend");
        }
    }

    void ProcessRoomLeave(Message<Room> message)
    {
        if (!message.IsError)
        {
            var room = message.Data;
            _curState = CurState.RoomLeaveRecved;
            //SetExecuteResult($"leaven room Room.ID: {room.RoomId}, set current state: RoomLeaveRecved");
            SetExecuteResult($"leaven room Room.ID: {room.RoomId} Success");
        }
        else
        {
            var error = message.GetError();
            SetExecuteResult($"leave room error: {error.Message}");
        }
    }

    private void InitPlatformService()
    {
        SetExecuteResult("Start InitPlatformService");
        CoreService.Initialize();
        if (!CoreService.Initialized)
        {
            SetExecuteResult("pico initialize failed");
            return;
        }

        UserService.GetAccessToken().OnComplete(delegate(Message<string> message)
        {
            if (message.IsError)
            {
                var err = message.GetError();
                SetExecuteResult($"Got access token error {err.Message} code={err.Code}");
                return;
            }

            accessToken = message.Data;
            //SetExecuteResult($"Got access token: {accessToken}, GameInitialize begin");
            SetExecuteResult($"GameInitialize begin");
            CoreService.GameInitialize(accessToken).OnComplete(OnGameInitialize);
        });

    }

    void OnGameInitialize(Message<GameInitializeResult> msg)
    {
        if (msg == null)
        {
            SetExecuteResult($"OnGameInitialize: fail, message is null");
            return;
        }

        if (msg.IsError)
        {
            SetExecuteResult($"GameInitialize Failed: {msg.Error.Code}, {msg.Error.Message}");
        }
        else
        {
            //SetExecuteResult($"OnGameInitialize: {msg.Data}");
            if (msg.Data == GameInitializeResult.Success)
            {
                SetExecuteResult("GameInitialize: success ");
            }
            else
            {
                Uninitialize();
                SetExecuteResult("GameInitialize: fail Please re-initialize ");
            }
        }

        UserService.GetLoggedInUser().OnComplete(delegate(Message<User> m)
        {
            if (m.IsError)
            {
                SetExecuteResult($"GetLoggedInUser failed:code={m.Error.Code} message={m.Error.Message}");
                return;
            }

            userId = m.Data.ID;
            displayName = m.Data.DisplayName;
            //SetExecuteResult($"Got user id: {userId}  displayName:{displayName} , GameInitialize end");
            SetExecuteResult($"GameInitialize end Welcome {displayName}");
        });

    }

    void Uninitialize()
    {
        CoreService.GameUninitialize();
    }

    public void SetExecuteResult(string result)
    {
        LogHelper.LogInfo(TAG, result);
        _logMessages.Enqueue(result);

        if (_logMessages.Count > logMaxCount)
        {
            _logMessages.Dequeue();
        }

        var outstring = "";
        foreach (var log in _logMessages)
        {
            outstring += log + "\n";
        }

        ExecuteResult.text = outstring;
    }


    private void OnBtnPressedCreateAnchor()
    {
        menuPanel.SetActive(false);
        isShowUI = false;
        IsCreateAnchorMode = !IsCreateAnchorMode;
        if (IsCreateAnchorMode)
        {
            btnCreateAnchor.transform.Find("Text").GetComponent<Text>().text = "CancelCreate";
            anchorPreview.SetActive(true);
            ControllerManager.Instance.BingingPrimaryHotKey(false, (args) =>
            {
                //SetExecuteResult($"ClickButtonToCreateAnchor: Position: ({anchorPreview.transform.position.x:F3}, {anchorPreview.transform.position.y:F3}, {anchorPreview.transform.position.z:F3})");
                CreateSpatialAnchor(anchorPreview.transform);
            }, null, null);
        }
        else
        {
            btnCreateAnchor.transform.Find("Text").GetComponent<Text>().text = "CreateAnchor";
            anchorPreview.SetActive(false);
            ControllerManager.Instance.UnBingingPrimaryInputActionRight();
        }
    }

    private void OnBtnPressedStartMatch()
    {
        SetExecuteResult($"Start Matching Room");
        _curState = CurState.Inited;
    }

    private void OnBtnPressedLeaveRoom()
    {
        StartRoomLeave();
    }

    private async void OnBtnPressedLoadLocalAnchors()
    {
        var result = await PXR_MixedReality.QuerySpatialAnchorAsync();
        SetExecuteResult("LoadSpatialAnchorAsync:" + result.result.ToString());
        if (result.result == PxrResult.SUCCESS)
        {
            foreach (var key in result.anchorHandleList)
            {
                if (!anchorList.ContainsKey(key))
                {
                    GameObject anchorObject = Instantiate(anchorPrefab);
                    SharedAnchor anchor = anchorObject.GetComponent<SharedAnchor>();
                    anchor.SetAnchorHandle(key);
                    PXR_MixedReality.GetAnchorUuid(key, out var uuid);
                    anchor.SetAnchorUuid(uuid);
                    anchor.SetAnchorSource(displayName);

                    PXR_MixedReality.LocateAnchor(key, out var position, out var orientation);
                    anchor.transform.position = position;
                    anchor.transform.rotation = orientation;
                    anchorList.Add(key, anchor);
                    anchorList[key].IsSavedLocally = true;
                }
            }
        }
    }

    private async void OnBtnPressedLoadAnchorByUuid()
    {
        if (inputUuid.text != String.Empty)
        {
            loading.SetActive(true);
            if (Guid.TryParse(inputUuid.text, out var guid))
            {
                SetExecuteResult($"Valid anchor uuid: {guid}");
                var result = await PXR_MixedReality.DownloadSharedSpatialAnchorAsync(guid);
                if (result == PxrResult.SUCCESS)
                {
                    var uuids = new[] { guid };
                    var result1 = await PXR_MixedReality.QuerySpatialAnchorAsync(uuids);
                    SetExecuteResult("LoadSpatialAnchorAsync: " + result1.result.ToString());
                    if (result1.result == PxrResult.SUCCESS)
                    {
                        foreach (var key in result1.anchorHandleList)
                        {
                            if (!anchorList.ContainsKey(key))
                            {
                                Debug.Log("PXR_MRSample SpatialAnchorLoaded handle:" + key);
                                GameObject anchorObject = Instantiate(anchorPrefab);
                                SharedAnchor anchor = anchorObject.GetComponent<SharedAnchor>();
                                anchor.SetAnchorHandle(key);
                                anchor.SetAnchorUuid(guid);
                                anchor.SetAnchorSource("From Cloud");

                                PXR_MixedReality.LocateAnchor(key, out var position, out var orientation);
                                anchor.transform.position = position;
                                anchor.transform.rotation = orientation;
                                anchorList.Add(key, anchor);
                                anchorList[key].IsSavedLocally = true;
                            }
                        }
                    }
                }

            }
            else
            {
                SetExecuteResult("Invalid anchor uuid");
            }
        }
    }

    private void SpatialTrackingStateUpdate(PxrEventSpatialTrackingStateUpdate info)
    {
        Debug.Log("PXR_MRSample TrackingState Event:" + info.state + info.message);
    }

    private async void CreateSpatialAnchor(Transform transform)
    {
        var result = await PXR_MixedReality.CreateSpatialAnchorAsync(transform.position, transform.rotation);
        //SetExecuteResult($"CreateSpatialAnchorAsync: {result.ToString()} Position: ({transform.position.x:F3}, {transform.position.y:F3}, {transform.position.z:F3})");
        if (result.result == PxrResult.SUCCESS)
        {
            GameObject anchorObject = Instantiate(anchorPrefab);
            var anchor = anchorObject.GetComponent<SharedAnchor>();
            if (anchor == null)
            {
                anchor = anchorObject.AddComponent<SharedAnchor>();
            }

            SetExecuteResult($"CreateSpatialAnchorAsync anchorHandle: {result.anchorHandle} uuid:{result.uuid}");
            anchor.SetAnchorHandle(result.anchorHandle);
            anchor.SetAnchorUuid(result.uuid);
            anchor.SetAnchorSource(displayName);
            anchor.transform.position = transform.position;
            anchor.transform.rotation = transform.rotation;
            anchorList.Add(result.anchorHandle, anchor);
        }
    }

    public void DestroySpatialAnchor(ulong anchorHandle)
    {
        var result = PXR_MixedReality.DestroyAnchor(anchorHandle);
        SetExecuteResult("DestroySpatialAnchor:" + result.ToString());
        if (result == PxrResult.SUCCESS)
        {
            if (anchorList.ContainsKey(anchorHandle))
            {
                Destroy(anchorList[anchorHandle].gameObject);
                anchorList.Remove(anchorHandle);
            }
        }
    }

    public void SetLoadingImg(bool state)
    {
        loading.SetActive(state);
        loadingTip.text = "Uploading";
    }

    public void ShareAnchorToOthers(Guid uuid)
    {
        loading.SetActive(false);
        CloudAnchorData anchorData = new CloudAnchorData();
        anchorData.messageType = RoomMessageType.UpdateAnchor;
        anchorData.uuid = uuid;
        anchorData.displayName = displayName;
        NetworkService.SendPacketToCurrentRoom(ConvertStructToBytes(anchorData));
        //NetworkService.SendPacketToCurrentRoom(uuid.ToByteArray());
    }

    private byte[] ConvertStructToBytes(CloudAnchorData anchorData)
    {

        MemoryStream memStream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(memStream);
        writer.Write((int)anchorData.messageType);
        writer.Write(anchorData.uuid.ToByteArray());
        writer.Write(anchorData.displayName);
        BinaryReader reader = new BinaryReader(memStream);
        memStream.Position = 0;
        int arraySize = (int)(memStream.Length - memStream.Position);
        byte[] bytes = new byte[arraySize];
        reader.Read(bytes, 0, arraySize);
        return bytes;
    }

    private CloudAnchorData ConvertBytesToStruct(byte[] bytes)
    {
        CloudAnchorData myStruct = new CloudAnchorData();
        MemoryStream memStream = new MemoryStream(bytes);
        BinaryReader reader = new BinaryReader(memStream);
        myStruct.messageType = (RoomMessageType)reader.ReadInt32();
        myStruct.uuid = new Guid(reader.ReadBytes(16));
        myStruct.displayName = reader.ReadString();
        return myStruct;

    }
}

public enum RoomMessageType
{
    None,
    UpdateAnchor,
    JoinedRoom,
}

struct CloudAnchorData
{
    public RoomMessageType messageType;
    public string displayName;
    public Guid uuid;
}
