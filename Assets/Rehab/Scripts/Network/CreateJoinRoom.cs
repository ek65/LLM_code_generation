using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

namespace Network
{
    /// <summary>
    /// Handles room creation and joining for rehabilitation sessions using Photon networking
    /// </summary>
    public class CreateRoom : MonoBehaviourPunCallbacks
    {
        private LobbyManager _lobbyManager;
        public TextMeshProUGUI text;

        private void Start()
        {
            _lobbyManager = transform.parent.GetComponent<LobbyManager>();
            if (_lobbyManager == null)
            {
                Debug.LogError("LobbyManager is null");
                return;
            }

            // Initialize UI and room connection based on user role
            if (_lobbyManager.isClient) // Clinician
            {
                text.text = "Join Rehab Instance";
                Invoke(nameof(AttemptJoinRehabInstance), 2f);
            }
            else // Meta Quest
            {
                text.text = "Create Rehab Instance";
                Invoke(nameof(CreateRehabInstance), 2f);
            }
        }

        private void AttemptJoinRehabInstance()
        {
            Debug.Log("Attempting Join Rehab Instance");
            PhotonNetwork.JoinRoom("RehabInstance1");
        }

        private void CreateRehabInstance()
        {
            // Create room with support for up to 6 players
            PhotonNetwork.CreateRoom("RehabInstance1",
                new RoomOptions() { MaxPlayers = 6 },
                TypedLobby.Default);
            Debug.Log("Create Rehab Instance");
        }

        public override void OnJoinRoomFailed(short returnCode, string message)
        {
            Debug.LogWarning($"Join room failed: {message}. Retrying in 2 seconds...");
            if (returnCode == ErrorCode.GameDoesNotExist)
            {
                Debug.Log("Room does not exist.");
            }
            else
            {
                Invoke(nameof(AttemptJoinRehabInstance), 2f); // Retry join attempt
            }
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("Successfully joined the room.");
            PhotonNetwork.LoadLevel("RehabInstance");
        }

        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogError($"Room creation failed: {message}");
        }
    }
}
