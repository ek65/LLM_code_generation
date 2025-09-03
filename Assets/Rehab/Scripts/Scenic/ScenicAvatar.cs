using System.Collections.Generic;
using UnityEngine;
using System;

namespace Rehab.Scripts.Scenic
{
    public class ScenicAvatar : MonoBehaviour
    {
        private GameObject avatar;
        private AvatarAngles avatarAngles;
        private Dictionary<string, Transform> jointDictionary;
        private float timeElapsed = 0f;

        void Start()
        {
            avatar = GameObject.FindGameObjectWithTag("Avatar").transform.Find("Skeleton").Find("Hips").gameObject;
            jointDictionary = new Dictionary<string, Transform>();
            PopulateJointDictionary(avatar.GetComponent<Transform>());
            avatarAngles = GetComponent<AvatarAngles>();
        }

        //using recursion to add all children of avatar into dictionary
        void PopulateJointDictionary(Transform parent)
        {
            if (!jointDictionary.ContainsKey(parent.name))
            {
                jointDictionary.Add(parent.name, parent);
            }
            foreach (Transform child in parent)
            {
                PopulateJointDictionary(child);
            }
        }



        float CalculateAngle(string joint1, string joint2, string joint3)
        {
            // Vectors from joint2 to joint1 and joint3
            Vector3 vector1 = jointDictionary[joint1].position - jointDictionary[joint2].position;
            Vector3 vector2 = jointDictionary[joint3].position - jointDictionary[joint2].position;

            // Normalize the vectors to avoid scale issues
            vector1.Normalize();
            vector2.Normalize();

            float dotProduct = Vector3.Dot(vector1, vector2);
            float angleRad = Mathf.Acos(dotProduct);
            float angleDeg = angleRad * Mathf.Rad2Deg;

            return angleDeg;
        }

        float CalculateAngle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            // Vectors from joint2 to joint1 and joint3
            Vector3 vector1 = v1 - v2;
            Vector3 vector2 = v3 - v2;

            // Normalize the vectors to avoid scale issues
            vector1.Normalize();
            vector2.Normalize();

            float dotProduct = Vector3.Dot(vector1, vector2);
            float angleRad = Mathf.Acos(dotProduct);
            float angleDeg = angleRad * Mathf.Rad2Deg;

            return angleDeg;
        }

        public float ComputeSupination(bool isLeft)
        {
            Transform lowerArm;
            Transform lowerArmEnd;
            Transform hand;

            if (isLeft)
            {
                lowerArm = jointDictionary["Left_LowerArm"];
                lowerArmEnd = jointDictionary["Left_LowerArmEnd"];
                hand = jointDictionary["Left_Hand"];
            }
            else
            {
                lowerArm = jointDictionary["Right_LowerArm"];
                lowerArmEnd = jointDictionary["Right_LowerArmEnd"];
                hand = jointDictionary["Right_Hand"];
            }

            Vector3 forearmAxisWorld = (lowerArmEnd.position - lowerArm.position).normalized;
            Quaternion forearmToHand = Quaternion.Inverse(lowerArm.rotation) * hand.rotation;
            Vector3 localForearmAxis = lowerArm.InverseTransformDirection(forearmAxisWorld);
            Quaternion twist = ExtractTwist(forearmToHand, localForearmAxis);
            twist.ToAngleAxis(out float angleDeg, out Vector3 axis);
            float signedAngle = Vector3.Dot(axis, localForearmAxis) >= 0f ? angleDeg : -angleDeg;
            if (signedAngle > 180f) signedAngle -= 360f;

            if (isLeft) return signedAngle;
            return -signedAngle;
        }

        private static Quaternion ExtractTwist(Quaternion rotation, Vector3 axis)
        {
            axis.Normalize();
            Vector3 vec = new Vector3(rotation.x, rotation.y, rotation.z);
            Vector3 proj = Vector3.Project(vec, axis);
            return new Quaternion(proj.x, proj.y, proj.z, rotation.w).normalized;
        }

