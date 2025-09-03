using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using System;
using System.Reflection;

public class AvatarAngles : MonoBehaviour
{
    public float leftElbow;
    public float rightElbow;

    public float leftShoulderAbductionFlexion;
    public float leftHorizontalAbduction;

    public float rightShoulderAbductionFlexion;
    public float rightHorizontalAbduction;

    public float leftWristFlexion;
    public float rightWristFlexion;
    public float leftWristExtension;
    public float rightWristExtension;

    public float leftWristSupination;
    public float rightWristSupination;

    public float leftThumbIPFlexion;
    public float leftThumbCMCFlexion;
    public float leftIndexMCPFlexion;
    public float leftIndexPIPFlexion;
    public float leftIndexDIPFlexion;
    public float leftMiddleMCPFlexion;
    public float leftMiddlePIPFlexion;
    public float leftMiddleDIPFlexion;
    public float leftRingMCPFlexion;
    public float leftRingPIPFlexion;
    public float leftRingDIPFlexion;
    public float leftPinkyMCPFlexion;
    public float leftPinkyPIPFlexion;
    public float leftPinkyDIPFlexion;

    public float rightThumbIPFlexion;
    public float rightThumbCMCFlexion;
    public float rightIndexMCPFlexion;
    public float rightIndexPIPFlexion;
    public float rightIndexDIPFlexion;
    public float rightMiddleMCPFlexion;
    public float rightMiddlePIPFlexion;
    public float rightMiddleDIPFlexion;
    public float rightRingMCPFlexion;
    public float rightRingPIPFlexion;
    public float rightRingDIPFlexion;
    public float rightPinkyMCPFlexion;
    public float rightPinkyPIPFlexion;
    public float rightPinkyDIPFlexion;

    public float trunkTilt;

    public float hipFlexion;

    public float leftKnee;
    public float rightKnee;

    public Vector3 leftPalm;
    public Vector3 rightPalm;

    public Vector3 leftShoulderPos;
    public Vector3 rightShoulderPos;

    public Vector3 mouthPos;

    public Vector3 leftThumbTip;
    public Vector3 leftIndexTip;
    public Vector3 leftMiddleTip;
    public Vector3 leftRingTip;
    public Vector3 leftPinkyTip;

    public Vector3 rightThumbTip;
    public Vector3 rightIndexTip;
    public Vector3 rightMiddleTip;
    public Vector3 rightRingTip;
    public Vector3 rightPinkyTip;

    public Vector3 leftWristPos;
    public Vector3 rightWristPos;

    public Vector3 leftElbowPos;
    public Vector3 rightElbowPos;

    public Vector3 chestPos;
    public Vector3 headsetPos;

    public float leftThumbIndexAngle;
    public float leftIndexMiddleAngle;
    public float leftMiddleRingAngle;
    public float leftRingPinkyAngle;

    public float rightThumbIndexAngle;
    public float rightIndexMiddleAngle;
    public float rightMiddleRingAngle;
    public float rightRingPinkyAngle;

    [SerializeField] private TMP_Text displayText;



    public void RefreshDisplay()
    {
        if (displayText == null)
        {
            return;
        }
        string output = "";
        FieldInfo[] fields = this.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            if (field.FieldType == typeof(float))
            {
                int value = Convert.ToInt32(field.GetValue(this));
                output += $"{field.Name}: {value}\n";
            }
        }
        displayText.text = output;


    }

}
