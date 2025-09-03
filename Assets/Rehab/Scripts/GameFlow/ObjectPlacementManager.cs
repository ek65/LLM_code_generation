using Network;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameFlow
{
    public class ObjectPlacementManager : MonoBehaviour
    {
        public GameObject blockPrefab;        // The block/cube you want to place
        public GameObject pointerSpherePrefab; // The small sphere pointer
        public Camera mainCamera;             // Reference to the main camera
        public LayerMask placeableLayer;      // Layer that can be clicked for placement

        private GameObject _pointerSphere;
        [SerializeField] private MultiplayerManager multiplayerManager;
        void Start()
        {
            // Do not want to place object in the Quest
            if (multiplayerManager == null || multiplayerManager.isServer)
            {
                return;
            }
            
            if (mainCamera == null)
                mainCamera = Camera.main;

            
            _pointerSphere = Instantiate(pointerSpherePrefab, Vector3.zero, Quaternion.identity);
            _pointerSphere.SetActive(false);
        }

        void Update()
        {
            if (multiplayerManager == null || multiplayerManager.isServer)
            {
                return; 
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (!(Cursor.lockState == CursorLockMode.Locked))
            {
                return;
            }

            // Cast a ray from the mouse position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placeableLayer))
            {
                // Calculate the placement position at the raycast hit
                Vector3 placementPosition = hit.point;
                placementPosition.y += 0.2f;
                
                // Move the pointer to the placement position
                _pointerSphere.transform.position = placementPosition;
                _pointerSphere.SetActive(true);

                // On left-click, instantiate a block at that position
                if (Input.GetMouseButtonDown(0))
                {
                    GameObject networkedObject = PhotonNetwork.Instantiate("Prefabs/TempPlaceObject", placementPosition, Quaternion.identity);
                    // Any local player in the room for now
                    networkedObject.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                }
            }
            else
            {
                // hide pointer if it's nothing
                _pointerSphere.SetActive(false);
            }
        }
    }
}
