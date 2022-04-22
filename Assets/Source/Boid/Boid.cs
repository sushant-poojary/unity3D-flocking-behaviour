using System;
using UnityEngine;

public class Boid : ITreeChild
{
    //public const float MAX_SEPERATION_SPEED = 5;
    private Bounds mStartingBounds;
    public Bounds GetBounds()
    {
        //Bounds bounds = mStartingBounds;
        mStartingBounds.center = mTransform.position;
        return mStartingBounds;
    }

    public OctTree<Boid>.OctNode ContainerNode { get; set; }
    public Vector3 Position => mTransform.position;
    public string ID { get; private set; }

    private readonly BoidConfig mConfig;
    private SkinnedMeshRenderer mMeshRenderer;
    private BoxCollider mCollider;
    private Vector3 mVelocity;
    private GameObject mGameObject;
    private Transform mTransform;
    public Boid(GameObject gameObject, string ID, BoidConfig config)
    {
        this.ID = ID;
        mConfig = config;
        mGameObject = gameObject;
        mTransform = mGameObject.transform;
        mMeshRenderer = mGameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        mCollider = mGameObject.GetComponent<BoxCollider>();
        mStartingBounds = mCollider.bounds;
        mCollider.enabled = false;
        //Bounds = mCollider.bounds;
        //Bounds = mCollider.bounds;
        mVelocity = Vector3.zero;// new Vector3(UnityEngine.Random.Range(0.1f, 1f), 0, UnityEngine.Random.Range(0.1f, 1f));
    }

    public void Move(Vector3 alignmentSteering, Vector3 seperation, Vector3 cohension)
    {
        //Debug.Log($"alignmentSteering:{alignmentSteering}, seperation:{seperation}, cohension:{cohension}");

        Vector3 direction = mConfig.Target.position - Position;
        Vector3 force = direction.normalized;
        force *= mConfig.FollowTargetWeight;
        force.x = 0.0f;
        force.z = 0.0f;
        //force.y = 0;
        //force *= 4;
        var vel = force + (alignmentSteering + seperation + cohension);
        //vel.y = 0;
        mVelocity += vel;
        if (mVelocity.sqrMagnitude > mConfig.MAX_SPEED * mConfig.MAX_SPEED)
        {
            mVelocity = mVelocity.normalized * mConfig.MAX_SPEED;
        }
        //mVelocity.Normalize();
        mTransform.position += (mVelocity * Time.deltaTime);
        Vector3 newDirection = Vector3.RotateTowards(mTransform.forward, mVelocity, 5f * Time.deltaTime, 0.0f);
        //Quaternion prevRotation = mTransform.rotation;
        Debug.DrawRay(mTransform.position, newDirection, Color.red);

        mTransform.rotation = Quaternion.LookRotation(newDirection);// Quaternion.Slerp(prevRotation, newDirection,);


    }

    public override string ToString()
    {
        return base.ToString();
    }

    internal Vector3 GetCurrentVelocity()
    {
        return mVelocity;
    }

    //internal void ChangeVelocity(Vector3 vector)
    //{
    //    mVelocity = vector;
    //}

    internal void ChangePosition(Vector3 position)
    {
        mTransform.position = position;
    }

    internal Vector3 GetForwardVector()
    {
        return mTransform.forward;
    }
}
