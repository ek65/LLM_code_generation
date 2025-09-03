using Meta.XR.MRUtilityKit;
using Photon.Pun;
using UnityEngine;

namespace Network
{
    public class SceneMeshHandler : MonoBehaviour
    {
        private PhotonView photonView;
        public Material invisibleMaterial;
        void Start()
        {
            photonView = GetComponent<PhotonView>();
        }

        public void GetSceneModelMesh()
        {
            photonView.RPC(nameof(RequestSceneDataFromServer), RpcTarget.MasterClient);
        }

        [PunRPC]
        private void RequestSceneDataFromServer()
        {
            GetRoomData();
        }

        private void GetRoomData()
        {
            foreach (MRUKRoom room in MRUK.Instance.Rooms)
            {
                if (room != null)
                {
                    GameObject currentRoom = room.gameObject;

                    GameObject roomParent = PhotonNetwork.Instantiate("Prefabs/res_EMPTY", currentRoom.transform.position, Quaternion.Euler(currentRoom.transform.rotation.eulerAngles));
                    roomParent.name = "res_EMPTY_" + currentRoom.name;

                    foreach (Transform child in currentRoom.transform)
                    {
                        GameObject goParent1 = PhotonNetwork.Instantiate("Prefabs/res_EMPTY", child.localPosition, Quaternion.Euler(child.localRotation.eulerAngles));
                        goParent1.transform.SetParent(roomParent.transform);
                        goParent1.transform.localScale = child.localScale;
                        goParent1.name = "res_EMPTY_" + child.name;

                        MRUKAnchor anchorInfo = child.GetComponent<MRUKAnchor>();

                        if (anchorInfo.VolumeBounds.HasValue)
                        {
                            GameObject go = PhotonNetwork.Instantiate("Prefabs/res_OTHER_effect", child.GetChild(0).position, Quaternion.Euler(child.GetChild(0).rotation.eulerAngles));
                            go.transform.SetParent(goParent1.transform);
                            go.transform.localScale = child.GetChild(0).localScale;
                            go.name = "res_OTHER_effect" + child.name;
                        }
                        else if (anchorInfo.PlaneRect.HasValue)
                        {
                            if (child.childCount > 0)
                            {
                                GameObject go = PhotonNetwork.Instantiate("Prefabs/res_WALL_effect", child.GetChild(0).position, Quaternion.Euler(child.GetChild(0).rotation.eulerAngles));
                                go.transform.SetParent(goParent1.transform);
                                go.transform.localScale = child.GetChild(0).localScale;
                                go.name = "res_WALL_effect" + child.name;
                            }
                        }

                        /*if (child.childCount > 0)
                        {
                            // hiding non-networked mesh 
                            child.GetChild(0).gameObject.SetActive(false);
                        }*/
                    }

                    if (roomParent == null)
                    {
                        return;
                    } 
                    foreach (GameObject networkedRoom in roomParent.transform)
                    {
                        if (networkedRoom)
                        {
                            Renderer[] renderers = networkedRoom.GetComponentsInChildren<Renderer>();

                            foreach (Renderer rend in renderers)
                            {
                                Material[] mats = rend.materials;
                                for (int i = 0; i < mats.Length; i++)
                                {
                                    mats[i] = invisibleMaterial;
                                }
                                rend.materials = mats;
                            }
                        }
                    }
                    break;
                }
            }
            
            
        }

        #region Global Detailed SceneMesh Logic

