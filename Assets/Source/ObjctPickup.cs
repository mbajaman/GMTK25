using System.Collections;

using System.Collections.Generic;

using UnityEngine;

public class ObjctPickup : MonoBehaviour
{

    [SerializeField] private Transform playerCameraTransform;
    [SerializeField] private LayerMask pickupLayerMask; 
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            float pickUpDistance = 2f;
            Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward, out RaycastHit raycastHit, pickUpDistance);
            if (raycastHit.transform.TryGetComponent(out ObjectGrabable objectGrabable))
            {
                Debug.Log(objectGrabable);
            }
        }
    }

}
