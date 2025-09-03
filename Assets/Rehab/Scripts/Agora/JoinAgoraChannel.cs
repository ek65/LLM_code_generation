using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using Agora.Rtc;
using io.agora.rtc.demo;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.UI;
using TMPro;
using System.Collections;


public class JoinAgoraChannel : MonoBehaviour
{
    [FormerlySerializedAs("appIdInput")]
    [SerializeField]
    private AppIdInput _appIdInput;

    [Header("_____________Basic Configuration_____________")]
    [FormerlySerializedAs("APP_ID")]
    [SerializeField]
    private string _appID = "";

    [FormerlySerializedAs("TOKEN")]
    [SerializeField]
    private string _token = "";

    [FormerlySerializedAs("CHANNEL_NAME")]
    [SerializeField]
    private string _channelName = "";

    [SerializeField]
    public Transform target;
    [SerializeField]
    public GameObject videoSurfacePrefab;
    [SerializeField]
    public TMP_Text loadingText;

    internal IRtcEngine RtcEngine = null;

    private GameObject RemoteVideo;

    private IVideoFrameObserver videoFrameObserver;

    public WebCamTexture _webCamTexture;
    private bool _isStreaming = false;

    private void Start()
    {
        LoadAssetData();
        if (_appID != null)
        {
            RtcEngine = Agora.Rtc.RtcEngine.CreateAgoraRtcEngine();
        }
        InitEngine();
        JoinChannel();
    }


    // Load information to connect to the proper Agora channel
    [ContextMenu("ShowAgoraBasicProfileData")]
    private void LoadAssetData()
    {
        if (_appIdInput == null) return;
        _appID = _appIdInput.appID;
        _token = _appIdInput.token;
        _channelName = _appIdInput.channelName;
    }

    // Init RTC Agora channel
    public void InitEngine()
    {
        AREA_CODE areaCode = (AREA_CODE)Enum.Parse(typeof(AREA_CODE), "AREA_CODE_GLOB");

        UserEventHandler handler = new UserEventHandler(this);
        RtcEngineContext context = new RtcEngineContext();
        context.appId = _appID;
        context.channelProfile = CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_LIVE_BROADCASTING;
        context.audioScenario = AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_DEFAULT;
        context.areaCode = areaCode;
        var result = RtcEngine.Initialize(context);
        Debug.Log("Initialize result : " + result);

        RtcEngine.InitEventHandler(handler);

        RtcEngine.EnableAudio();
        RtcEngine.EnableVideo();

        VideoEncoderConfiguration config = new VideoEncoderConfiguration();
        config.dimensions = new VideoDimensions(640, 360);
        config.frameRate = 15;
        config.bitrate = 0;
        RtcEngine.SetVideoEncoderConfiguration(config);
        RtcEngine.SetChannelProfile(CHANNEL_PROFILE_TYPE.CHANNEL_PROFILE_COMMUNICATION);
        RtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);

        RtcEngine.SetExternalVideoSource(true, false, EXTERNAL_VIDEO_SOURCE_TYPE.VIDEO_FRAME, null);
    }

    public void JoinChannel()
    {
        RtcEngine.JoinChannel(_token, _channelName, "", 0);
    }


    // This function is called by WebCamTextureManager.cs and begins the process of sending headset camera to Agora channel
     public void SetWebCamTexture(WebCamTexture webCamTexture)
    {
        _webCamTexture = webCamTexture;
        StartCoroutine(SendWebCamFramesToAgora());
    }

    private IEnumerator SendWebCamFramesToAgora()
    {
        _isStreaming = true;
        while (_isStreaming)
        {
            if (_webCamTexture.didUpdateThisFrame)
            {
                SendFrameToAgora(_webCamTexture);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private void SendFrameToAgora(WebCamTexture webcamTexture)
    {
        if (RtcEngine == null || webcamTexture == null) return;

        int width = webcamTexture.width;
        int height = webcamTexture.height;
        Color32[] pixels = webcamTexture.GetPixels32();
        Color32[] flippedPixels = new Color32[pixels.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcIndex = y * width + x;
                int flippedIndex = y * width + (width - 1 - x);
                flippedPixels[flippedIndex] = pixels[srcIndex];
            }
        }
        
        byte[] byteBuffer = new byte[flippedPixels.Length * 4]; 
        for (int i = 0; i < flippedPixels.Length; i++)
        {
            byteBuffer[i * 4] = flippedPixels[i].r;
            byteBuffer[i * 4 + 1] = flippedPixels[i].g;
            byteBuffer[i * 4 + 2] = flippedPixels[i].b;
            byteBuffer[i * 4 + 3] = flippedPixels[i].a;
        }

        ExternalVideoFrame frame = new ExternalVideoFrame
        {
            type = VIDEO_BUFFER_TYPE.VIDEO_BUFFER_RAW_DATA,
            format = VIDEO_PIXEL_FORMAT.VIDEO_PIXEL_RGBA,
            buffer = byteBuffer,
            stride = width,
            height = height,
            rotation = 180,
            timestamp = (long)(Time.time * 1000)
        };

        int result = RtcEngine.PushVideoFrame(frame);
        // Debug.Log("PushVideoFrame result: " + result);
    }

    // Called when a remote user joins the Agora channel. 
    private void SpawnRemoteVideoPanel(uint uid, string channelId)
    {
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }

        var obj = Instantiate(videoSurfacePrefab, this.transform);
        RemoteVideo = obj.gameObject;
        Debug.Log(RemoteVideo);
        VideoSurface videoSurface = obj.GetComponent<VideoSurface>();
        videoSurface.SetForUser(uid, channelId, VIDEO_SOURCE_TYPE.VIDEO_SOURCE_REMOTE);
    }

    internal string GetChannelName()
    {
        return _channelName;
    }

    public void LeaveChannel()
    {
        if (RtcEngine == null) return;
        if (RemoteVideo != null)
        {
            Destroy(RemoteVideo);
        }
        //RtcEngine.InitEventHandler(null);
        RtcEngine.LeaveChannel();
        RtcEngine.Dispose();
    }

    
    public void OnApplicationQuit()
    {
        LeaveChannel();
    }
    

    #region Agora Events


    internal class UserEventHandler : IRtcEngineEventHandler
    {
        private readonly JoinAgoraChannel _videoSample;

        internal UserEventHandler(JoinAgoraChannel videoSample)
        {
            _videoSample = videoSample;
        }

        public override void OnUserJoined(RtcConnection connection, uint uid, int elapsed)
        {
            _videoSample.SpawnRemoteVideoPanel(uid, _videoSample.GetChannelName());
        }

        public override void OnUserOffline(RtcConnection connection, uint uid, USER_OFFLINE_REASON_TYPE reason)
        {
            Debug.Log("user left");
            if (_videoSample.RemoteVideo != null)
            {
                Destroy(_videoSample.RemoteVideo);
                _videoSample.RemoteVideo = null;
            }
        }

    }

    #endregion
}


