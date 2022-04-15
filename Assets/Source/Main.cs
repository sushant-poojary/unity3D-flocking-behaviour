using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    private bool mStartMovement;
    public GameObject BirdPrefab;
    [SerializeField]
    private SpatialOctree mSpatialTree;
    private List<Boid> mTreeChildren;
    public int NumberOfBirds = 0;
    private int mBoidCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GenerateBirds(NumberOfBirds));
        mBoidCount = NumberOfBirds;
    }

    private IEnumerator GenerateBirds(int count)
    {
        Bounds mainArea = mSpatialTree.SpaceTree.GetRootArea();
        mTreeChildren = new List<Boid>(count);
        for (int i = 0; i < count; i++)
        {
            GameObject bird = GameObject.Instantiate<GameObject>(BirdPrefab);
            Boid boid = new Boid(bird, i.ToString());
            mTreeChildren.Add(boid);

            var x = Random.Range(mainArea.min.x + 0.5f, mainArea.max.x - 0.5f);
            var y = Random.Range(mainArea.min.y + 0.5f, mainArea.max.y - 0.5f);
            var z = Random.Range(mainArea.min.z + 0.5f, mainArea.max.z - 0.5f);
            Vector3 randomPosition = new Vector3(x, y, z);
            bird.transform.position = randomPosition;
            Physics.SyncTransforms();
            OctTree<Boid>.OctNode container;
            if (mSpatialTree.SpaceTree.Insert(boid, out container))
            {
                boid.ContainerNode = container;
                Debug.LogError("--------------------------[START]container: " + container.ID);
            }
            yield return new WaitForEndOfFrame();
        }
        
        mTreeChildren.TrimExcess();
        mStartMovement = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!mStartMovement) return;
        for (int i = 0; i < mBoidCount; i++)
        {
            Boid boid = mTreeChildren[i];
            boid.Move(new Vector3(0, 0, 0.5f));
            OctTree<Boid>.OctNode container;
            //Debug.Log("Boid to bounds " + boid.Bounds);
            if (mSpatialTree.SpaceTree.Update(boid, boid.ContainerNode, out container))
            {
                boid.ContainerNode = container;
                //Debug.Log("[Update] container: " + container.ID);
            }
            else
            {
                //Debug.LogError("Failed to update " + boid + " from node:" + boid.ContainerNode);
            }
        }
    }

    private void OnGUI()
    {
        if (GUILayout.Button("stop"))
        {
            mStartMovement = false;
        }
    }

    private void OnDrawGizmos()
    {
        Color originalColor = Gizmos.color;
        if (mTreeChildren != null)
        {
            foreach (var item in mTreeChildren)
            {
                if (item.ContainerNode != null)
                {
                    Bounds bounds = item.ContainerNode.BoundingBox;
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
    }
}