        private void Update()
        {
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= 0.2f) // Check if 0.1 sec has passed
            {
                timeElapsed = 0f; // Reset timer
            }
            else
            {
                return;
            }
            // Match position with actual avatar
            this.transform.position = avatar.transform.position;
            UpdateAngles();
            avatarAngles.RefreshDisplay();
        }
        private void UpdateAngles()
        {
            //updating all joint angles
            avatarAngles.leftElbow = CalculateAngle("Left_UpperArm", "Left_LowerArm", "Left_LowerArmEnd");
            avatarAngles.rightElbow = CalculateAngle("Right_UpperArm", "Right_LowerArm", "Right_LowerArmEnd");

            avatarAngles.leftShoulderAbductionFlexion = CalculateAngle("Left_UpperLeg", "Left_UpperArm", "Left_LowerArm");
            avatarAngles.rightShoulderAbductionFlexion = CalculateAngle("Right_UpperLeg", "Right_UpperArm", "Right_LowerArm");

            avatarAngles.leftHorizontalAbduction = CalculateAngle("Left_UpperArm", "Left_UpperArm", "Left_LowerArm") - 90;
            avatarAngles.rightHorizontalAbduction = CalculateAngle("Right_UpperArm", "Right_UpperArm", "Right_LowerArm") - 90;

            // avatarAngles.leftWristFlexion = Math.Abs(CalculateAngle("Left_LowerArm", "Left_Hand", "Left_MiddleProximal") - 180);
            // avatarAngles.rightWristFlexion = Math.Abs(CalculateAngle("Right_LowerArm", "Right_Hand", "Right_MiddleProximal") - 180);

            avatarAngles.leftWristFlexion = jointDictionary["Left_Hand"].localEulerAngles.z;
            if (avatarAngles.leftWristFlexion > 180) avatarAngles.leftWristFlexion -= 360;
            avatarAngles.rightWristFlexion = jointDictionary["Right_Hand"].localEulerAngles.z;
            if (avatarAngles.rightWristFlexion > 180) avatarAngles.rightWristFlexion -= 360;
            avatarAngles.leftWristExtension = -avatarAngles.leftWristFlexion;
            avatarAngles.rightWristExtension = -avatarAngles.rightWristFlexion;

            avatarAngles.leftWristSupination = ComputeSupination(true);
            avatarAngles.rightWristSupination = ComputeSupination(false);

            avatarAngles.leftThumbIPFlexion = CalculateAngle("Left_ThumbIntermediate", "Left_ThumbDistal", "Left_ThumbDistalEnd");
            avatarAngles.rightThumbIPFlexion = CalculateAngle("Right_ThumbIntermediate", "Right_ThumbDistal", "Right_ThumbDistalEnd");

            avatarAngles.leftThumbCMCFlexion = CalculateAngle("Left_ThumbProximal", "Left_ThumbIntermediate", "Left_ThumbDistal");
            avatarAngles.rightThumbCMCFlexion = CalculateAngle("Right_ThumbProximal", "Right_ThumbIntermediate", "Right_ThumbDistal");

            avatarAngles.leftIndexMCPFlexion = CalculateAngle("Left_Hand", "Left_IndexProximal", "Left_IndexIntermediate");
            avatarAngles.rightIndexMCPFlexion = CalculateAngle("Right_Hand", "Right_IndexProximal", "Right_IndexIntermediate");

            avatarAngles.leftIndexPIPFlexion = CalculateAngle("Left_IndexProximal", "Left_IndexIntermediate", "Left_IndexDistal");
            avatarAngles.rightIndexPIPFlexion = CalculateAngle("Right_IndexProximal", "Right_IndexIntermediate", "Right_IndexDistal");

            avatarAngles.leftIndexDIPFlexion = CalculateAngle("Left_IndexIntermediate", "Left_IndexDistal", "Left_IndexDistalEnd");
            avatarAngles.rightIndexDIPFlexion = CalculateAngle("Right_IndexIntermediate", "Right_IndexDistal", "Right_IndexDistalEnd");

            avatarAngles.leftMiddleMCPFlexion = CalculateAngle("Left_Hand", "Left_MiddleProximal", "Left_MiddleIntermediate");
            avatarAngles.rightMiddleMCPFlexion = CalculateAngle("Right_Hand", "Right_MiddleProximal", "Right_MiddleIntermediate");

            avatarAngles.leftMiddlePIPFlexion = CalculateAngle("Left_MiddleProximal", "Left_MiddleIntermediate", "Left_MiddleDistal");
            avatarAngles.rightMiddlePIPFlexion = CalculateAngle("Right_MiddleProximal", "Right_MiddleIntermediate", "Right_MiddleDistal");

            avatarAngles.leftMiddleDIPFlexion = CalculateAngle("Left_MiddleIntermediate", "Left_MiddleDistal", "Left_MiddleDistalEnd");
            avatarAngles.rightMiddleDIPFlexion = CalculateAngle("Right_MiddleIntermediate", "Right_MiddleDistal", "Right_MiddleDistalEnd");

            avatarAngles.leftRingMCPFlexion = CalculateAngle("Left_Hand", "Left_RingProximal", "Left_RingIntermediate");
            avatarAngles.rightRingMCPFlexion = CalculateAngle("Right_Hand", "Right_RingProximal", "Right_RingIntermediate");

            avatarAngles.leftRingPIPFlexion = CalculateAngle("Left_RingProximal", "Left_RingIntermediate", "Left_RingDistal");
            avatarAngles.rightRingPIPFlexion = CalculateAngle("Right_RingProximal", "Right_RingIntermediate", "Right_RingDistal");

            avatarAngles.leftRingDIPFlexion = CalculateAngle("Left_RingIntermediate", "Left_RingDistal", "Left_RingDistalEnd");
            avatarAngles.rightRingDIPFlexion = CalculateAngle("Right_RingIntermediate", "Right_RingDistal", "Right_RingDistalEnd");

            avatarAngles.leftPinkyMCPFlexion = CalculateAngle("Left_Hand", "Left_PinkyProximal", "Left_PinkyIntermediate");
            avatarAngles.rightPinkyMCPFlexion = CalculateAngle("Right_Hand", "Right_PinkyProximal", "Right_PinkyIntermediate");

            avatarAngles.leftPinkyPIPFlexion = CalculateAngle("Left_PinkyProximal", "Left_PinkyIntermediate", "Left_PinkyDistal");
            avatarAngles.rightPinkyPIPFlexion = CalculateAngle("Right_PinkyProximal", "Right_PinkyIntermediate", "Right_PinkyDistal");

            avatarAngles.leftPinkyDIPFlexion = CalculateAngle("Left_PinkyIntermediate", "Left_PinkyDistal", "Left_PinkyDistalEnd");
            avatarAngles.rightPinkyDIPFlexion = CalculateAngle("Right_PinkyIntermediate", "Right_PinkyDistal", "Right_PinkyDistalEnd");


            // avatarAngles.leftKnee = Math.Abs(CalculateAngle("Left_UpperLeg", "Left_LowerLeg", "Left_Foot") - 180);
            // avatarAngles.rightKnee = Math.Abs(CalculateAngle("Right_UpperLeg", "Right_LowerLeg", "Right_Foot") - 180);

            avatarAngles.trunkTilt = CalculateAngle(jointDictionary["Neck"].position, jointDictionary["Hips"].position, jointDictionary["Hips"].position + Vector3.up);

            avatarAngles.hipFlexion = jointDictionary["Spine"].localEulerAngles.x;
            if (avatarAngles.hipFlexion > 180) avatarAngles.hipFlexion -= 360;

            //update positions
            avatarAngles.leftPalm = jointDictionary["Left_MiddleProximal"].position;
            avatarAngles.rightPalm = jointDictionary["Right_MiddleProximal"].position;

            avatarAngles.leftShoulderPos = jointDictionary["Left_UpperArm"].position;
            avatarAngles.rightShoulderPos = jointDictionary["Right_UpperArm"].position;

            avatarAngles.mouthPos = jointDictionary["Jaw"].position;


            avatarAngles.leftThumbTip = jointDictionary["Left_ThumbDistalEnd"].position;
            avatarAngles.leftIndexTip = jointDictionary["Left_IndexDistalEnd"].position;
            avatarAngles.leftMiddleTip = jointDictionary["Left_MiddleDistalEnd"].position;
            avatarAngles.leftRingTip = jointDictionary["Left_RingDistalEnd"].position;
            avatarAngles.leftPinkyTip = jointDictionary["Left_PinkyDistalEnd"].position;

            avatarAngles.rightThumbTip = jointDictionary["Right_ThumbDistalEnd"].position;
            avatarAngles.rightIndexTip = jointDictionary["Right_IndexDistalEnd"].position;
            avatarAngles.rightMiddleTip = jointDictionary["Right_MiddleDistalEnd"].position;
            avatarAngles.rightRingTip = jointDictionary["Right_RingDistalEnd"].position;
            avatarAngles.rightPinkyTip = jointDictionary["Right_PinkyDistalEnd"].position;

            avatarAngles.leftWristPos = jointDictionary["Left_Hand"].position;
            avatarAngles.rightWristPos = jointDictionary["Right_Hand"].position;

            avatarAngles.leftElbowPos = jointDictionary["Left_LowerArm"].position;
            avatarAngles.rightElbowPos = jointDictionary["Right_LowerArm"].position;

            avatarAngles.chestPos = jointDictionary["UpperChest"].position;
            avatarAngles.headsetPos = jointDictionary["Left_Eye"].position;

            // update angles between adjacent fingers
            avatarAngles.leftThumbIndexAngle = CalculateAngle("Left_ThumbDistalEnd", "Left_Hand", "Left_IndexDistalEnd");
            avatarAngles.leftIndexMiddleAngle = CalculateAngle("Left_IndexDistalEnd", "Left_Hand", "Left_MiddleDistalEnd");
            avatarAngles.leftMiddleRingAngle = CalculateAngle("Left_MiddleDistalEnd", "Left_Hand", "Left_RingDistalEnd");
            avatarAngles.leftRingPinkyAngle = CalculateAngle("Left_RingDistalEnd", "Left_Hand", "Left_PinkyDistalEnd");

            avatarAngles.rightThumbIndexAngle = CalculateAngle("Right_ThumbDistalEnd", "Right_Hand", "Right_IndexDistalEnd");
            avatarAngles.rightIndexMiddleAngle = CalculateAngle("Right_IndexDistalEnd", "Right_Hand", "Right_MiddleDistalEnd");
            avatarAngles.rightMiddleRingAngle = CalculateAngle("Right_MiddleDistalEnd", "Right_Hand", "Right_RingDistalEnd");
            avatarAngles.rightRingPinkyAngle = CalculateAngle("Right_RingDistalEnd", "Right_Hand", "Right_PinkyDistalEnd");


        }
    }
}
