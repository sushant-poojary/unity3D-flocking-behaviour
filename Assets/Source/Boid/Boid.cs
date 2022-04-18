using System;
using UnityEngine;

public class Boid : ITreeChild
{
    public const float MAX_SPEED = 3;
    public const float MAX_SEPERATION_SPEED = 5;

    public Bounds Bounds
    {
        //get { return mCollider.bounds; }
        get;
    }

    public OctTree<Boid>.OctNode ContainerNode { get; set; }
    public Vector3 Position => mTransform.position;
    public string ID { get; private set; }
    private SkinnedMeshRenderer mMeshRenderer;
    private BoxCollider mCollider;
    private Vector3 mVelocity;
    private GameObject mGameObject;
    private Transform mTransform;
    public Boid(GameObject gameObject, string ID)
    {
        this.ID = ID;
        mGameObject = gameObject;
        mTransform = mGameObject.transform;
        mMeshRenderer = mGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        mCollider = mGameObject.GetComponent<BoxCollider>();
        Bounds = mCollider.bounds;
        mVelocity = new Vector3(UnityEngine.Random.Range(0.1f, 1f), 0, UnityEngine.Random.Range(0.1f, 1f));
    }

    public void Move(Vector3 alignmentSteering, Vector3 seperation, Vector3 cohension)
    {
        /*
        if (alignmentSteering != Vector3.zero)
        {
            alignmentSteering.Normalize();
            //alignmentSteering *= MAX_SPEED;
            //alignmentSteering = alignmentSteering - mVelocity;

            //if (alignmentSteering.sqrMagnitude > 2 * 2)
            //{

            //    alignmentSteering.Normalize();
            //    alignmentSteering *= MAX_SPEED;
            //}
            //alignmentSteering *= 0.3f;
        }

        if (seperation != Vector3.zero)
        {
            seperation.Normalize();
            //seperation *= MAX_SEPERATION_SPEED;
            //seperation = seperation - mVelocity;
            //alignmentSteering = alignmentSteering - mVelocity;
        }

        if (cohension != Vector3.zero)
        {
            //cohension = cohension - Position;
            cohension.Normalize();
            //cohension *= MAX_SPEED;
            //cohension = cohension - mVelocity;

            //if (cohension.sqrMagnitude > 2 * 2)
            //{

            //    cohension.Normalize();
            //    cohension *= MAX_SPEED;
            //}
            //cohension *= 0.6f;
            //alignmentSteering = alignmentSteering - mVelocity;
        }
        //Debug.Log($"alignmentSteering:{alignmentSteering}, seperation:{seperation}, cohension:{cohension}");
        mVelocity += (alignmentSteering + seperation + cohension);
        if (mVelocity.sqrMagnitude > MAX_SPEED * MAX_SPEED)
        {
            mVelocity = mVelocity.normalized * MAX_SPEED;
        }
        mVelocity.Normalize();
        mVelocity *= 2.0f;
        Vector3 prev = mTransform.position;
        mTransform.position += (mVelocity * Time.deltaTime);
        Vector3 dir = mTransform.position - prev;
        Vector3 newDirection = Vector3.RotateTowards(mTransform.forward, mVelocity, 0.5f, 1.0f);
        //Debug.DrawRay(mTransform.position, newDirection, Color.red);
        mTransform.rotation = Quaternion.LookRotation(newDirection);
        */
    }

    public override string ToString()
    {
        return base.ToString();
    }

    internal Vector3 GetCurrentVelocity()
    {
        return mVelocity;
    }

    internal void ChangeVelocity(Vector3 vector)
    {
        mVelocity = vector;
    }

    internal void ChangePosition(Vector3 position)
    {
        mTransform.position = position;
    }
}