        /*

        private const int ChunkSize = 5000;
        private Dictionary<int, string> receivedChunks = new Dictionary<int, string>();
        private GameObject globalSceneMeshObject;

        [SerializeField] private Material sceneModelMaterial;

        [System.Serializable]
        public class MeshData
        {
            public Vector3[] vertices;
            public int[] triangles;

            public MeshData(Mesh mesh)
            {
                vertices = mesh.vertices;
                triangles = mesh.triangles;
            }
        }

        

        [PunRPC]
        private void RequestGlobalSceneMeshFromServer()
        {
            if (photonView.IsMine)
            {
                foreach (MRUKRoom room in MRUK.Instance.Rooms)
                {
                    Debug.Log($"Room Name: {room.name}");

                    foreach (MRUKAnchor childAnchor in room.Anchors)
                    {
                        Mesh sceneMesh = childAnchor.LoadGlobalMeshTriangles();
                        if (sceneMesh != null)
                        {
                            Debug.Log($"Vertex Count: {sceneMesh.vertexCount}");
                            Debug.Log($"Triangle Count: {sceneMesh.triangles.Length / 3}"); // Each triangle is represented by 3 indices
                            string jsonMeshData = JsonUtility.ToJson(new MeshData(sceneMesh), true);
                            Debug.Log("JSON mesh data extracted on server side");
                            SendLargeData(Utility.CompressString(jsonMeshData));
                            break;
                        }
                    }
                }
            }
        }

        

        private void SendLargeData(string jsonMeshData)
        {
            int totalChunks = Mathf.CeilToInt((float)jsonMeshData.Length / ChunkSize);
            StartCoroutine(LoadChunks(jsonMeshData, totalChunks));
        }

        private IEnumerator LoadChunks(string jsonMeshData, int totalChunks)
        {
            Debug.Log("Total chunks : " + totalChunks);
            for (int i = 0; i < totalChunks; i++)
            {
                int start = i * ChunkSize;
                int length = Mathf.Min(ChunkSize, jsonMeshData.Length - start);
                string chunk = jsonMeshData.Substring(start, length);

                Debug.Log($"Sending chunk {i + 1}/{totalChunks} (Size: {chunk.Length} bytes)");

                // Send each chunk via RPC
                photonView.RPC("SendChunkToClient", RpcTarget.Others, chunk, i, totalChunks);
                yield return new WaitForSeconds(0.1f);
            }
        }


        [PunRPC]
        private void SendChunkToClient(string chunk, int chunkIndex, int totalChunks)
        {
            // Store the received chunk
            receivedChunks[chunkIndex] = chunk;

            Debug.Log("received chunks count : " + receivedChunks.Count);

            // Check if all chunks are received
            if (receivedChunks.Count == totalChunks)
            {
                // Reconstruct the full string
                string fullDataUncompressed = string.Join("", receivedChunks.Values);
                receivedChunks.Clear();
                string fullData = Utility.DecompressString(fullDataUncompressed);

                Debug.Log("Successfully reconstructed full data.");

                MeshData meshData = JsonUtility.FromJson<MeshData>(fullData);
                Mesh reconstructedMesh = new Mesh
                {
                    vertices = meshData.vertices,
                    triangles = meshData.triangles
                };

                Debug.Log($"Vertex Count: {reconstructedMesh.vertices.Length}");
                Debug.Log($"Triangle Count: {reconstructedMesh.triangles.Length / 3}");

                // Optionally, reconstruct the mesh here
    #if UNITY_STANDALONE_WIN || UNITY_WEBGL || UNITY_STANDALONE_OSX 
                Debug.Log("reconstructing mesh");
                ReconstructMesh(fullData);
    #endif
            }
        }

        private void ReconstructMesh(string jsonMeshData)
        {
            MeshData meshData = JsonUtility.FromJson<MeshData>(jsonMeshData);
            Mesh reconstructedMesh = new Mesh
            {
                vertices = meshData.vertices,
                triangles = meshData.triangles
            };

            GameObject meshObject = new GameObject("ReconstructedMesh");
            MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter.mesh = reconstructedMesh;
            meshRenderer.material = sceneModelMaterial;

            meshObject.transform.position = new Vector3(0, 2, 0);
            meshObject.transform.rotation = Quaternion.Euler(270, 90, 0);

            globalSceneMeshObject = meshObject;

            Debug.Log("Reconstructed mesh added to scene.");
        }

        */

        #endregion

    }
}