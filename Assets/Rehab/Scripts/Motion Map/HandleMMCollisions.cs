using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HandleMMCollisions : MonoBehaviour
{
     private GameObject cubePatient;
     private GameObject mmm;
     private MotionMap mm;
     private float sphereSize;
     private float cubeSize;
     private float maxLength;
     
     private BoxCollider bc;
     private Bounds bounds;

     private Material visualMat, invisibleMat, voxelMat;
     
     private float frames;

     private Dictionary<GameObject, GameObject> voxelVisuals;
     private Dictionary<GameObject, Vector3> voxelVisualPoints;

     void Start()
     {
          cubePatient = GameObject.Find("Cube Patient");
          if (cubePatient == null) return;
          mm = cubePatient.GetComponentInChildren<MotionMap>();
          if (mm == null) return;
          mmm = GameObject.Find("MotionMapManager");
          if (mmm == null)
          {
               return;
          }
          sphereSize = mm.GetSphereSize();
          cubeSize = mm.GetCubeSize();
          maxLength = mm.GetMaxLength();

          visualMat = mm.GetVisualMat();
          invisibleMat = mm.GetInvisibleMat();
          voxelMat = mm.GetVoxelMat();

          bc = GetComponent<BoxCollider>();
          bounds = bc.bounds;

          voxelVisuals = new Dictionary<GameObject, GameObject>();
          voxelVisualPoints = new Dictionary<GameObject, Vector3>();
     }

     void Update()
     {
          if (mm == null) return;
          bool visReady = mm.GetVisReady();
          if (voxelVisuals == null) return;
          foreach (GameObject visual in voxelVisuals.Values)
          {
               if (visual != null)
               {
                    Renderer visualRenderer = visual.GetComponent<Renderer>();
                    visualRenderer.material = visReady ? visualMat : invisibleMat;
               }
          }

          bool voxelVis = mm.GetVoxelVis();
          foreach (Transform child in cubePatient.transform)
          {
               child.GetComponent<Renderer>().material = voxelVis ? voxelMat : invisibleMat;
          }

          /*
          if (!mm.GetFixVisuals() && frames % 10 == 0) 
          {
               foreach (Transform child in cubePatient.transform)
               {
                    GameObject voxel = child.gameObject;
                    if (ShouldProject(voxel))
                    {
                         VisualizeCollision(voxel, Vector3.zero, false, true);
                    }
                    else if (ShouldReproject(voxel))
                    {
                         VisualizeCollision(voxel, Vector3.zero, true, true);
                    }
               }
          }
          */
          
          maxLength = mm.GetMaxLength();

          /*
          foreach (Transform child in cubePatient.transform)
          {
               GameObject voxel = child.gameObject;
               if (name.ToLower().Contains("storage") && (!WithinXZBounds(voxel) || !WithinMaxLength(voxel)))
               {
                    if (voxelVisuals.Keys.Contains(voxel))
                    {
                         Destroy(voxelVisuals[voxel]);
                         voxelVisualPoints[voxel] = Vector3.zero;
                    }
               }
          }
          */
          
          frames++;
     }
     

     public void OnTriggerStay(Collider other1)
     {
          if (mm == null) return;
          if (other1 == null) return;
          if (!mm.GetFixVisuals() && frames % 10 == 0)
          {
               Vector3 contactPoint = other1.ClosestPoint(transform.position);
               GameObject other = other1.gameObject;

               if (ShouldVisualize(other) & frames > 1)
               {
                    VisualizeCollision(other, contactPoint, false, false);
                    /*Debug.Log("COLLISION ENTERED");*/
               }
               else if (ShouldRevisualize(other, contactPoint) & frames > 1)
               {
                    VisualizeCollision(other, contactPoint, true, false);
               }
               /*foreach (ContactPoint contact in other1.contacts)
               {
                    Vector3 contactPoint = contact.point;
                    GameObject other = other1.gameObject;
                    if (ShouldVisualize(other) & frames > 1)
                    {
                         VisualizeCollision(other, contactPoint, false, false);
                         Debug.Log("COLLISION ENTERED");
                    }
                    else if (ShouldRevisualize(other, contactPoint) & frames > 1)
                    {
                         VisualizeCollision(other, contactPoint, true, false);
                    }
               }*/
          }
     }

     public void OnTriggerExit(Collider other1)
     {
          if (mm == null) return;
          if (other1 == null) return;
          if (!mm.GetFixVisuals())
          {
               GameObject other = other1.gameObject;
               if (voxelVisuals.Keys.Contains(other))
               {
                    // clear mappings
                    Destroy(voxelVisuals[other]);
                    voxelVisualPoints[other] = Vector3.zero;
               }

               /*Debug.Log("COLLISION EXITED");*/
          }
     }

     private bool ShouldVisualize(GameObject other)
     {
          if (voxelVisuals.Keys.Contains(other) && voxelVisuals[other] != null)
          {
               return false;
          }
          if (!other.tag.Equals("Voxel"))
          {
               return false;
          }
          /*
          if (!other.tag.Equals("Voxel") || !name.ToLower().Contains("table"))
          {
               return false;
          }
          */
          return true;
     }

     private bool ShouldRevisualize(GameObject other, Vector3 collisionPoint)
     {
          /*
          if (!name.ToLower().Contains("table"))
          {
               return false;
          }
          */
          if (voxelVisuals.Keys.Contains(other) && voxelVisuals[other] != null &&
              voxelVisualPoints[other] != collisionPoint)
          {
               return true;
          }

          return false;
     }

     private bool ShouldProject(GameObject voxel)
     {
          if (voxelVisuals.Keys.Contains(voxel) && voxelVisuals[voxel] != null)
          {
               return false;
          }
          if (name.ToLower().Contains("storage"))
          {
               Vector3 pos = voxel.transform.position;
               if (WithinXZBounds(voxel) && pos.y >= bounds.max.y && WithinMaxLength(voxel))
               {
                    return true;
               }
          }
          return false;
     }

     private bool ShouldReproject(GameObject voxel)
     {
          Vector3 pos = voxel.transform.position;
          if (!name.ToLower().Contains("storage"))
          {
               return false;
          }
          if (voxelVisuals.Keys.Contains(voxel) && voxelVisuals[voxel] != null &&
              voxelVisualPoints[voxel] != pos && WithinMaxLength(voxel))
          {
               return true;
          }

          return false;
     }
     

     private void VisualizeCollision(GameObject voxel, Vector3 collisionPoint, bool revis, bool down)
     {
          GameObject visual;
          if (!revis)
          {
               visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
               visual.transform.SetParent(mmm.transform);
               visual.name = "Visual";
               visual.layer = 6;
          }
          else
          {
               visual = voxelVisuals[voxel];
          }

          Transform visTrans = visual.transform;
          Transform voxTrans = voxel.transform;
          Vector3 contactBound = bc.ClosestPointOnBounds(collisionPoint);

          if (down)
          {
               visTrans.position = new Vector3(voxTrans.position.x, bounds.max.y, voxTrans.position.z);
               visTrans.rotation = Quaternion.Euler(new Vector3(0, voxTrans.eulerAngles.y, 0));
          }
          else
          {
               visTrans.position = new Vector3(contactBound.x, bc.bounds.max.y, contactBound.z);
               visTrans.rotation = Quaternion.Euler(new Vector3(0, voxel.transform.eulerAngles.y, 0));
          }
          
          visTrans.localScale = new Vector3(1, 0, 1) * cubeSize;
          Renderer visualRenderer = visual.GetComponent<Renderer>();
          visualRenderer.material = mm.GetVisReady() ? visualMat : invisibleMat;

          if (voxelVisuals.Keys.Contains(voxel))
          {
               voxelVisuals[voxel] = visual;
               voxelVisualPoints[voxel] = collisionPoint;
          }
          else if (revis)
          {
               voxelVisualPoints[voxel] = collisionPoint;
          }
          else
          {
               voxelVisuals.Add(voxel, visual);
               voxelVisualPoints.Add(voxel, collisionPoint);
          }
     }

     private bool WithinXZBounds(GameObject voxel)
     {
          Vector3 pos = voxel.transform.position;
          return pos.x >= bounds.min.x && pos.x <= bounds.max.x && pos.z >= bounds.min.z && pos.z <= bounds.max.z;
     }

     private bool WithinMaxLength(GameObject voxel)
     {
          Vector3 pos = voxel.transform.position;
          return Vector3.Distance(new Vector3(pos.x, bounds.max.y, pos.z), mm.GetSpineUpperPos()) < maxLength;
     }
}