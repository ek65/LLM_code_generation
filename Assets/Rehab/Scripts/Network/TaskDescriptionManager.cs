using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Network
{
    public class TaskDescriptionManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _taskText;
        private PhotonView _photonView;

        private Queue<string> _textQueue = new Queue<string>();
        private const int _maxEntries = 10;

        void Start()
        {
            _photonView = gameObject.GetComponent<PhotonView>();
        }

        public void OnSubmit(string messageFromTherapist)
        {
            string newText = messageFromTherapist;

            if (newText == null)
            {
                Debug.Log("Message from therapist is null");
                return;
            }

            if (newText == "")
            {
                Debug.Log("Empty string from therapist...");
                return;
            }

            if (!string.IsNullOrEmpty(newText))
            {
                if (_textQueue.Count >= _maxEntries)
                {
                    _textQueue.Dequeue();
                }

                _textQueue.Enqueue(newText);
                string finalText = string.Join("\n", _textQueue);
                _photonView.RPC(nameof(RPC_UpdateText), RpcTarget.All, finalText);
            }
        }

        [PunRPC]
        private void RPC_UpdateText(string task)
        {
            _taskText.text = task;
        }
    }
}