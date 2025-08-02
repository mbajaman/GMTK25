using UnityEngine;

public class AdvancedRigidBodyPush : MonoBehaviour
{
    [Header("Push Settings")]
    public LayerMask pushLayers = -1; // Default to all layers
    public bool canPush = true;
    [Range(0.5f, 10f)] public float strength = 2.0f;
    [Range(0.1f, 2f)] public float pushRadius = 1.0f;
    
    [Header("Kinematic Push Settings")]
    public bool canPushKinematic = true;
    [Range(0.1f, 5f)] public float kinematicPushSpeed = 2.0f;
    public float maxPushDistance = 3.0f;
    
    [Header("Visual Feedback")]
    public bool showPushGizmos = true;
    public Color gizmoColor = Color.yellow;
    
    private CharacterController characterController;
    private Vector3 lastMoveDirection;
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("AdvancedRigidBodyPush requires a CharacterController component!");
        }
    }
    
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!canPush) return;
        
        // Store the move direction for kinematic pushing
        lastMoveDirection = hit.moveDirection;
        
        // Try to push non-kinematic rigid bodies
        PushNonKinematicRigidBodies(hit);
        
        // Try to push kinematic rigid bodies
        if (canPushKinematic)
        {
            PushKinematicRigidBodies(hit);
        }
    }
    
    private void PushNonKinematicRigidBodies(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;
        
        // Check if the object is on a pushable layer
        var bodyLayerMask = 1 << body.gameObject.layer;
        if ((bodyLayerMask & pushLayers.value) == 0) return;
        
        // Don't push objects below us
        if (hit.moveDirection.y < -0.3f) return;
        
        // Calculate push direction from move direction, horizontal motion only
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);
        
        // Apply the push force
        body.AddForce(pushDir * strength, ForceMode.Impulse);
    }
    
    private void PushKinematicRigidBodies(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || !body.isKinematic) return;
        
        // Check if the object is on a pushable layer
        var bodyLayerMask = 1 << body.gameObject.layer;
        if ((bodyLayerMask & pushLayers.value) == 0) return;
        
        // Don't push objects below us
        if (hit.moveDirection.y < -0.3f) return;
        
        // Calculate push direction
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z).normalized;
        
        // Calculate new position
        Vector3 newPosition = body.position + pushDir * kinematicPushSpeed * Time.deltaTime;
        
        // Check if the new position is within max distance
        float distance = Vector3.Distance(transform.position, newPosition);
        if (distance <= maxPushDistance)
        {
            // Move the kinematic rigid body
            body.MovePosition(newPosition);
        }
    }
    
    // Alternative method for pushing objects when not colliding
    public void PushNearbyObjects()
    {
        if (!canPush) return;
        
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, pushRadius, pushLayers);
        
        foreach (Collider col in nearbyColliders)
        {
            Rigidbody body = col.attachedRigidbody;
            if (body == null) continue;
            
            Vector3 direction = (col.transform.position - transform.position).normalized;
            direction.y = 0; // Keep it horizontal
            
            if (!body.isKinematic)
            {
                // Push non-kinematic rigid bodies
                body.AddForce(direction * strength, ForceMode.Impulse);
            }
            else if (canPushKinematic)
            {
                // Push kinematic rigid bodies
                Vector3 newPosition = body.position + direction * kinematicPushSpeed * Time.deltaTime;
                float distance = Vector3.Distance(transform.position, newPosition);
                
                if (distance <= maxPushDistance)
                {
                    body.MovePosition(newPosition);
                }
            }
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!showPushGizmos) return;
        
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, pushRadius);
        
        // Draw push direction if we have a last move direction
        if (lastMoveDirection != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, lastMoveDirection * 2f);
        }
    }
} 