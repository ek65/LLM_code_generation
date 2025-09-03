using UnityEngine;
using UnityEditor;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PhotonQuickConnect : EditorWindow, IConnectionCallbacks, IMatchmakingCallbacks
{
    [SerializeField] private string roomName = "TestRoom";
    [SerializeField] private string gameVersion = "1.0";
    [SerializeField] private int maxPlayers = 4;
    [SerializeField] private string playerName = "Developer";
    [SerializeField] private bool autoSyncScene = true;

    private Vector2 scrollPosition;
    private bool isConnecting = false;
    private string connectionStatus = "Disconnected";
    private List<string> recentRooms = new List<string>();

    [MenuItem("Tools/Photon/Quick Connect")]
    public static void ShowWindow()
    {
        PhotonQuickConnect window = GetWindow<PhotonQuickConnect>("Photon Quick Connect");
        window.minSize = new Vector2(400, 300);
        window.Show();
    }

    void OnEnable()
    {
        // Load preferences
        roomName = EditorPrefs.GetString("PhotonQuickConnect_RoomName", "TestRoom");
        gameVersion = EditorPrefs.GetString("PhotonQuickConnect_GameVersion", "1.0");
        maxPlayers = EditorPrefs.GetInt("PhotonQuickConnect_MaxPlayers", 4);
        playerName = EditorPrefs.GetString("PhotonQuickConnect_PlayerName", "Developer");
        autoSyncScene = EditorPrefs.GetBool("PhotonQuickConnect_AutoSync", true);

        // Load recent rooms
        LoadRecentRooms();

        // Register callbacks
        if (PhotonNetwork.NetworkClientState != ClientState.Disconnected)
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        // Listen for play mode changes
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        UpdateConnectionStatus();
    }

    void OnDisable()
    {
        // Save preferences
        EditorPrefs.SetString("PhotonQuickConnect_RoomName", roomName);
        EditorPrefs.SetString("PhotonQuickConnect_GameVersion", gameVersion);
        EditorPrefs.SetInt("PhotonQuickConnect_MaxPlayers", maxPlayers);
        EditorPrefs.SetString("PhotonQuickConnect_PlayerName", playerName);
        EditorPrefs.SetBool("PhotonQuickConnect_AutoSync", autoSyncScene);

        SaveRecentRooms();

        // Remove callbacks
        if (PhotonNetwork.NetworkClientState != ClientState.Disconnected)
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        // Remove play mode listener
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    void OnGUI()
    {
        EditorGUILayout.Space(10);

        // Header
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 16;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("Photon Quick Connect", headerStyle);

        EditorGUILayout.Space(10);

        // Connection Status
        DrawConnectionStatus();

        EditorGUILayout.Space(10);

        // Settings
        DrawSettings();

        EditorGUILayout.Space(10);

        // Connection Controls
        DrawConnectionControls();

        EditorGUILayout.Space(10);

        // Recent Rooms
        DrawRecentRooms();

        EditorGUILayout.Space(10);

        // Current Room Info
        DrawCurrentRoomInfo();
    }

    void DrawConnectionStatus()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Connection Status", EditorStyles.boldLabel);

        GUIStyle statusStyle = new GUIStyle(EditorStyles.label);
        if (!Application.isPlaying)
        {
            statusStyle.normal.textColor = Color.gray;
            connectionStatus = "Play Mode Required";
        }
        else if (PhotonNetwork.IsConnected)
        {
            statusStyle.normal.textColor = Color.green;
            connectionStatus = $"Connected to {PhotonNetwork.CloudRegion}";
        }
        else if (isConnecting)
        {
            statusStyle.normal.textColor = Color.yellow;
            connectionStatus = "Connecting...";
        }
        else
        {
            statusStyle.normal.textColor = Color.red;
            connectionStatus = "Disconnected";
        }

        EditorGUILayout.LabelField(connectionStatus, statusStyle);

        if (PhotonNetwork.InRoom)
        {
            EditorGUILayout.LabelField($"Room: {PhotonNetwork.CurrentRoom.Name}");
            EditorGUILayout.LabelField($"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers}");
        }

        EditorGUILayout.EndVertical();
    }

    void DrawSettings()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        roomName = EditorGUILayout.TextField("Room Name", roomName);
        gameVersion = EditorGUILayout.TextField("Game Version", gameVersion);
        maxPlayers = EditorGUILayout.IntSlider("Max Players", maxPlayers, 1, 20);
        playerName = EditorGUILayout.TextField("Player Name", playerName);
        autoSyncScene = EditorGUILayout.Toggle("Auto Sync Scene", autoSyncScene);

        EditorGUILayout.EndVertical();
    }

    void DrawConnectionControls()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = Application.isPlaying && !PhotonNetwork.IsConnected && !isConnecting;
        if (GUILayout.Button("Connect & Join Room", GUILayout.Height(30)))
        {
            ConnectAndJoinRoom();
        }

        GUI.enabled = Application.isPlaying && PhotonNetwork.IsConnected && !PhotonNetwork.InRoom;
        if (GUILayout.Button("Join Room Only", GUILayout.Height(30)))
        {
            JoinRoom();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = Application.isPlaying && PhotonNetwork.InRoom;
        if (GUILayout.Button("Leave Room"))
        {
            PhotonNetwork.LeaveRoom();
        }

        GUI.enabled = Application.isPlaying && PhotonNetwork.IsConnected;
        if (GUILayout.Button("Disconnect"))
        {
            PhotonNetwork.Disconnect();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }

    void DrawRecentRooms()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Recent Rooms", EditorStyles.boldLabel);

        if (recentRooms.Count == 0)
        {
            EditorGUILayout.LabelField("No recent rooms", EditorStyles.miniLabel);
        }
        else
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.MaxHeight(100));

            for (int i = 0; i < recentRooms.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(recentRooms[i]);

                if (GUILayout.Button("Use", GUILayout.Width(40)))
                {
                    roomName = recentRooms[i];
                }

                if (GUILayout.Button("�", GUILayout.Width(20)))
                {
                    recentRooms.RemoveAt(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            if (GUILayout.Button("Clear Recent Rooms"))
            {
                recentRooms.Clear();
            }
        }

        EditorGUILayout.EndVertical();
    }

    void DrawCurrentRoomInfo()
    {
        if (!PhotonNetwork.InRoom) return;

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Current Room Players", EditorStyles.boldLabel);

        foreach (var player in PhotonNetwork.PlayerList)
        {
            string playerInfo = $"{player.NickName ?? player.UserId}";
            if (player.IsMasterClient) playerInfo += " (Master)";
            if (player.IsLocal) playerInfo += " (You)";

            EditorGUILayout.LabelField($"� {playerInfo}");
        }

        EditorGUILayout.EndVertical();
    }

    void ConnectAndJoinRoom()
    {
        if (string.IsNullOrEmpty(roomName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a room name", "OK");
            return;
        }

        isConnecting = true;

        // Setup Photon
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.AutomaticallySyncScene = autoSyncScene;
        PhotonNetwork.NickName = playerName;

        // Add callbacks
        PhotonNetwork.AddCallbackTarget(this);

        // Connect
        PhotonNetwork.ConnectUsingSettings();

        Debug.Log($"[PhotonQuickConnect] Connecting to Photon and joining room: {roomName}");
    }

    void JoinRoom()
    {
        if (string.IsNullOrEmpty(roomName))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a room name", "OK");
            return;
        }

        RoomOptions roomOptions = new RoomOptions()
        {
            MaxPlayers = maxPlayers,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);

        Debug.Log($"[PhotonQuickConnect] Joining room: {roomName}");
    }

    void AddToRecentRooms(string room)
    {
        if (recentRooms.Contains(room))
        {
            recentRooms.Remove(room);
        }

        recentRooms.Insert(0, room);

        // Keep only last 10 rooms
        if (recentRooms.Count > 10)
        {
            recentRooms.RemoveAt(recentRooms.Count - 1);
        }

        SaveRecentRooms();
    }

    void LoadRecentRooms()
    {
        recentRooms.Clear();
        for (int i = 0; i < 10; i++)
        {
            string room = EditorPrefs.GetString($"PhotonQuickConnect_RecentRoom_{i}", "");
            if (!string.IsNullOrEmpty(room))
            {
                recentRooms.Add(room);
            }
        }
    }

    void SaveRecentRooms()
    {
        for (int i = 0; i < 10; i++)
        {
            string room = i < recentRooms.Count ? recentRooms[i] : "";
            EditorPrefs.SetString($"PhotonQuickConnect_RecentRoom_{i}", room);
        }
    }

    void UpdateConnectionStatus()
    {
        // Update connection status
        if (Application.isPlaying)
        {
            if (PhotonNetwork.IsConnected)
            {
                connectionStatus = $"Connected to {PhotonNetwork.CloudRegion}";
            }
            else if (isConnecting)
            {
                connectionStatus = "Connecting...";
            }
            else
            {
                connectionStatus = "Disconnected";
            }
        }
        else
        {
            connectionStatus = "Play Mode Required";
            isConnecting = false;
        }

        Repaint();
    }

    void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        switch (state)
        {
            case PlayModeStateChange.EnteredEditMode:
                // When exiting play mode, reset connection state
                isConnecting = false;
                connectionStatus = "Play Mode Required";
                if (PhotonNetwork.NetworkClientState != ClientState.Disconnected)
                {
                    PhotonNetwork.RemoveCallbackTarget(this);
                }
                break;

            case PlayModeStateChange.EnteredPlayMode:
                // When entering play mode, check if we need to add callbacks
                if (PhotonNetwork.NetworkClientState != ClientState.Disconnected)
                {
                    PhotonNetwork.AddCallbackTarget(this);
                }
                break;
        }

        UpdateConnectionStatus();
    }

    // Photon Callbacks
    public void OnConnectedToMaster()
    {
        Debug.Log("[PhotonQuickConnect] Connected to Photon Master Server");
        isConnecting = false;
        JoinRoom();
        UpdateConnectionStatus();
    }

    public void OnJoinedRoom()
    {
        Debug.Log($"[PhotonQuickConnect] Joined room: {PhotonNetwork.CurrentRoom.Name}");
        AddToRecentRooms(PhotonNetwork.CurrentRoom.Name);
        UpdateConnectionStatus();
    }

    public void OnLeftRoom()
    {
        Debug.Log("[PhotonQuickConnect] Left room");
        UpdateConnectionStatus();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"[PhotonQuickConnect] Disconnected: {cause}");
        isConnecting = false;
        PhotonNetwork.RemoveCallbackTarget(this);
        UpdateConnectionStatus();
    }

    public void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[PhotonQuickConnect] Failed to join room: {message}");
        UpdateConnectionStatus();
    }

    public void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"[PhotonQuickConnect] Failed to create room: {message}");
        UpdateConnectionStatus();
    }

    public void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[PhotonQuickConnect] Player joined: {newPlayer.NickName ?? newPlayer.UserId}");
        UpdateConnectionStatus();
    }

    public void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[PhotonQuickConnect] Player left: {otherPlayer.NickName ?? otherPlayer.UserId}");
        UpdateConnectionStatus();
    }

    // Required empty implementations
    public void OnConnected() { }
    public void OnRegionListReceived(RegionHandler regionHandler) { }
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }
    public void OnCreatedRoom() { }
    public void OnJoinedLobby() { }
    public void OnLeftLobby() { }
    public void OnRoomListUpdate(List<RoomInfo> roomList) { }
    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) { }
    public void OnFriendListUpdate(List<FriendInfo> friendList) { }
    public void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps) { }
    public void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged) { }
    public void OnMasterClientSwitched(Player newMasterClient) { }
    public void OnJoinRandomFailed(short returnCode, string message) { }
}