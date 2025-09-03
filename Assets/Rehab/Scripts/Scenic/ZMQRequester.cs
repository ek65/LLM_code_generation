using System.Threading;
using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

namespace Rehab.Scripts.Scenic
{
    public class ZMQRequester : RunAbleThread
    {
        private string ip;
        private string port;
        private bool isInit;
        private bool isServer;
        public string data;
        private string outData;
        private bool readyToCommunicate;
        private string address;
        public ZMQRequester(string ip, string port, bool isServer)
        {
            this.ip = ip;
            this.port = port;
            this.isInit = false;
            this.isServer = isServer;
            data = null;
            readyToCommunicate = true;
        }

        public string getAddress()
        {
            return address;
        } 
        protected override void Run()
        {
            ForceDotNet.Force(); //this prevents unity freezing idk why 
            if (isServer)
            {
                using (ResponseSocket server = new ResponseSocket())
                {
                    address = "tcp://" + ip + ":" + port;
                    server.Bind("tcp://"+ ip +":" + port);
                    string message = null;
                    string outMessage = null;
                    //int outNum = 0;
                    bool gotMessage = false;
                    //understand what is going on here and try to terminate socket yet still keep the same thread running 
                    while (Running)
                    {
                        // Debug.Log(outData == null);
                        if (outData != null){
                            while (Running)
                            {
                                //Debug.Log("I am receiving");
                                gotMessage = server.TryReceiveFrameString(out message);
                                if (gotMessage)
                                {
                                    data = message;
                                    break;
                                }
                            }
                            if (message != null)
                            {
                                data = message;
                            } 
                            else
                            {
                                Debug.LogWarning("Received scenic data is NULL");
                            }
                            outMessage = outData;
                            if (!readyToCommunicate)
                            {
                                bool humanReady = false;
                                while (!humanReady)
                                {
                                    if (readyToCommunicate)
                                    {
                                        server.TrySendFrame(outMessage);
                                        Thread.Sleep(100);
                                        humanReady = true;
                                    }
                                }
                            
                            }
                            else
                            {
                                server.TrySendFrame(outMessage);
                                //Debug.Log(outMessage);
                            
                                // outNum++;
                                Thread.Sleep(100);
                            }
                        
                        }
                        else
                        {
                            // Debug.LogError("Outdata is null. Zmq cannot load. Check if Scenic Is Running");
                        }
                    }
                
                    server.Close();
                    server.Dispose();
                }
            }
        }
    
        public string GetData()
        {
            return data;
        }
        public void SetSendData(string message)
        {
            outData = message;
        }
        public void SetReady(bool b)
        {
            readyToCommunicate = b;
        }

    }
}