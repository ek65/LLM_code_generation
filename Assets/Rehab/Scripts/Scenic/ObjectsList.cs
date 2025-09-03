using System.Collections.Generic;
using UnityEngine;

namespace Rehab.Scripts.Scenic
{
    /// <summary>
    /// Manages the collection and lifecycle of all objects in the rehabilitation scene
    /// Handles object instantiation, tracking, and cleanup
    /// </summary>
    public class ObjectsList : MonoBehaviour
    {
        // Scene object collections
        public List<GameObject> scenicPlayers;    // Active player avatars
        public List<GameObject> scenicObjects;    // Interactive environment objects
        public Dictionary<string, GameObject> modelList;  // Available object prefabs
        public Dictionary<string, int> objectCount;      // Instance count per object type

        void Start()
        {
            // Initialize collections
            scenicPlayers = new List<GameObject>();
            scenicObjects = new List<GameObject>();
            modelList = new Dictionary<string, GameObject>();
            objectCount = new Dictionary<string, int>();
            InitModelDict();
        }

        /// <summary>
        /// Loads all available object prefabs from Resources/ScenicObjects
        /// </summary>
        private void InitModelDict()
        {
            List<GameObject> models = new List<GameObject>();
            models.AddRange(Resources.LoadAll<GameObject>("ScenicObjects"));
            foreach (GameObject obj in models)
            {
                modelList.Add(obj.name, obj);
            }

            InitCountDict();
        }

        /// <summary>
        /// Initializes the counter dictionary for tracking object instances
        /// </summary>
        public void InitCountDict()
        {
            objectCount = new Dictionary<string, int>();
            foreach (KeyValuePair<string, GameObject> keyValuePair in modelList)
            {
                string key = keyValuePair.Key;
                objectCount.Add(key, 0);
            }
        }
    
        /// <summary>
        /// Cleans up all scene objects and resets collections
        /// </summary>
        public void Reset()
        {
            // Destroy all active objects
            foreach (GameObject player in scenicPlayers)
                Destroy(player);
            foreach (GameObject obj in scenicObjects)
                Destroy(obj);

            // Reset collections
            scenicPlayers = new List<GameObject>();
            scenicObjects = new List<GameObject>();
            InitCountDict();
        }
    }
}
