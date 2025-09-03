using System;
using System.Collections;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using UnityEngine;
using TMPro;

public class RoomMeshChanger : MonoBehaviour
{
    public Material newMaterial; 
    public TMP_Text text; 
    public void SetRoomMaterials(float delay)
    {
        StartCoroutine(DelayedGetRoomData(delay)); 
    }
    IEnumerator DelayedGetRoomData(float delay)
    {
        yield return new WaitForSeconds(delay);
        GetRoomData();
    }

    public void GetRoomData()
    {
        foreach (MRUKRoom room in MRUK.Instance.Rooms)
        {
            if (room)
            {
                string logg = "";
                foreach (Transform r in room.gameObject.transform)
                {
                    logg = "Name: " + r.gameObject.name + " : " + r.position + "\n";
                }
                Renderer[] renderers = room.GetComponentsInChildren<Renderer>();

                foreach (Renderer rend in renderers)
                {
                    Material[] mats = rend.materials;
                    for (int i = 0; i < mats.Length; i++)
                    {
                        mats[i] = newMaterial;
                    }
                    rend.materials = mats;
                }
            }
        }
    }
    
}
