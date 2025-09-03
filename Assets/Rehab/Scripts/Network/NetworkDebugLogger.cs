using Photon.Pun;
using UnityEngine;

public class NetworkDebugLogger : MonoBehaviourPun
{
    public static NetworkDebugLogger Instance;

    private void Awake()
    {
        Instance = this;
    }

    [PunRPC]
    void LogMessage(string message)
    {
        Debug.Log(message);
    }

    public void LogToAllClients(string message)
    {
        photonView.RPC("LogMessage", RpcTarget.All, message);
    }
}