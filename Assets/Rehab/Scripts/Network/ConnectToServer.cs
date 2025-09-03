using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Network
{
    public class ConnectToServer : MonoBehaviourPunCallbacks
    {
        private void Start()
        {
            Invoke(nameof(Connect), 1f);
        }

        private void Connect()
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "usw";
        }

        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();
            PhotonNetwork.JoinLobby();
        }
        
        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            SceneManager.LoadScene("Lobby");
            Debug.Log("onjoined lobby");
        }
    }
}
