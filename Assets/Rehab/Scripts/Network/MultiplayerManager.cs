using UnityEngine;
using UnityEngine.Serialization;

namespace Network
{
    /// <summary>
    /// Manages multiplayer functionality and scene setup for client/server architecture
    /// </summary>
    public class MultiplayerManager : MonoBehaviour
    {
        [FormerlySerializedAs("IsClient")] public bool isClient;
        [FormerlySerializedAs("IsServer")] public bool isServer;
        [SerializeField] private GameObject _mixedRealityHandlers;
        [SerializeField] private SceneMeshHandler _sceneMeshHandler;

        private void Awake()
        {
            // Set client/server roles based on platform
#if UNITY_ANDROID || UNITY_EDITOR// Patient
            isClient = false;
#elif UNITY_STANDALONE_WIN || UNITY_WEBGL || UNITY_STANDALONE_OSX  // Clinician
            isClient = true;
#endif
            isServer = !isClient;
        }

        private void Start()
        {
            SetupScene();
        }

        private void SetupScene()
        {
            // Configure scene based on role (server/patient vs client/clinician)
            if (isServer && !isClient)
            {
                Debug.Log("server scene setup");
                if (_mixedRealityHandlers != null)
                    _mixedRealityHandlers.SetActive(true);
            }
            else if (!isServer && isClient)
            {
                Debug.Log("client scene setup");
                if (_mixedRealityHandlers != null)
                    _mixedRealityHandlers.SetActive(false);

                Invoke(nameof(LoadRoom), 1f);
            }
        }

        private void LoadRoom()
        {
            // Request scene mesh data for client visualization
            _sceneMeshHandler.GetSceneModelMesh();
        }
    }
}
