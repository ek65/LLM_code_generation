using UnityEngine;

namespace Network
{
    public class LobbyManager : MonoBehaviour
    {
        public bool isClient;
        private void Awake()
        {            
#if UNITY_ANDROID || UNITY_EDITOR// Patient
                Debug.Log("Android");
                isClient = false;
#elif UNITY_STANDALONE_WIN || UNITY_WEBGL || UNITY_STANDALONE_OSX   // Clinician
                Debug.Log("Unity windows");
                isClient = true;
#endif
        }
    }
}
