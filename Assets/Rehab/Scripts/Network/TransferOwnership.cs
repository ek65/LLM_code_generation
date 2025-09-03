using Photon.Pun;
using UnityEngine;

namespace Network
{
    public class TransferOwnership : MonoBehaviourPun
    {

        private Rigidbody _rigidbody;
        private bool _isThisNetworkInstatiated;
        public bool test;
        

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _isThisNetworkInstatiated = photonView.InstantiationId != 0;
        }

        public void OnGrabRequestOwnership()
        {
            if (!photonView.AmOwner && _isThisNetworkInstatiated)
            {
                photonView.RequestOwnership();
                Debug.Log("RequestOwnership");
            }
        }

        private void Update()
        {
            if (!_isThisNetworkInstatiated) return;
            if (test)
            {
                OnGrabRequestOwnership();
                test = !test;
            }
            if (!photonView.AmOwner)
            {
                _rigidbody.useGravity = false;
                _rigidbody.isKinematic = true;
            }
            else
            {
                _rigidbody.useGravity = true;
                _rigidbody.isKinematic = false;
            }
        }
    }
}
