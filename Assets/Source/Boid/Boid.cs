using System;
using UnityEngine;

public class Boid : ITreeChild
{
    public const float MAX_SPEED = 3;

    public Bounds Bounds
    {
        get { return mCollider.bounds; }
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
        mVelocity = new Vector3(UnityEngine.Random.Range(0.1f, 1f), UnityEngine.Random.Range(0.1f, 1f), UnityEngine.Random.Range(0.1f, 1f));
    }

    public void Move(Vector3 alignmentSteering)
    {
        if (alignmentSteering != Vector3.zero)
        {
            alignmentSteering.Normalize();
            alignmentSteering *= MAX_SPEED;
            alignmentSteering = alignmentSteering - mVelocity;
        }

        mVelocity += (alignmentSteering);
        if (mVelocity.sqrMagnitude > MAX_SPEED * MAX_SPEED)
        {
            mVelocity = mVelocity.normalized * MAX_SPEED;
        }
        mTransform.position += (mVelocity * Time.deltaTime);
        Vector3 newDirection = Vector3.RotateTowards(mTransform.forward, mVelocity, 1, 0.0f);
        Debug.DrawRay(mTransform.position, newDirection, Color.red);
        mTransform.rotation = Quaternion.LookRotation(newDirection);
    }

    public override string ToString()
    {
        return base.ToString();
    }

    internal Vector3 GetForwardVector()
    {
        return mVelocity;
    }

    internal void ChangeVelocity(Vector3 vector)
    {
        mVelocity = vector;
    }
}
