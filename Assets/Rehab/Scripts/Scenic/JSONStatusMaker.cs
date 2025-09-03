using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Rehab.Scripts.Scenic
{
    /// <summary>
    /// Manages the serialization and communication of game state data between Unity and Scenic
    /// Handles player positions, joint angles, and object states for rehabilitation tracking
    /// </summary>
    public class JSONStatusMaker : MonoBehaviour
    {
        private ObjectsList objectList;
        private Root root;
        private int lastTick;
        [SerializeField] private ZmqServer server;
        private bool stop_scenic;
        private bool taskStarted;

        void Start()
        {
            objectList = GetComponent<ObjectsList>();
            lastTick = -1;
            server = GetComponent<ZmqServer>();
            stop_scenic = false;
            taskStarted = false;
        }

        /// <summary>
        /// Serializes the current game state to JSON for transmission
        /// </summary>
        public string GetUnityData()
        {
            return JsonConvert.SerializeObject(root);
        }

        /// <summary>
        /// Signals the Scenic program to stop execution
        /// </summary>
        public void QuitScenicProgram()
        {
            this.stop_scenic = true;
        }

        void Update()
        {
            // Track task state changes
            if (taskStarted && (server.lastTick == -1 || server.lastTick == 0))
            {
                taskStarted = false;
            }
            else if (!taskStarted && !(server.lastTick == -1 || server.lastTick == 0))
            {
                taskStarted = true;
            }

            // Reset stop flag when tick resets
            if (server.lastTick == -1 || server.lastTick == 0)
            {
                stop_scenic = false;
            }
            lastTick = server.lastTick;

            // Create new root object for this tick
            Root r = new Root();

            // Collect data from all players and objects
            foreach (var player in objectList.scenicPlayers)
            {
                Player p = new Player();
                AddPlayerData(player, p);
                r.TickData.ScenicPlayers.Add(p);
            }

            foreach (GameObject obj in objectList.scenicObjects)
            {
                Object o = new Object();
                AddObjectData(obj, o);
                r.TickData.ScenicObjects.Add(o);
            }

            root = r;
        }

        /// <summary>
        /// Collects and formats player data including position, joint angles, and status
        /// </summary>
        void AddPlayerData(GameObject player, Player pData)
        {
            if (player != null)
            {
                pData.movementData.stopButton = stop_scenic;
                Vector3ToJsonClass(player.transform.position, pData.movementData.transform);

                AvatarAngles avatarAngles = player.GetComponent<AvatarAngles>();
                CheckAvatarStatus avatarStatus = player.GetComponent<CheckAvatarStatus>();
                if (avatarAngles != null)
                    SetJointAngles(avatarAngles, pData.jointAngles);
                if (avatarStatus != null)
                    SetAvatarStatus(avatarStatus, pData.avatarStatus);
            }
        }

        /// <summary>
        /// Collects and formats object data including position, rotation, and state
        /// </summary>
        void AddObjectData(GameObject obj, Object oData)
        {
            Vector3ToJsonClass(obj.transform.position, oData.movementData.transform);
            QuaternionToJsonClass(obj.transform.rotation, oData.movementData.rotation);
            ObjectStatus objectStatus = obj.GetComponent<ObjectStatus>();
            if (objectStatus != null)
                SetObjectStatus(objectStatus, oData.objectState);
        }

        /// <summary>
        /// Updates object state data for serialization
        /// </summary>
        private void SetObjectStatus(ObjectStatus objStatus, ObjectState oData)
        {
            oData.Grabbed = objStatus.grabbed;
        }

        /// <summary>
        /// Maps joint angle data from AvatarAngles to serializable format
        /// </summary>
        private void SetJointAngles(AvatarAngles a, JointAngles b)
        {
            // Map all joint angles from source to target
            b.LeftElbow = a.leftElbow;
            b.RightElbow = a.rightElbow;

            b.LeftShoulderAbductionFlexion = a.leftShoulderAbductionFlexion;
            b.LeftHorizontalAbduction = a.leftHorizontalAbduction;

            b.RightShoulderAbductionFlexion = a.rightShoulderAbductionFlexion;
            b.RightHorizontalAbduction = a.rightHorizontalAbduction;

            b.LeftWristFlexion = a.leftWristFlexion;
            b.RightWristFlexion = a.rightWristFlexion;

            b.LeftWristSupination = a.leftWristSupination;
            b.RightWristSupination = a.rightWristSupination;

            b.LeftThumbIPFlexion = a.leftThumbIPFlexion;
            b.LeftThumbCMCFlexion = a.leftThumbCMCFlexion;
            b.LeftIndexMCPFlexion = a.leftIndexMCPFlexion;
            b.LeftIndexPIPFlexion = a.leftIndexPIPFlexion;
            b.LeftIndexDIPFlexion = a.leftIndexDIPFlexion;
            b.LeftMiddleMCPFlexion = a.leftMiddleMCPFlexion;
            b.LeftMiddlePIPFlexion = a.leftMiddlePIPFlexion;
            b.LeftMiddleDIPFlexion = a.leftMiddleDIPFlexion;
            b.LeftRingMCPFlexion = a.leftRingMCPFlexion;
            b.LeftRingPIPFlexion = a.leftRingPIPFlexion;
            b.LeftRingDIPFlexion = a.leftRingDIPFlexion;
            b.LeftPinkyMCPFlexion = a.leftPinkyMCPFlexion;
            b.LeftPinkyPIPFlexion = a.leftPinkyPIPFlexion;
            b.LeftPinkyDIPFlexion = a.leftPinkyDIPFlexion;

            b.RightThumbIPFlexion = a.rightThumbIPFlexion;
            b.RightThumbCMCFlexion = a.rightThumbCMCFlexion;
            b.RightIndexMCPFlexion = a.rightIndexMCPFlexion;
            b.RightIndexPIPFlexion = a.rightIndexPIPFlexion;
            b.RightIndexDIPFlexion = a.rightIndexDIPFlexion;
            b.RightMiddleMCPFlexion = a.rightMiddleMCPFlexion;
            b.RightMiddlePIPFlexion = a.rightMiddlePIPFlexion;
            b.RightMiddleDIPFlexion = a.rightMiddleDIPFlexion;
            b.RightRingMCPFlexion = a.rightRingMCPFlexion;
            b.RightRingPIPFlexion = a.rightRingPIPFlexion;
            b.RightRingDIPFlexion = a.rightRingDIPFlexion;
            b.RightPinkyMCPFlexion = a.rightPinkyMCPFlexion;
            b.RightPinkyPIPFlexion = a.rightPinkyPIPFlexion;
            b.RightPinkyDIPFlexion = a.rightPinkyDIPFlexion;

            b.RightKnee = a.rightKnee;
            b.LeftKnee = a.leftKnee;

            b.TrunkTilt = a.trunkTilt;

            b.HipFlexion = a.hipFlexion;

            Vector3ToJsonClass(a.rightPalm, b.RightPalm);
            Vector3ToJsonClass(a.leftPalm, b.LeftPalm);

            Vector3ToJsonClass(a.rightShoulderPos, b.RightShoulderPos);
            Vector3ToJsonClass(a.leftShoulderPos, b.LeftShoulderPos);

            Vector3ToJsonClass(a.mouthPos, b.MouthPos);

            // Map new Vector3 fields
            Vector3ToJsonClass(a.leftThumbTip, b.LeftThumbTip);
            Vector3ToJsonClass(a.leftIndexTip, b.LeftIndexTip);
            Vector3ToJsonClass(a.leftMiddleTip, b.LeftMiddleTip);
            Vector3ToJsonClass(a.leftRingTip, b.LeftRingTip);
            Vector3ToJsonClass(a.leftPinkyTip, b.LeftPinkyTip);

            Vector3ToJsonClass(a.rightThumbTip, b.RightThumbTip);
            Vector3ToJsonClass(a.rightIndexTip, b.RightIndexTip);
            Vector3ToJsonClass(a.rightMiddleTip, b.RightMiddleTip);
            Vector3ToJsonClass(a.rightRingTip, b.RightRingTip);
            Vector3ToJsonClass(a.rightPinkyTip, b.RightPinkyTip);

            Vector3ToJsonClass(a.leftWristPos, b.LeftWristPos);
            Vector3ToJsonClass(a.rightWristPos, b.RightWristPos);

            Vector3ToJsonClass(a.leftElbowPos, b.LeftElbowPos);
            Vector3ToJsonClass(a.rightElbowPos, b.RightElbowPos);

            Vector3ToJsonClass(a.chestPos, b.ChestPos);
            Vector3ToJsonClass(a.headsetPos, b.HeadsetPos);

            // Map new finger‐angle fields
            b.LeftThumbIndexAngle = a.leftThumbIndexAngle;
            b.LeftIndexMiddleAngle = a.leftIndexMiddleAngle;
            b.LeftMiddleRingAngle = a.leftMiddleRingAngle;
            b.LeftRingPinkyAngle = a.leftRingPinkyAngle;

            b.RightThumbIndexAngle = a.rightThumbIndexAngle;
            b.RightIndexMiddleAngle = a.rightIndexMiddleAngle;
            b.RightMiddleRingAngle = a.rightMiddleRingAngle;
            b.RightRingPinkyAngle = a.rightRingPinkyAngle;
        }


        /// <summary>
        /// Updates avatar status data for serialization
        /// </summary>
        private void SetAvatarStatus(CheckAvatarStatus dataAvatarStatus, AvatarStatus objectAvatarStatus)
        {
            objectAvatarStatus.Pain = dataAvatarStatus.pain;
            objectAvatarStatus.SpeakActionCount = dataAvatarStatus.speakActionCount;
            objectAvatarStatus.Fatigue = dataAvatarStatus.fatigue;
            objectAvatarStatus.Dizziness = dataAvatarStatus.dizziness;
            objectAvatarStatus.Anything = dataAvatarStatus.anything;
            objectAvatarStatus.Feedback = dataAvatarStatus.feedback;
            objectAvatarStatus.InProgress = dataAvatarStatus.inProgress;
            objectAvatarStatus.TaskDone = dataAvatarStatus.taskDone;
            objectAvatarStatus.StopProgram = dataAvatarStatus.stopProgram;
            objectAvatarStatus.ImageID = dataAvatarStatus.imageID;
        }

        /// <summary>
        /// Converts Unity Vector3 to JSON-compatible format with coordinate system adjustment
        /// </summary>
        void Vector3ToJsonClass(Vector3 v, Vector3Json vj)
        {
            vj.x = v.x;
            vj.z = v.y;
            vj.y = v.z;
        }

        /// <summary>
        /// Converts Unity Quaternion to JSON-compatible format
        /// </summary>
        void QuaternionToJsonClass(Quaternion q, QuaternionJson qj)
        {
            qj.x = q.x;
            qj.z = q.y;
            qj.y = q.z;
            qj.w = q.w;
        }

        /// <summary>
        /// Converts a list of Vector3 to JSON-compatible format
        /// </summary>
        void Vector3ListToJsonList(List<Vector3> v3List, List<Vector3Json> v3jList)
        {
            for (int i = 0; i < v3List.Count; i++)
            {
                Vector3Json v3j = new Vector3Json();
                Vector3ToJsonClass(v3List[i], v3j);
                v3jList.Add(v3j);
            }
        }

        // JSON serialization classes
        public class Vector3Json
        {
            public Vector3Json()
            {
                this.x = 0f;
                this.y = 0f;
                this.z = 0f;
            }
            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }
        }

        public class QuaternionJson
        {
            public QuaternionJson()
            {
                this.x = 0f;
                this.y = 0f;
                this.z = 0f;
                this.w = 1f;
            }
            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }
            public float w { get; set; }
        }

        /// <summary>
        /// Contains all status information for an avatar/player
        /// </summary>
        public class AvatarStatus
        {
            public string Pain { get; set; } = "";
            public int SpeakActionCount { get; set; } = 0;
            public string Fatigue { get; set; } = "";
            public string Dizziness { get; set; } = "";
            public string Anything { get; set; } = "";
            public bool TaskDone { get; set; } = false;
            public bool InProgress { get; set; } = false;
            public bool StopProgram { get; set; } = false;
            public string Feedback { get; set; } = "";
            public string ImageID { get; set; } = "";
        }

        /// <summary>
        /// Contains all joint angle data for avatar movement tracking
        /// </summary>
        public class JointAngles
        {
            public float LeftShoulderAbductionFlexion { get; set; } = 0;
            public float LeftHorizontalAbduction { get; set; } = 0;

            public float RightShoulderAbductionFlexion { get; set; } = 0;
            public float RightHorizontalAbduction { get; set; } = 0;

            public float LeftWristFlexion { get; set; } = 0;
            public float RightWristFlexion { get; set; } = 0;

            public float LeftWristSupination { get; set; } = 0;
            public float RightWristSupination { get; set; } = 0;

            public float LeftThumbIPFlexion { get; set; } = 0;
            public float LeftThumbCMCFlexion { get; set; } = 0;
            public float LeftIndexMCPFlexion { get; set; } = 0;
            public float LeftIndexPIPFlexion { get; set; } = 0;
            public float LeftIndexDIPFlexion { get; set; } = 0;
            public float LeftMiddleMCPFlexion { get; set; } = 0;
            public float LeftMiddlePIPFlexion { get; set; } = 0;
            public float LeftMiddleDIPFlexion { get; set; } = 0;
            public float LeftRingMCPFlexion { get; set; } = 0;
            public float LeftRingPIPFlexion { get; set; } = 0;
            public float LeftRingDIPFlexion { get; set; } = 0;
            public float LeftPinkyMCPFlexion { get; set; } = 0;
            public float LeftPinkyPIPFlexion { get; set; } = 0;
            public float LeftPinkyDIPFlexion { get; set; } = 0;

            public float RightThumbIPFlexion { get; set; } = 0;
            public float RightThumbCMCFlexion { get; set; } = 0;
            public float RightIndexMCPFlexion { get; set; } = 0;
            public float RightIndexPIPFlexion { get; set; } = 0;
            public float RightIndexDIPFlexion { get; set; } = 0;
            public float RightMiddleMCPFlexion { get; set; } = 0;
            public float RightMiddlePIPFlexion { get; set; } = 0;
            public float RightMiddleDIPFlexion { get; set; } = 0;
            public float RightRingMCPFlexion { get; set; } = 0;
            public float RightRingPIPFlexion { get; set; } = 0;
            public float RightRingDIPFlexion { get; set; } = 0;
            public float RightPinkyMCPFlexion { get; set; } = 0;
            public float RightPinkyPIPFlexion { get; set; } = 0;
            public float RightPinkyDIPFlexion { get; set; } = 0;

            public float LeftElbow { get; set; } = 0;
            public float LeftKnee { get; set; } = 0;

            public float RightElbow { get; set; } = 0;
            public float RightKnee { get; set; } = 0;

            public float TrunkTilt { get; set; } = 0;

            public float HipFlexion { get; set; } = 0;

            public Vector3Json LeftPalm { get; set; } = new Vector3Json();
            public Vector3Json RightPalm { get; set; } = new Vector3Json();

            public Vector3Json LeftShoulderPos { get; set; } = new Vector3Json();
            public Vector3Json RightShoulderPos { get; set; } = new Vector3Json();

            public Vector3Json MouthPos { get; set; } = new Vector3Json();

            public Vector3Json LeftThumbTip { get; set; } = new Vector3Json();
            public Vector3Json LeftIndexTip { get; set; } = new Vector3Json();
            public Vector3Json LeftMiddleTip { get; set; } = new Vector3Json();
            public Vector3Json LeftRingTip { get; set; } = new Vector3Json();
            public Vector3Json LeftPinkyTip { get; set; } = new Vector3Json();

            public Vector3Json RightThumbTip { get; set; } = new Vector3Json();
            public Vector3Json RightIndexTip { get; set; } = new Vector3Json();
            public Vector3Json RightMiddleTip { get; set; } = new Vector3Json();
            public Vector3Json RightRingTip { get; set; } = new Vector3Json();
            public Vector3Json RightPinkyTip { get; set; } = new Vector3Json();

            public Vector3Json LeftWristPos { get; set; } = new Vector3Json();
            public Vector3Json RightWristPos { get; set; } = new Vector3Json();

            public Vector3Json LeftElbowPos { get; set; } = new Vector3Json();
            public Vector3Json RightElbowPos { get; set; } = new Vector3Json();

            public Vector3Json ChestPos { get; set; } = new Vector3Json();
            public Vector3Json HeadsetPos { get; set; } = new Vector3Json();

            public float LeftThumbIndexAngle { get; set; } = 0;
            public float LeftIndexMiddleAngle { get; set; } = 0;
            public float LeftMiddleRingAngle { get; set; } = 0;
            public float LeftRingPinkyAngle { get; set; } = 0;

            public float RightThumbIndexAngle { get; set; } = 0;
            public float RightIndexMiddleAngle { get; set; } = 0;
            public float RightMiddleRingAngle { get; set; } = 0;
            public float RightRingPinkyAngle { get; set; } = 0;
        }


        /// <summary>
        /// Contains movement and transform data for objects and players
        /// </summary>
        public class MovementData
        {
            public MovementData()
            {
                transform = new Vector3Json();
                speed = 0.0;
                velocity = new Vector3Json();
                rotation = new QuaternionJson();
                stopButton = false;
            }
            public Vector3Json transform { get; set; }
            public double speed { get; set; }
            public Vector3Json velocity { get; set; }
            public QuaternionJson rotation { get; set; }
            public bool stopButton { get; set; }
        }

        /// <summary>
        /// Contains all data for a player/avatar in the scene
        /// </summary>
        public class Player
        {
            public MovementData movementData { get; set; } = new();
            public JointAngles jointAngles { get; set; } = new();
            public AvatarStatus avatarStatus { get; set; } = new();
            public int clientID { get; set; }
        }

        /// <summary>
        /// Contains state information for interactive objects
        /// </summary>
        public class ObjectState
        {
            public bool Grabbed { get; set; } = false;
        }

        /// <summary>
        /// Contains all data for an object in the scene
        /// </summary>
        public class Object
        {
            public MovementData movementData { get; set; } = new();
            public ObjectState objectState { get; set; } = new();
        }

        /// <summary>
        /// Contains all data for a single game tick
        /// </summary>
        public class TickData
        {
            public int numPlayers { get; set; } = 0;
            public List<Player> ScenicPlayers { get; set; } = new();
            public List<Object> ScenicObjects { get; set; } = new();
        }

        /// <summary>
        /// Root class containing all game state data for serialization
        /// </summary>
        public class Root
        {
            public Root()
            {
                this.TickData = new TickData();
            }
            public TickData TickData { get; set; }
        }
    }
}
