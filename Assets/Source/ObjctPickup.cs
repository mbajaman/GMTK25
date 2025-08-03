using System.Collections;

using System.Collections.Generic;

using UnityEngine;

public class ObjctPickup : MonoBehaviour
{

    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private Transform objectGrabPointTransform;
    [SerializeField] private LayerMask pickupLayerMask;

    private ObjectGrabable objectGrabable;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (objectGrabable == null)
            {
                //Not Carrying an object, try to grab
                float pickUpDistance = 2f;
                Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit raycastHit, pickUpDistance);
                if (raycastHit.transform.TryGetComponent(out objectGrabable))
                {
                    objectGrabable.Grab(objectGrabPointTransform);
                    objectGrabable.Pickup();

                    Debug.Log(objectGrabable);

                }
            }
            else
            {
                //Currently carrying something, drop
                objectGrabable.Drop();
                objectGrabable = null;
            }
        }

    }

}    
