using UnityEngine;

public class PushableObject : MonoBehaviour
{
    [Header("Push Settings")]
    public bool isPushable = true;
    public bool useKinematic = true;
    public float mass = 1.0f;
    public float drag = 0.5f;
    public float angularDrag = 0.05f;
    
    [Header("Kinematic Settings")]
    public float kinematicMoveSpeed = 2.0f;
    public float maxPushDistance = 5.0f;
    
    [Header("Layer Settings")]
    public LayerMask pushableLayer = 0; // Default layer
    
    private Rigidbody rb;
    
    void Start()
    {
        SetupRigidbody();
    }
    
    void SetupRigidbody()
    {
        // Get or add Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // Configure the rigidbody
        rb.mass = mass;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        rb.isKinematic = useKinematic;
        
        // Set the layer if specified
        if (pushableLayer != 0)
        {
            gameObject.layer = (int)Mathf.Log(pushableLayer.value, 2);
        }
        
        // Add collider if none exists
        if (GetComponent<Collider>() == null)
        {
            // Try to add a BoxCollider as default
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            // You might want to adjust the collider size based on your mesh
        }
    }
    
    // Public method to push this object
    public void Push(Vector3 direction, float force)
    {
        if (!isPushable) return;
        
        if (rb.isKinematic)
        {
            // Move kinematic rigidbody
            Vector3 newPosition = rb.position + direction.normalized * kinematicMoveSpeed * Time.deltaTime;
            float distance = Vector3.Distance(transform.position, newPosition);
            
            if (distance <= maxPushDistance)
            {
                rb.MovePosition(newPosition);
            }
        }
        else
        {
            // Apply force to non-kinematic rigidbody
            rb.AddForce(direction * force, ForceMode.Impulse);
        }
    }
    
    // Method to toggle between kinematic and non-kinematic
    public void ToggleKinematic()
    {
        useKinematic = !useKinematic;
        rb.isKinematic = useKinematic;
    }
    
    // Method to set kinematic state
    public void SetKinematic(bool kinematic)
    {
        useKinematic = kinematic;
        rb.isKinematic = kinematic;
    }
    
    void OnDrawGizmosSelected()
    {
        if (!isPushable) return;
        
        // Draw pushable area
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
        
        // Draw kinematic indicator
        if (rb != null && rb.isKinematic)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
} 