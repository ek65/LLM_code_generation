using Photon.Pun;
using UnityEngine;

namespace Network
{
    public class NoOwnershipObject : MonoBehaviourPun, IPunObservable
    {
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.W))
            {
                _rigidbody.MovePosition(transform.position + transform.forward * Time.deltaTime);
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {

            if (stream.IsWriting) 
            {
                // Broadcast position & rotation to others
                stream.SendNext(transform.position);
                stream.SendNext(transform.rotation);
            }
            else 
            {
                // Receive position & rotation from whichever client sent it
                transform.position = (Vector3)stream.ReceiveNext();
                transform.rotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}
