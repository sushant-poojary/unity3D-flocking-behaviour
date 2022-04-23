using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Main : MonoBehaviour
{
    private const float BOUNDARY_BUFFER = 4;
    public const int interval = 4;
    public const int doubleInterval = interval * 2;
    private bool mStartMovement;
    public GameObject BirdPrefab;
    //[SerializeField]
    //private SpatialOctree mSpatialTree;

    [SerializeField]
    private BoidConfig mBoidConfig;
    private List<Boid> mTreeChildren;
    public int NumberOfBirds = 0;
    private OctTree<Boid> mSpaceTree;
    private int mBoidCount = 0;
    private OctTree<Boid>.OctNode topNode;
    private Bounds mSpaceBounds;
    private List<Boid> neighs;
    private List<Bounds> allRegions;

    [Range(-1, 1)]
    public float mFov = 0.5f;
    public float minDistance = 3f;
    public float awarenessRadius = 6;
    // Start is called before the first frame update
    void Start()
    {

        mSpaceTree = new OctTree<Boid>(transform.position, 64, 4);
        mBoidCount = NumberOfBirds;
        StartCoroutine(GenerateBirds(mBoidCount));

        //Debug.LogError((-19 > -20));
    }

    private IEnumerator GenerateBirds(int count)
    {
        mSpaceBounds = mSpaceTree.GetRootArea();
        mTreeChildren = new List<Boid>(count);
        var y = Random.Range(mSpaceBounds.min.y + BOUNDARY_BUFFER, mSpaceBounds.max.y - BOUNDARY_BUFFER);
        for (int i = 0; i < count; i++)
        {
            GameObject bird = GameObject.Instantiate<GameObject>(BirdPrefab);
            Boid boid = new Boid(bird, i.ToString(), mBoidConfig);
            mTreeChildren.Add(boid);
            //if (i == 0) mBoidConfig.Target = bird.transform;
            //var x = Random.Range(-1, 2);//Random.Range(mSpaceBounds.min.x + 2f, mSpaceBounds.max.x - 2f);
            //var y = Random.Range(-1, 2);//Random.Range(mSpaceBounds.min.y + 2f, mSpaceBounds.max.y - 2f);
            //var z = Random.Range(-1, 2);//Random.Range(mSpaceBounds.min.z + 2f, mSpaceBounds.max.z - 2f);

            var x = Random.Range(mSpaceBounds.min.x + BOUNDARY_BUFFER, mSpaceBounds.max.x - BOUNDARY_BUFFER);
            var z = Random.Range(mSpaceBounds.min.z + BOUNDARY_BUFFER, mSpaceBounds.max.z - BOUNDARY_BUFFER);
            Vector3 randomPosition = new Vector3(x, y, z);
            bird.transform.position = randomPosition;
            yield return new WaitForEndOfFrame();
        }
        List<OctTree<Boid>.OctNode> containers;
        mSpaceTree.BuildTree(mTreeChildren, out containers);
        foreach (OctTree<Boid>.OctNode item in containers)
        {
            AssignContainerNode(item);
        }
        allRegions = mSpaceTree.GetAllRegions();
        mTreeChildren.TrimExcess();
        mStartMovement = true;
    }

    private void AssignContainerNode(OctTree<Boid>.OctNode item)
    {
        foreach (var boid in mTreeChildren)
        {
            if (item.Contains(boid))
            {
                boid.ContainerNode = item;
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!mStartMovement) return;
        for (int i = 0; i < mBoidCount; i++)
        {
            Boid boid = mTreeChildren[i];
            neighs = mSpaceTree.FindNeighboringChildren(boid, boid.ContainerNode, 3);
            //neighs = mSpaceTree.DebugFindNeighboringChildren(boid, boid.ContainerNode, 2, out topNode);

            CalculateMovement(boid, ref neighs);
            //CalculateMovement(boid, ref mTreeChildren, ref cohensionCount, ref seperationCount, ref allignmentCount, ref alignment, ref seperation, ref cohension);

            //Debug.Log($"POS:{boid.Position}   mSpaceBounds:{ mSpaceBounds.min} and max{mSpaceBounds.max}");
            if (boid.Position.x < mSpaceBounds.min.x + BOUNDARY_BUFFER)
            {
                Vector3 currentPos = boid.Position;
                //Debug.Log("Change vel MIN");
                //Debug.Log($"POS:{boid.Position}   mSpaceBounds:{ mSpaceBounds.min} and max{mSpaceBounds.max}");
                currentPos.x = mSpaceBounds.max.x - BOUNDARY_BUFFER;
                boid.ChangePosition(currentPos);
            }
            else if (boid.Position.y < mSpaceBounds.min.y + BOUNDARY_BUFFER)
            {
                Vector3 currentPos = boid.Position;
                currentPos.y = mSpaceBounds.max.y - BOUNDARY_BUFFER;
                boid.ChangePosition(currentPos);
            }
            else if (boid.Position.z < mSpaceBounds.min.z + BOUNDARY_BUFFER)
            {
                Vector3 currentPos = boid.Position;
                currentPos.z = mSpaceBounds.max.z - BOUNDARY_BUFFER;
                boid.ChangePosition(currentPos);
            }
            else if (boid.Position.x > mSpaceBounds.max.x - BOUNDARY_BUFFER)
            {
                //Debug.Log("Change vel MAX");
                //Debug.Log($"POS:{boid.Position}   mSpaceBounds:{ mSpaceBounds.min} and max{mSpaceBounds.max}");
                //boid.ChangeVelocity(boid.GetCurrentVelocity() * -1);
                Vector3 currentPos = boid.Position;
                currentPos.x = mSpaceBounds.min.x + BOUNDARY_BUFFER;
                boid.ChangePosition(currentPos);
            }
            else if (boid.Position.y > mSpaceBounds.max.y - BOUNDARY_BUFFER)
            {
                Vector3 currentPos = boid.Position;
                currentPos.y = mSpaceBounds.min.y + BOUNDARY_BUFFER;
                boid.ChangePosition(currentPos);
            }
            else if (boid.Position.z > mSpaceBounds.max.z - BOUNDARY_BUFFER)
            {
                Vector3 currentPos = boid.Position;
                currentPos.z = mSpaceBounds.min.z + BOUNDARY_BUFFER;
                boid.ChangePosition(currentPos);
            }
            OctTree<Boid>.OctNode container;
            //Debug.Log("Boid to bounds " + boid.GetBounds());
            if (mSpaceTree.Update(boid, boid.ContainerNode, out container))
            {
                if (container == null)
                {
                    Debug.LogWarning("Container is null for boid at prev:" + boid.ContainerNode.GUID);
                }
                boid.ContainerNode = container;
                //Debug.Log("GOT IT!!!      [Update] container: " + container.NAME);
            }
            else
            {
                //Debug.LogError("Failed to update " + boid + " from node:" + boid.ContainerNode);
            }
        }//end for loop
        if (Time.frameCount % doubleInterval == 0)
        {
            mSpaceTree.Prune();
        }
        //allRegions = mSpaceTree.GetAllRegions();
    }

    private void CalculateMovement(Boid boid, ref List<Boid> neighbhours)
    {
        int cohensionCount = 0;
        int seperationCount = 0;
        int alignmentCount = 0;
        Vector3 alignment = Vector3.zero;
        Vector3 seperation = Vector3.zero;
        Vector3 cohension = Vector3.zero;
        int length = neighbhours.Count;
        Vector3 boidPosition = boid.Position;
        Vector3 foward = boid.GetForwardVector();
        for (int neighCount = 0; neighCount < length; neighCount++)
        {
            Boid neighbhor = neighbhours[neighCount];
            Vector3 position = neighbhor.Position;
            if (neighbhor != boid)
            {
                Vector3 difference = position - boidPosition;
                float distSqaured = difference.sqrMagnitude;
                if (distSqaured < awarenessRadius * awarenessRadius)
                {
                    if (distSqaured < minDistance * minDistance)
                    {
                        //SEPERATION
                        seperation -= difference;
                        seperationCount++;
                    }
                    //Debug.Log($"dist:{dist}, angle:{angle}");

                    //COHENSION
                    cohension += position;
                    cohensionCount++;
                    float angle = Vector3.Dot(foward, difference.normalized);
                    if (angle >= mFov)
                    {
                        //ALIGNMENT
                        alignment += ((Boid)neighbhor).GetCurrentVelocity();
                        alignmentCount++;
                    }
                }
            }
        }

        if (alignment != Vector3.zero)
        {
            alignment /= (float)alignmentCount;
            alignment.Normalize();
        }
        if (cohension != Vector3.zero)
        {
            cohension /= (float)cohensionCount;
            cohension -= boid.Position;
            cohension.Normalize();
        }
        if (seperation != Vector3.zero)
        {
            seperation /= (float)seperationCount;
            seperation.Normalize();
        }

        seperation *= mBoidConfig.SeparationWeight;  //Separation from each other
        alignment *= mBoidConfig.AlignmentWeight;   // to align all the objects in a particular direction
        cohension *= mBoidConfig.CohesionWeight;    //for grouping


        Vector3 direction = mBoidConfig.Target.position - boidPosition;
        Vector3 targetDirection = direction.normalized;
        targetDirection *= mBoidConfig.FollowTargetWeight;
        targetDirection.x = 3.0f;
        //targetDirection.z = 0.0f;

        Vector3 force = targetDirection + (seperation + alignment + cohension);
        boid.Move(force);
    }

    private void OnGUI()
    {
        if (GUILayout.Button("stop"))
        {
            mStartMovement = false;
        }
        if (GUILayout.Button("Start"))
        {
            mStartMovement = true;
        }
        if (GUILayout.Button(" FInd Neighbhors"))
        {
            var boid = (Boid)mTreeChildren[5];
            var neighs = mSpaceTree.DebugFindNeighboringChildren(boid, boid.ContainerNode, 2, out topNode);
        }
    }

    private void OnDrawGizmos()
    {
        Color originalColor = Gizmos.color;
        if (mSpaceBounds.size != Vector3.zero)
        {
            Gizmos.DrawWireCube(mSpaceBounds.center, mSpaceBounds.size);
        }
        if (mTreeChildren != null)
        {
            foreach (var item in mTreeChildren)
            {
                if (((Boid)item).ContainerNode != null)
                {
                    Bounds bounds = ((Boid)item).ContainerNode.BoundingBox;
                    if (bounds.size != Vector3.zero)
                    {
                        Color yellow = Color.yellow;
                        yellow.a = 0.7f;
                        Gizmos.color = yellow;
                        Gizmos.DrawCube(bounds.center, bounds.size);
                        Gizmos.color = originalColor;
                    }
                }
            }
        }

        if (allRegions != null && allRegions.Count > 0)
        {
            for (int i = 0; i < allRegions.Count; i++)
            {
                var item = allRegions[i];

                Gizmos.color = Color.black;
                Gizmos.DrawSphere(item.center, 0.05f);
                //Debug.Log("item:"+item+ " Size:"+item.size);
                Gizmos.DrawWireCube(item.center, item.size);
                Gizmos.color = originalColor;
            }
        }
        /*
        if (mTreeChildren != null && mTreeChildren.Count > 0 && topNode != null)
        {
            Bounds bds = topNode.BoundingBox;
            Color red = Color.red;
            red.a = 0.4f;
            Gizmos.color = red;
            Gizmos.DrawCube(bds.center, bds.size);
            Gizmos.color = originalColor;

            //var boid = mTreeChildren[5];
            //var bounds = new Bounds(boid.Position, new Vector3(3, 3, 3));
            //Color blue = Color.blue;
            //blue.a = 0.3f;
            //Gizmos.color = blue;
            //Gizmos.DrawCube(bounds.center, bounds.size);
            //Gizmos.color = originalColor;
        }
        */
    }
}
