using UnityEngine;

[System.Serializable]
public class BoidConfig
{
    public float AlignmentWeight;
    public float CohesionWeight;
    public float SeparationWeight;
    public float MAX_SPEED = 3;
    public float FollowTargetWeight;

    public Transform Target;

}
