using UnityEngine;

namespace Rehab.Scripts.Scenic
{
    /// <summary>
    /// Handles the creation and management of scenic objects in the rehabilitation environment
    /// Supports both player avatars and environmental objects
    /// </summary>
    public class InstantiateScenicObject
    {
        private ObjectsList objectList;

        /// <summary>
        /// Creates a new scenic object with specified dimensions and position
        /// </summary>
        /// <param name="pos">Position in 3D space</param>
        /// <param name="rot">Rotation quaternion</param>
        /// <param name="width">Object width</param>
        /// <param name="length">Object length</param>
        /// <param name="height">Object height</param>
        /// <param name="tag">Identifier for the object type</param>
        public InstantiateScenicObject(Vector3 pos, Quaternion rot, float width, float length, float height, string tag)
        {
            objectList = GameObject.FindGameObjectWithTag("ScenicManager").GetComponent<ObjectsList>();
            AddScenicObject(pos, rot, width, length, height, tag);
        }

        /// <summary>
        /// Instantiates and registers a new scenic object in the environment
        /// </summary>
        private void AddScenicObject(Vector3 pos, Quaternion rot, float width, float length, float height, string tag)
        {
            if (objectList.modelList.TryGetValue(tag, out var value))
            {
                // Generate unique name for the instance
                string objectName = value.name;
                if (objectList.objectCount.ContainsKey(value.name))
                {
                    objectList.objectCount[value.name]++;
                    objectName = objectName + objectList.objectCount[value.name].ToString();
                }   
                
                // Create instance with position and rotation
                GameObject scenicInstance = Object.Instantiate(value, new Vector3(pos.x, pos.z, pos.y), rot);
                scenicInstance.name = objectName;
           
                // Register instance in appropriate collection based on type
                switch (scenicInstance.tag)
                {
                    case "ScenicAvatar":
                        objectList.scenicPlayers.Add(scenicInstance);
                        break;
                    case "ScenicObject":
                        objectList.scenicObjects.Add(scenicInstance);
                        break;
                }
            }
            else
            {
                Debug.Log("InstantiateScenicObject: The tag: " + tag + " does not exist.");
            }
        }
    }
}









