using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace Rehab.Scripts.Scenic
{
    public class ScenicParser
    {
        public ScenicJson ParseData(string json)
        {
            var jsonResult = JsonConvert.DeserializeObject(json).ToString();
            ScenicJson t = JsonConvert.DeserializeObject<ScenicJson>(jsonResult);
            return t;
        }
        public List<ScenicMovementData> ScenicMovementParser(ScenicJson t)
        {
            HandleControl(t);
            List<ScenicMovementData> objData = new List<ScenicMovementData>();
            foreach (Object p in t.Objects)
            {
                ScenicMovementData scenicMovementData = HandleMovementData(p);
                objData.Add(scenicMovementData);
            }
            return objData;
        }
        private ScenicMovementData HandleMovementData(Object data)
        {
            Vector3 pos = ListToVector(data.Position);
            string modelType = data.Model.ModelType;
            // TODO: Initialize any other necessary movement data here (and add the field in the return statement)
            // Debug.LogError("handling movement data"); 
            // --------------------
            if (data.ActionDict.Count > 0)
            {
                string actionFunc = data.ActionDict.First().Key;
                ActionDictType actionValues = data.ActionDict.First().Value;

                Type classType = Type.GetType("Rehab.Scripts.Scenic.ActionAPI");
                if (classType.GetMethod(actionFunc) == null)
                {
                    Debug.LogError("Given action function does not exist.");
                }
                else
                {
                    MethodBase method = classType.GetMethod(actionFunc);
                    ParameterInfo[] parameters = method.GetParameters();
                
                    List<object> actionArgs = new List<object>();
                    int vector3Index = 0;
                    int boolIndex = 0;
                    int floatIndex = 0;
                    int intIndex = 0;
                    int stringIndex = 0;

                    foreach (ParameterInfo param in parameters)
                    {
                        if (param.ParameterType == typeof(Vector3))
                        {
                            Vector3 val;
                            // do not want to error here and rather add a null item as some values may have defaults 
                            if (actionValues.TupleVals.Count - vector3Index > 0 &&
                                vector3Index < parameters.Count(p => p.ParameterType == typeof(Vector3)))
                            {
                                val = ListToVector(actionValues.TupleVals[vector3Index]);
                                actionArgs.Add(val);
                            }
                            else
                            {
                                actionArgs.Add(null);
                            }
                            vector3Index++;
                        } else if (param.ParameterType == typeof(bool))
                        {
                            bool val;
                            if (actionValues.BoolVals.Count - boolIndex > 0 &&
                                boolIndex < parameters.Count(p => p.ParameterType == typeof(bool)))
                            {
                                val = actionValues.BoolVals[boolIndex];
                                actionArgs.Add(val);
                            }
                            else
                            {
                                actionArgs.Add(null);
                            }
                            boolIndex++;
                        }
                        else if (param.ParameterType == typeof(string))
                        {
                            string val;
                            if (actionValues.StringVals.Count - stringIndex > 0 &&
                                stringIndex < parameters.Count(p => p.ParameterType == typeof(string)))
                            {
                                val = actionValues.StringVals[stringIndex];
                                actionArgs.Add(val);
                            }
                            else
                            {
                                actionArgs.Add(null);
                            }
                        
                            stringIndex++;
                        }
                        else if (param.ParameterType == typeof(float))
                        {
                            float val;
                            if (actionValues.FloatVals.Count - floatIndex > 0 &&
                                floatIndex < parameters.Count(p => p.ParameterType == typeof(float)))
                            {
                                val = actionValues.FloatVals[floatIndex];
                                actionArgs.Add(val);
                            }
                            else
                            {
                                actionArgs.Add(null);
                            }
                            floatIndex++;
                        }
                    }
                    return new ScenicMovementData(pos, modelType, actionFunc, actionArgs);
                }
                return new ScenicMovementData(pos, modelType);
            }

            return new ScenicMovementData(pos, modelType);
        }
        public void HandleControl(ScenicJson data)
        {
            if (data.Control)
            {
                if (data.AddObject && data.SpawnObjectQueue.Count != 0)
                {
                    foreach (Object p in data.SpawnObjectQueue)
                    {
                        Vector3 v = ListToVector(p.Position);
                        Quaternion rot = ListToQuaternion(p.Rotation);
                        //Scenic uses right hand coord system so have to flip?
                        rot.y = -rot.y;
                        rot.x = -rot.x;
                        rot.z = -rot.z;
                        string tag = p.Model.ModelType;
                        Debug.Log("Scenic Model Type name: " + p.Model.ModelType);
                        float width = p.Model.Width;
                        float height = p.Model.Height;
                        float length = p.Model.Length;

                        InstantiateScenicObject instObj = new InstantiateScenicObject(v, rot, width, length, height, tag);
                    }
                }
                else if (data.Destroy)
                {
                    GameObject manager = GameObject.FindGameObjectWithTag("ScenicManager");
                    ObjectsList objectsList = manager.GetComponent<ObjectsList>();

                    if (objectsList != null)
                    {
                        objectsList.Reset();
                        ZmqServer server = manager.GetComponent<ZmqServer>();
                        server.ResetTickServerRpc();
                    }
                }
            }
        }

        // Helper Functions

        private Vector3 ListToVector(List<float> v)
        {
            /*  
        v[0] = x
        v[1] = y
        v[2] = z
        NOTE: We swap y and z because scenic uses z as vertical axis
        */
            return new Vector3(v[0], v[2], v[1]);
        }
        private Quaternion ListToQuaternion(List<float> q)
        {
            /*
        q[0] = x
        q[1] = y
        q[2] = z
        q[3] = w
        NOTE: We switch y and z
        */
            return new Quaternion(q[0], q[2], q[1], q[3]);
        }
    

        //Json deparsing stuff here 

        public partial class ScenicJson
        {
            [JsonProperty("control")]
            public bool Control { get; set; }

            [JsonProperty("addObject")]
            public bool AddObject { get; set; }
            [JsonProperty("timestepNumber")]
            public int TimestepNumber { get; set; }
        
            [JsonProperty("destroy")]
            public bool Destroy{ get; set; }
            [JsonProperty("objects")]
            public List<Object> Objects { get; set; }

            [JsonProperty("spawnQueue")]
            public List<Object> SpawnObjectQueue { get; set; }

            [JsonProperty("actionDict")]
            public Dictionary<string, string> ActionDictType { get; set; }
        }
        public partial class Model
        {
            [JsonProperty("length")]
            public float Length { get; set; }
            [JsonProperty("width")]
            public float Width { get; set; }
            [JsonProperty("height")]
            public float Height { get; set; }

            [JsonProperty("type")]

            public string ModelType { get; set; }
            [JsonProperty("color")]
            public List<float> color { get; set; }
        }

        // TODO: add any necessary classes

        public partial class Object
        {
            [JsonProperty("model")]
            public Model Model { get; set; }
            [JsonProperty("position")]
            public List<float> Position { get; set; }

            [JsonProperty("rotation")]
            public List<float> Rotation { get; set; }

            [JsonProperty("velocity")]
            public List<float> Velocity { get; set; }

            [JsonProperty("angularVelocity")]
            public List<float> AngularVelocity { get; set; }

            [JsonProperty("speed")]
            public float Speed { get; set; }

            [JsonProperty("velocityStop")]
            public bool VelocityStop { get; set; }

            [JsonProperty("actionDict")]
            public Dictionary<string, ActionDictType> ActionDict { get; set; }
        
            [JsonProperty("destroy")]
            public bool Destroy { get; set; }

            // TODO: add any variables to the Player Class if neccessary. Should match the properties of the class gameObject in client.py
            // They will be read by the HandleMovementData above to populate the ScenicMovementData
        }
    }

    public partial class ActionDictType
    {
        [JsonProperty("intVals")]
        public List<int> IntVals{ get; set; }
        [JsonProperty("floatVals")]
        public List<float> FloatVals{ get; set; }
        [JsonProperty("stringVals")]
        public List<string> StringVals{ get; set; }
        [JsonProperty("tupleVals")]
        public List<List<float>> TupleVals{ get; set; }
        [JsonProperty("boolVals")]
        public List<bool> BoolVals{ get; set; }
    }
}