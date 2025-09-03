using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Meta.XR.MRUtilityKit;
using Newtonsoft.Json;
using Photon.Pun;
using Vector3 = UnityEngine.Vector3;

public class MotionMap : MonoBehaviour
{

    // GAME OBJECTS (-esque)
    public Transform cubePatient;
    private GameObject MRRoom;
    private GameObject mmm;
    private List<GameObject> meshes;

    // VOXELIZATION
    public List<Vector3> leftRelativeData, rightRelativeData, pointSpheres;
    private List<Vector3> voxels;
    private float sphereSize = 0.25f;
    private float cubeSize = 0.7f;
    private float scalingFactor = 0.05f;

    // COUNTERS & TRACKERS
    private int frames;
    private bool MRfound;
    private string strokeSide = "right";
    private string healthySide = "left";
    public float maxLength;

    // BODY TRACKING
    public Transform spineUpper, leftMiddleMCP, rightMiddleMCP;
    public List<Vector3> spineUpperData, leftMiddleMCPData, rightMiddleMCPData;

    // SERIALIZED FIELDS
    [SerializeField] private Material voxelMat, visualMat, invisibleMat;
    [SerializeField] public bool voxelVis;
    [SerializeField] public bool visualizeReady;
    [SerializeField] public bool fixVisuals;

    void Start()
    {
        cubePatient = GameObject.Find("Cube Patient").transform;
        MRRoom = null;
        mmm = GameObject.Find("MotionMapManager");
        meshes = new List<GameObject>();

        leftRelativeData = new List<Vector3>();
        rightRelativeData = new List<Vector3>();

        spineUpper = GameObject.Find("UpperChest")?.transform;
        leftMiddleMCP = GameObject.Find("Left_MiddleProximal")?.transform;
        rightMiddleMCP = GameObject.Find("Right_MiddleProximal")?.transform;

        voxels = new List<Vector3>();
        sphereSize *= scalingFactor;
        cubeSize *= scalingFactor;

        spineUpperData = new List<Vector3>();
        leftMiddleMCPData = new List<Vector3>();
        rightMiddleMCPData = new List<Vector3>();
        
        SetSides("left");
        ReadInData();
        Done();
        Show();
    }

    public void Update()
    {
        spineUpper = GameObject.Find("UpperChest")?.transform;
        leftMiddleMCP = GameObject.Find("Left_MiddleProximal")?.transform;
        rightMiddleMCP = GameObject.Find("Right_MiddleProximal")?.transform;

        if (MRRoom == null)
        {
            MRRoom = FindObjectOfType<MRUKRoom>()?.gameObject;
            if (MRRoom != null)
            {
                Debug.Log("ROOM NAME: " + MRRoom.name);
                SetUpObjectCollision();
            }
        }

        if (spineUpper != null)
        {
            cubePatient.position = spineUpper.position;
            cubePatient.rotation = spineUpper.rotation;
        }

        frames++;
    }

    private void SetUpObjectCollision()
    {
        if (MRRoom != null)
        {
            var i = 0;
            String[] objectNames = new String[]
                { "TABLE", "SCREEN", "WINDOW", "DOOR", "SHELF", "STORAGE" };
            foreach (Transform child in MRRoom.transform)
            {
                if (objectNames.Contains(child.name))
                {
                    Transform ch = child.Find(child.name + "_EffectMesh");
                    if (ch)
                    {
                        GameObject mesh = ch.gameObject;
                        if (mesh != null)
                        {
                            mesh.AddComponent<HandleMMCollisions>();
                            mesh.AddComponent<Rigidbody>();
                            Rigidbody rb = mesh.GetComponent<Rigidbody>();

                            meshes.Add(mesh);

                            rb.useGravity = false;
                            rb.constraints = RigidbodyConstraints.FreezeAll;
                        }
                    }
                }

                i++;
            }
        }

        Debug.Log("MM SUBROUTINE: SETUP COMPLETE");
    }
    

    private void GenerateVoxels()
    {
        pointSpheres = strokeSide == "right" ? rightRelativeData : leftRelativeData;

        List<float> xs = new List<float>();
        List<float> ys = new List<float>();
        List<float> zs = new List<float>();

        foreach (Vector3 v in pointSpheres)
        {
            xs.Add(v.x);
            ys.Add(v.y);
            zs.Add(v.z);
        }

        // Not a task
        if (xs.Count == 0 || ys.Count == 0 || zs.Count == 0)
        {
            return;
        }

        float xMin = xs.Min();
        float xMax = xs.Max();

        float yMin = ys.Min();
        float yMax = ys.Max();

        float zMin = zs.Min();
        float zMax = zs.Max();

        double dist = cubeSize * Math.Sqrt(3) / 2;

        float zPos = zMin;
        while (zPos < zMax)
        {
            float yPos = yMin;
            while (yPos < yMax)
            {
                float xPos = xMin;
                while (xPos < xMax)
                {
                    Vector3 pos = new Vector3(xPos, yPos, zPos);
                    foreach (Vector3 pt in pointSpheres)
                    {
                        if (Vector3.Distance(pos, pt) <= dist && !voxels.Contains(pos))
                        {
                            bool tooClose = false;

                            foreach (Vector3 vox in voxels)
                            {
                                if (Vector3.Distance(pt, vox) <= dist)
                                {
                                    tooClose = true;
                                    break;
                                }
                            }
                            
                            if (!tooClose)
                            {
                                voxels.Add(pos);
                            }
                        }
                    }

                    xPos += cubeSize;
                }

                yPos += cubeSize;
            }

            zPos += cubeSize;
        }

        Debug.Log("MM SUBROUTINE: VOXELS GENERATED");
    }

