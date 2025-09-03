using Photon.Pun;
using UnityEngine;

namespace Network
{
    public class NetworkAvatar : MonoBehaviourPun, IPunObservable
    {
        public Transform[] childJoints; // Array to store references to the child joints
        public Vector3[] networkedPositions;
        public Quaternion[] networkedRotations;

        private void Start()
        {
            // Find all child joints
            childJoints = GetComponentsInChildren<Transform>();
            networkedPositions = new Vector3[childJoints.Length];
            networkedRotations = new Quaternion[childJoints.Length];
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // Send all joint data to others
                for (int i = 0; i < childJoints.Length; i++)
                {
                    stream.SendNext(childJoints[i].localPosition);
                    stream.SendNext(childJoints[i].localRotation);
                }
            }
            else
            {
                // Receive all joint data from others
                for (int i = 0; i < childJoints.Length; i++)
                {
                    networkedPositions[i] = (Vector3)stream.ReceiveNext();
                    networkedRotations[i] = (Quaternion)stream.ReceiveNext();
                }
            }
        }

        private void Update()
        {
            if (!photonView.IsMine)
            {
                // Smoothly interpolate received positions and rotations
                for (int i = 0; i < childJoints.Length; i++)
                {
                    childJoints[i].localPosition = Vector3.Lerp(
                        childJoints[i].localPosition,
                        networkedPositions[i],
                        Time.deltaTime * 10
                    );
                    childJoints[i].localRotation = Quaternion.Lerp(
                        childJoints[i].localRotation,
                        networkedRotations[i],
                        Time.deltaTime * 10
                    );
                }
            }
        }
    }
}
