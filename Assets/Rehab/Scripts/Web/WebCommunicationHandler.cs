using Network;
using UnityEngine;

public class WebCommunicationHandler : MonoBehaviour
{
    [SerializeField] private TaskDescriptionManager _taskDescriptionManager;
    public void RecieveMessageFromTherapist(string message)
    {
        Debug.Log("Message received from therapist: " + message);
        _taskDescriptionManager.OnSubmit(message);
    }
}