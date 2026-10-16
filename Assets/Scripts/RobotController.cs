using UnityEngine;

/// Moves the robot from an InputFrame. The player and the ghost share this exact code,
/// so a replayed recording plays out the same way every loop.
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class RobotController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float jumpVelocity = 14f;
    public float gravityScale = 4f;
    [Tooltip("Extra gravity while rising with jump released (short hop)")]
    public float lowJumpMultiplier = 2.5f;

    [Header("Feel (counted in physics ticks, 50 per second)")]
    public int coyoteTicks = 6;      // grace window to jump after leaving a ledge
    public int jumpBufferTicks = 6;  // jump pressed slightly before landing still counts

    [Header("Ground check")]
    public LayerMask groundMask;
    public float checkDistance = 0.05f;

    Rigidbody2D rb;
    BoxCollider2D box;
    ContactFilter2D groundFilter;
    readonly Collider2D[] hits = new Collider2D[8];
    int coyoteCounter;
    int bufferCounter;

    public bool IsGrounded { get; private set; }
    public Rigidbody2D Body => rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        rb.gravityScale = gravityScale;

        groundFilter = new ContactFilter2D();
        groundFilter.SetLayerMask(groundMask);
        groundFilter.useTriggers = false; // plates and the exit are not ground
    }

    /// Advance one physics tick using the given input. Call from FixedUpdate only.
    public void Simulate(InputFrame input)
    {
        IsGrounded = CheckGround();

        // Both timers count down in ticks, not seconds, so replays stay deterministic.
        coyoteCounter = IsGrounded ? coyoteTicks : Mathf.Max(0, coyoteCounter - 1);
        bufferCounter = input.jumpPressed ? jumpBufferTicks : Mathf.Max(0, bufferCounter - 1);

        Vector2 v = rb.linearVelocity;
        v.x = input.move * moveSpeed;

        // Jump if a press is buffered and we are (or just were) on the ground
        if (bufferCounter > 0 && coyoteCounter > 0)
        {
            v.y = jumpVelocity;
            bufferCounter = 0;
            coyoteCounter = 0;
        }

        // Released early while rising: fall faster for a short hop
        if (v.y > 0f && !input.jumpHeld)
            v.y += Physics2D.gravity.y * gravityScale * (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;

        rb.linearVelocity = v;
    }

    /// Put the robot at a position with a given velocity and cleared timers.
    /// The ghost calls this every loop so it never drifts.
    public void ResetState(Vector2 position, Vector2 velocity)
    {
        rb.position = position;
        transform.position = position;
        rb.linearVelocity = velocity;
        coyoteCounter = 0;
        bufferCounter = 0;
    }

    /// Box-cast just below the feet, ignoring our own collider.
    bool CheckGround()
    {
        Bounds b = box.bounds;
        Vector2 center = new Vector2(b.center.x, b.min.y - checkDistance * 0.5f);
        Vector2 size = new Vector2(b.size.x * 0.9f, checkDistance);
        int count = Physics2D.OverlapBox(center, size, 0f, groundFilter, hits);
        for (int i = 0; i < count; i++)
            if (hits[i] != box) return true;
        return false;
    }
}