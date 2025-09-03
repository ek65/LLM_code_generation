using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// comment

public class TextLocation : MonoBehaviour
{
    private string _posrot;
    public TextMeshProUGUI text;
    private void Update()
    {
        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;
        float rotX = transform.rotation.eulerAngles.x;
        float rotY = transform.rotation.eulerAngles.y;
        float rotZ = transform.rotation.eulerAngles.z;
        _posrot = $"Position: x = {x}, y = {y}, z ={z} \n Rotation x = {rotX}, y = {rotY}, z ={rotZ}";
        text.text = _posrot;
    }
}
