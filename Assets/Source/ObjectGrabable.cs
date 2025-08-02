using System.Collections;

using System.Collections.Generic;

using UnityEngine;

public class ObjectGrabable : MonoBehaviour
{

    private Rigidbody objectRigibody;
    private Transform objectGrabPointTransform;

    private void Awake()
    {
       objectRigibody = GetComponent<Rigidbody>();
    }
    public void Grab(Transform objectGrabPointTransform)
    {
        this.objectGrabPointTransform = objectGrabPointTransform;
        objectRigibody.useGravity = false;
    }

    public void Drop()

    {
        this.objectGrabPointTransform = null;
        objectRigibody.useGravity = true;


    }
    private void FixedUpdate()
    {
        if (objectGrabPointTransform != null)
        {
            float lerpSpeed = 20f;
            Vector3 newPosition = Vector3.Lerp(transform.position, objectGrabPointTransform.position, Time.deltaTime * lerpSpeed );
            objectRigibody.MovePosition(newPosition);
        }

    }
}
