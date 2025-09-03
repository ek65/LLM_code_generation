using Photon.Pun;
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    private GameObject cubePatient;
    private MotionMap mm;
    public PhotonView photonView;

    void Start()
    {
        cubePatient  = GameObject.Find("Cube Patient");
        mm = cubePatient.GetComponent<MotionMap>();
        photonView = cubePatient.GetComponent<PhotonView>();
    }

    public void Done()
    {
        photonView.RPC("SetKinematics", RpcTarget.All, false);
        photonView.RPC("Done", RpcTarget.All);
    }

    public void Show()
    {
        photonView.RPC("Show", RpcTarget.All);
    }

    public void Hide()
    {
        photonView.RPC("Hide", RpcTarget.All);
    }

    public void StopMap()
    {
        photonView.RPC("StopMap", RpcTarget.All);
    }

    public void Wipe()
    {
        photonView.RPC("Wipe", RpcTarget.All);
    }

    public void Dest()
    {        
        photonView.RPC("DestroyVoxels", RpcTarget.All);
    }

    public void Swap()
    {
        photonView.RPC("ToggleVoxelVis", RpcTarget.All);

    }
}
