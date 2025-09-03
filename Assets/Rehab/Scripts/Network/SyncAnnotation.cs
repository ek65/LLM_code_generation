using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.Windows;

public class SyncAnnotation : MonoBehaviour
{
    private bool setByUI = false;

    // Quick fix for homework labeling. Need it to be the gameobject name number
    private void Start()
    {
        if (setByUI) return; // We don't want to override if this was spawned by UI. This is only for scenic spawning

        string numbers = Regex.Replace(gameObject.name, @"[^0-9]", "");
        Debug.Log($"name {gameObject.name} number: {numbers}");
        transform.GetChild(0).GetComponent<TextMeshPro>().text = numbers;
    }

    [Photon.Pun.PunRPC]
    public void SyncAnnotationCount(string count)
    {
        setByUI = true;
        if (count != null)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).GetComponent<TextMeshPro>().text = count;
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }

    }
}
