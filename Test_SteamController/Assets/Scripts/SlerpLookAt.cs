﻿using UnityEngine;
using System.Collections;

public class SlerpLookAt : MonoBehaviour {

    //values that will be set in the Inspector
    public Transform Target;
    public float RotationSpeed;

    //values for internal use
    private Quaternion lookRotation;
    private Vector3 direction;

    // Update is called once per frame
    void Update()
    {
        //find the vector pointing from our position to the target
        direction = (Target.position - transform.position).normalized;

        //create the rotation we need to be in to look at the target
        lookRotation = Quaternion.LookRotation(direction);

        //rotate us over time according to speed until we are in the required rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * RotationSpeed);
    }
}
