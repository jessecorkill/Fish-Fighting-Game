using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class WaypointPatrol2D : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float arriveDistance = 0.05f;   // how close counts as “arrived”
    public bool loop = true;               // loop vs. ping-pong

    [Header("Path")]
    public Transform[] waypoints;

    [Header("Animation (optional)")]
    public Animator animator;              // expects float MoveX, MoveY, and bool IsMoving
    public SpriteRenderer spriteRenderer;  // if you want to just flip instead of using Animator

    private Rigidbody2D rb;
    private int currentIndex = 0;
    private int dir = 1; // used for ping-pong if loop=false

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        Vector2 target = waypoints[currentIndex].position;
        Vector2 pos = rb.position;
        Vector2 toTarget = target - pos;
        float dist = toTarget.magnitude;

        Vector2 step = Vector2.zero;
        if (dist > arriveDistance)
        {
            Vector2 dirNorm = toTarget.normalized;
            step = dirNorm * speed * Time.fixedDeltaTime; //A value representing how far to move the item at the current frame
            rb.MovePosition(pos + step);

            // --- Animation or Sprite flip updates based on direction ---
            UpdateFacing(step);
        }
        else
        {
            AdvanceIndex();
        }
    }

    private void AdvanceIndex()
    {
        if (loop)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
        else
        {
            // ping-pong
            if (currentIndex == waypoints.Length - 1) dir = -1;
            else if (currentIndex == 0) dir = 1;
            currentIndex += dir;
        }
    }

    private void UpdateFacing(Vector2 velocity)
    {
        bool isMoving = velocity.sqrMagnitude > 0.0001f;

        if (animator)
        {
            animator.SetBool("IsMoving", isMoving);
            animator.SetFloat("MoveX", velocity.x);
            animator.SetFloat("MoveY", velocity.y);
        }

        // Simple flip option (useful if you only have left/right art):
        if (spriteRenderer)
        {
            if (Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y)) //Is horizontal movement
            {
                // Face left/right -> flip on X
                if (velocity.x != 0f) spriteRenderer.flipX = (velocity.x > 0f);
            }
            // If you have up/down variants, prefer Animator-based swap instead of flip.
        }
    }

    // Visualize path in Scene view
    void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] && waypoints[i + 1])
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
        }
        if (loop && waypoints[0] && waypoints[waypoints.Length - 1])
            Gizmos.DrawLine(waypoints[0].position, waypoints[waypoints.Length - 1].position);
    }
}
