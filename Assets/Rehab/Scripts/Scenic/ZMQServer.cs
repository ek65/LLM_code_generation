using System;
using System.Collections.Generic;
using NetMQ;
using UnityEngine;

namespace Rehab.Scripts.Scenic
{
    /// <summary>
    /// Manages communication between Unity and Scenic using ZMQ protocol
    /// Handles bidirectional data exchange for movement and state synchronization
    /// </summary>
    public class ZmqServer : MonoBehaviour
    {
        [SerializeField] private string ip;
        [SerializeField] private string port = "5555";

        private ScenicParser parser;
        private ZMQRequester zmqRequester;
        public int lastTick;
        private ObjectsList objectList;
        private bool destroyed;
        private JSONStatusMaker sender;
        public bool StartServer = false;
        private bool windows;
        [SerializeField] private bool stopScenicProgram;
        private float checkTime = 2f; // Time interval to check for server activity
        private float lastCheckTime;
        private bool isServerOn;

        /// <summary>
        /// Detects the operating system platform for proper ZMQ cleanup
        /// </summary>
        private void Awake()
        {
            if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
            {
                Debug.Log("Running on macOS");
                windows = false;
            }
            else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                Debug.Log("Running on Windows");
                windows = true;
            }
            else
            {
                Debug.Log("Running on an unknown platform");
                windows = false;
            }
        }

        /// <summary>
        /// Signals Scenic program to stop execution
        /// </summary>
        public void StopScenicProgram()
        {
            sender.QuitScenicProgram();
        }

        /// <summary>
        /// Returns the ZMQ connection address
        /// </summary>
        public string GetAddress()
        {
            return "tcp://" + ip + ":";
        }

        /// <summary>
        /// Initializes ZMQ connection and required components
        /// </summary>
        void Start()
        {
            if (ip == null || port == null)
            {
                throw new Exception();
            }
            zmqRequester = new ZMQRequester(ip, port, true);
            zmqRequester.Start();
            destroyed = false;
            lastTick = -1;
            objectList = GetComponent<ObjectsList>();
            parser = new ScenicParser();
            sender = this.gameObject.GetComponent<JSONStatusMaker>();
            StartServer = true;
        }

        /// <summary>
        /// Main communication loop: sends Unity state and processes Scenic commands
        /// </summary>
        void Update()
        {
            if (!StartServer) return;

            if (stopScenicProgram)
            {
                StopScenicProgram();
                stopScenicProgram = false;
            }
        
            // Check for server timeout
            if (isServerOn && Time.time - lastCheckTime >= checkTime)
            {
                Debug.Log("lastTick has not changed for 2 seconds!");
                objectList.Reset();
                isServerOn = false;
            }
        
            // Send Unity state to Scenic
            string newSendData = sender.GetUnityData();
            zmqRequester.SetSendData(newSendData);
        
            // Receive and process Scenic commands
            string newData = zmqRequester.GetData();
            if (newData == null || newData.Equals("Null") || newData.Equals(""))
            {
                return;
            }

            try
            {
                ScenicParser.ScenicJson jsonResult = parser.ParseData(newData);
                int scenicTick = GetTickFromData(jsonResult);
                int newTick = -1;
                if (!destroyed || scenicTick == 0)
                {
                    newTick = scenicTick;
                    destroyed = false;
                }

                if (newTick == lastTick) return;

                // Check for skipped ticks
                if (newTick > lastTick + 10)
                {
                    Debug.LogError("A scenic tick might have been skipped. Last Tick = " + lastTick.ToString() +
                                   " New Tick = " + newTick.ToString());
                }

                lastTick = newTick;
                lastCheckTime = Time.time;
                isServerOn = true;
                List<ScenicMovementData> mvData = ParseMovementData(jsonResult);
                ApplyMovement(mvData);
            }
            catch (NullReferenceException e)
            {
                Debug.LogError("json failed " + e);
            }
            catch (Exception e)
            {
                Debug.LogError("ZMQ Server Error: " + e);
            }
        }

        /// <summary>
        /// Cleanup ZMQ resources on component destruction
        /// </summary>
        private void OnDestroy()
        {
            zmqRequester.Stop();
            StartServer = false;
            if (windows)
                NetMQConfig.Cleanup(false); 
        }

        /// <summary>
        /// Cleanup ZMQ resources on application quit
        /// </summary>
        private void OnApplicationQuit()
        {
            zmqRequester.Stop();
            StartServer = false;
            if (windows)
                NetMQConfig.Cleanup(false); 
        }

        /// <summary>
        /// Parses movement data from Scenic JSON response
        /// </summary>
        private List<ScenicMovementData> ParseMovementData(ScenicParser.ScenicJson data)
        {
            return parser.ScenicMovementParser(data);
        }
    
        /// <summary>
        /// Extracts tick number from Scenic JSON data
        /// </summary>
        private int GetTickFromData(ScenicParser.ScenicJson data)
        {
            return data.TimestepNumber;
        }
    
        /// <summary>
        /// Applies movement commands to the player interface
        /// </summary>
        private void ApplyMovement(List<ScenicMovementData> movementData) 
        {
            for(int i = 0; i < movementData.Count; i++)
            {
                PlayerInterface p = objectList.scenicPlayers[0].GetComponentInChildren<PlayerInterface>();
                if (p != null)
                    p.ApplyMovement(movementData[i]);
            }
        }

        /// <summary>
        /// Resets the server tick counter
        /// </summary>
        public void ResetTickServerRpc()
        {
            destroyed = true;
            lastTick = -1;
        }
    }
}
