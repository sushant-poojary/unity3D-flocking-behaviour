using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]
public class CheckFov : MonoBehaviour
{
    public Transform Target;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var difference = Target.position - transform.position;
        float angle = Vector3.Dot(transform.forward.normalized, difference.normalized);
        Debug.Log($"Angle:{angle}");
    }
}