    private void CreateVoxels()
    {
        for (int i = 0; i < voxels.Count; i++)
        {
            GameObject voxel = GameObject.CreatePrimitive(PrimitiveType.Cube);
            voxel.GetComponent<BoxCollider>().isTrigger = true;
            voxel.transform.SetParent(cubePatient);
            voxel.transform.localPosition = voxels[i];
            voxel.transform.localScale = new Vector3(1, 1, 1) * cubeSize;
            voxel.name = "voxel" + (i + 1);
            voxel.GetComponent<Renderer>().material = voxelMat;
            voxel.tag = "Voxel";
            voxel.layer = 6;
        }

        Debug.Log("MM SUBROUTINE: VOXELS CREATED");
    }

    private void ReadInData()
    {
        int i = 0;
        while (i < 30)
        {
            string path = $"MotionMap/Relative/Relative{i}_0";
            TextAsset jsonFile = Resources.Load<TextAsset>(path);

            if (jsonFile == null)
            {
                Debug.Log($"File not found: {path}");
                break;
            }

            try
            {
                RelativeTrajectoryData relative = JsonConvert.DeserializeObject<RelativeTrajectoryData>(jsonFile.text);
                leftRelativeData.AddRange(relative.leftRelative);
                rightRelativeData.AddRange(relative.rightRelative);
                i++;
            }
            catch (JsonException ex)
            {
                Debug.LogError($"JSON deserialization error: {ex.Message}");
                break;
            }
        }

        List<Vector3> data = strokeSide == "right" ? rightRelativeData : leftRelativeData;
        foreach (Vector3 p in data)
        {
            float dist = Vector3.Distance(p, Vector3.zero);
            if (dist > maxLength)
            {
                maxLength = dist;
            }
        }
    }
    

    [PunRPC]
    public void Done()
    {
        if (cubePatient.childCount == 0)
        {
            GenerateVoxels();
            CreateVoxels();
            Debug.Log("MM CMD: DONE");
        }
    }

    [PunRPC]
    public void DestroyVoxels()
    {
        foreach (Transform child in transform)
        {
            PhotonNetwork.Destroy(child.gameObject);
        }

        Debug.Log("MM CMD: DESTROYED");
    }
    
    
    [PunRPC]
    public void Show()
    {
        visualizeReady = true;
        Debug.Log("MM CMD: SHOWED");
    }

    [PunRPC]
    public void Hide()
    {
        visualizeReady = false;
        Debug.Log("MM CMD: HIDDEN");
    }

    [PunRPC]
    public void StopMap()
    {
        fixVisuals = !fixVisuals;
        GetComponent<PhotonView>().RPC("SetKinematics", RpcTarget.All, fixVisuals);
        Debug.Log("MM CMD: STOPPED");
    }

    [PunRPC]
    public void Wipe()
    {
        fixVisuals = false;
        visualizeReady = false;
        DestroyVoxels();
        foreach (Transform child in mmm.transform)
        {
            Destroy(child.gameObject);
        }
        Debug.Log("MM CMD: WIPED");
    }
    
    [PunRPC]
    public void ToggleVoxelVis(bool x = true)
    {
        voxelVis = x ? !voxelVis : false;
    }
    
    

    // SETTERS
    
    [PunRPC]
    private void SetKinematics(bool on)
    {
        foreach (GameObject mesh in meshes)
        {
            mesh.GetComponent<Rigidbody>().isKinematic = on;
        }
    }

    public void SetSides(string stroke)
    {
        if (stroke.ToLower() == "right")
        {
            strokeSide = "right";
            healthySide = "left";
        }
        else
        {
            strokeSide = "left";
            healthySide = "right";
        }
    }

    // GETTERS

    public float GetSphereSize()
    {
        return sphereSize;
    }

    public float GetCubeSize()
    {
        return cubeSize;
    }

    public Material GetVisualMat()
    {
        return visualMat;
    }

    public Material GetInvisibleMat()
    {
        return invisibleMat;
    }

    public Material GetVoxelMat()
    {
        return voxelMat;
    }

    public bool GetVisReady()
    {
        return visualizeReady;
    }

    public bool GetFixVisuals()
    {
        return fixVisuals;
    }

    public bool GetVoxelVis()
    {
        return voxelVis;
    }

    public float GetMaxLength()
    {
        return maxLength;
    }

    public Vector3 GetSpineUpperPos()
    {
        return spineUpper.position;
    }
}


[Serializable]
public class RelativeTrajectoryData
{
    public List<Vector3> leftRelative, rightRelative;

    public RelativeTrajectoryData(List<Vector3> leftRelative, List<Vector3> rightRelative)
    {
        this.leftRelative = leftRelative;
        this.rightRelative = rightRelative;
    }
}