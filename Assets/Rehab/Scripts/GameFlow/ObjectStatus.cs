using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Text.RegularExpressions;

[RequireComponent(typeof(PhotonView))]
public class ObjectStatus : MonoBehaviour, IPunInstantiateMagicCallback
{
    [SerializeField] public TMP_Text label;
    public bool grabbed = false;
    public bool activeMesh = false;
    private PhotonView photonView;

    private bool setByUI = false;
    private void Start()
    {
        if (setByUI) return;

        string numbers = Regex.Replace(gameObject.name, @"[^0-9]", "");
        Debug.Log($"name {gameObject.name} number: {numbers}");
        label?.SetText(numbers);
    }

    public void SetObjectStatus(bool grab)
    {
        this.grabbed = grab;
    }

    public void SetGrabbed(bool grab)
    {
        this.grabbed = grab;
    }

    public void SetActiveMesh(bool active)
    {
        this.activeMesh = active;
    }

    [PunRPC]
    public void SetLabelRPC(string text)
    {
        Debug.Log("THING");
        label?.SetText(text);
    }

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void ActivateShine()
    {   
        Transform shine = transform.Find("Shine");
        if (shine != null)
        {
            shine.gameObject.SetActive(true);
        }
        else
        {
            GameObject shineObj = Resources.Load("Prefabs/Particles/Shine") as GameObject;
            GameObject shineChild = Instantiate(shineObj, transform);
            shineChild.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            shineChild.name = "Shine";
        }
        
    }

    public void DeactivateShine()
    {
        Transform shine = transform.Find("Shine");
        if (shine != null)
        {
            shine.gameObject.SetActive(false);
        }
    }
    private void Update()
    {
        if (photonView.IsMine) // Example trigger
        {
            photonView.RPC("SyncTransform", RpcTarget.Others, transform.position, transform.rotation);
        }
    }

    [PunRPC]
    void SyncTransform(Vector3 newPosition, Quaternion newRotation)
    {
        if (!photonView.IsMine)
        {
            transform.position = newPosition;
            transform.rotation = newRotation;
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        setByUI = true;
        // Access instantiation data through the PhotonMessageInfo
        if (info.photonView.InstantiationData != null && info.photonView.InstantiationData.Length > 0)
        {
            string labelText = (string)info.photonView.InstantiationData[0];
            Debug.Log($"SET BY PHOTON {labelText}");
            label?.SetText(labelText);
        }
    }
}