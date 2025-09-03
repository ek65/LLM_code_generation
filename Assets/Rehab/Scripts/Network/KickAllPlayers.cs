using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine;

public class KickOnQuit : MonoBehaviourPunCallbacks, IOnEventCallback
{
    private const byte KickAllEventCode = 1; // Event code for kicking all players

    void Start()
    {
        PhotonNetwork.AddCallbackTarget(this); // Register event listener
    }

    void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this); // Unregister when destroyed
    }

    // Called when the application is closing or the player quits
    void OnApplicationQuit()
    {
        if (PhotonNetwork.IsMasterClient) // Only the Master Client should kick everyone
        {
            KickEveryone();
        }
    }

    private void KickEveryone()
    {
        RaiseEventOptions options = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // Send to all players
        SendOptions sendOptions = new SendOptions { Reliability = true }; // Ensure event is delivered

        PhotonNetwork.RaiseEvent(KickAllEventCode, null, options, sendOptions);
        Debug.Log("Kick event sent to all players.");

        // Ensure Photon disconnects cleanly
        PhotonNetwork.Disconnect();
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == KickAllEventCode)
        {
            Debug.Log("Received kick event. Leaving room...");
            PhotonNetwork.LeaveRoom(); // Leave the room when receiving the event
        }
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left the room due to kick event.");
        // You can load a different scene or show a message here if needed
    }
}