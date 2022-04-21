using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Main : MonoBehaviour
{
    private const float BOUNDARY_BUFFER = 4;
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

        mSpaceTree = new OctTree<Boid>(transform.position, 64, 2);
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

            //var x = Random.Range(-1, 2);//Random.Range(mSpaceBounds.min.x + 2f, mSpaceBounds.max.x - 2f);
            //var y = Random.Range(-1, 2);//Random.Range(mSpaceBounds.min.y + 2f, mSpaceBounds.max.y - 2f);
            //var z = Random.Range(-1, 2);//Random.Range(mSpaceBounds.min.z + 2f, mSpaceBounds.max.z - 2f);

            var x = Random.Range(mSpaceBounds.min.x + BOUNDARY_BUFFER, mSpaceBounds.max.x - BOUNDARY_BUFFER);

            var z = Random.Range(mSpaceBounds.min.z + BOUNDARY_BUFFER, mSpaceBounds.max.z - BOUNDARY_BUFFER);

            Vector3 randomPosition = new Vector3(x, y, z);
            bird.transform.position = randomPosition;
            //Physics.SyncTransforms();
            //OctTree<Boid>.OctNode container;
            //if (mSpaceTree.Insert(boid, out container))
            //{
            //    boid.ContainerNode = container;
            //    //Debug.LogError("--------------------------[START]container: " + container.ID);
            //}
            yield return new WaitForEndOfFrame();
        }
        List<OctTree<Boid>.OctNode> containers;
        mSpaceTree.BuildTree(mTreeChildren, out containers);
        foreach (var item in containers)
        {
            foreach (var boid in mTreeChildren)
            {
                if (item.Contains(boid))
                {
                    boid.ContainerNode = item;
                }
            }
        }
        allRegions = mSpaceTree.GetAllRegions();
        mTreeChildren.TrimExcess();
        //mStartMovement = true;
    }

    //private void GetSeperation()
    //{
    //    var desiredSeparation = getBoidViewDistance() / 2;

    //    var desired = new Victor(0, 0);

    //    // For every flockmate, check if it's too close
    //    for (var i = 0, l = boids.length; i < l; ++i)
    //    {
    //        var other = boids[i];
    //        var dist = this.position.distance(other.position);
    //        if (dist < desiredSeparation && dist > 0)
    //        {
    //            // Calculate vector pointing away from the flockmate, weighted by distance
    //            var diff = this.position.clone().subtract(other.position).normalize().divideScalar(dist);
    //            desired.add(diff);
    //        }
    //    }

    //    // If the boid had flockmates to separate from
    //    if (desired.length() > 0)
    //    {
    //        // We set the average vector to the length of our desired speed
    //        desired.normalize().multiplyScalar(getDesiredSpeed());

    //        // We then calculate the steering force needed to get to that desired velocity
    //        return this.steer(desired);
    //    }

    //    return desired;
    //}

    // Update is called once per frame
    void Update()
    {
        if (!mStartMovement) return;
        for (int i = 0; i < mBoidCount; i++)
        {
            Boid boid = (Boid)mTreeChildren[i];
            neighs = mSpaceTree.FindNeighboringChildren(boid, boid.ContainerNode, 4);
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
                boid.ContainerNode = container;
                //Debug.Log("GOT IT!!!      [Update] container: " + container.ID);
            }
            else
            {
                //Debug.LogError("Failed to update " + boid + " from node:" + boid.ContainerNode);
            }

            allRegions = mSpaceTree.GetAllRegions();
        }
    }

    private void CalculateMovement(Boid boid, ref List<Boid> neighs)
    {
        int cohensionCount = 0;
        int seperationCount = 0;
        int alignmentCount = 0;
        Vector3 alignment = Vector3.zero;
        Vector3 seperation = Vector3.zero;
        Vector3 cohension = Vector3.zero;
        int length = neighs.Count;
        Vector3 boidPosition = boid.Position;
        Vector3 foward = boid.GetForwardVector();
        for (int neighCount = 0; neighCount < length; neighCount++)
        {
            Boid neighbhor = neighs[neighCount];
            Vector3 position = neighbhor.Position;
            if (neighbhor != boid)
            {
                var difference = position - boidPosition;
                var dist = difference.sqrMagnitude;
                if (dist < awarenessRadius * awarenessRadius)
                {
                    if (dist < minDistance * minDistance)
                    {
                        seperation -= difference;
                        seperationCount++;
                    }
                    //Debug.Log($"dist:{dist}, angle:{angle}");

                    cohension += position;
                    cohensionCount++;
                    float angle = Vector3.Dot(foward, difference.normalized);
                    if (angle >= mFov)
                    {
                        alignment += ((Boid)neighbhor).GetCurrentVelocity();
                        alignmentCount++;
                    }
                }
            }
        }

        if (alignment != Vector3.zero)
        {
            alignment /= (float)alignmentCount;
        }
        if (cohension != Vector3.zero)
        {
            cohension /= (float)cohensionCount;
        }
        if (seperation != Vector3.zero)
        {
            seperation /= (float)seperationCount;
            //seperation *= -1;
        }
        //Debug.Log($"alignment:{alignment}, cohension:{cohension}, seperation:{seperation} neighs.Count:{neighs.Count}");


        if (alignment != Vector3.zero)
        {
            //alignment -= mVelocity;
            alignment.Normalize();
            //alignmentSteering *= mConfig.MAX_SPEED;
            //alignmentSteering = alignmentSteering - mVelocity;

            //if (alignmentSteering.sqrMagnitude > mConfig.MAX_SPEED * mConfig.MAX_SPEED)
            //{

            //    alignmentSteering.Normalize();
            //    alignmentSteering *= mConfig.MAX_SPEED;
            //}
        }

        if (seperation != Vector3.zero)
        {
            seperation.Normalize();
            //seperation *= mConfig.MAX_SPEED;
            //seperation -= mVelocity;
            //if (seperation.sqrMagnitude > mConfig.MAX_SPEED * mConfig.MAX_SPEED)
            //{

            //    seperation.Normalize();
            //    seperation *= mConfig.MAX_SPEED;
            //}
        }

        if (cohension != Vector3.zero)
        {
            cohension -= boid.Position;
            cohension.Normalize();
            //cohension *= mConfig.MAX_SPEED;
            //cohension = cohension - mVelocity;

            //if (cohension.sqrMagnitude > mConfig.MAX_SPEED * mConfig.MAX_SPEED)
            //{

            //    cohension.Normalize();
            //    cohension *= mConfig.MAX_SPEED;
            //}
        }

        seperation *= mBoidConfig.SeparationWeight;  //Separation from each other
        alignment *= mBoidConfig.AlignmentWeight;   // to align all the objects in a particular direction
        cohension *= mBoidConfig.CohesionWeight;    //for grouping


        boid.Move(alignment, seperation, cohension);
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
