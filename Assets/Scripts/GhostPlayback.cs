using UnityEngine;

/// Replays recorded inputs through the same RobotController the player uses, looping forever.
[RequireComponent(typeof(RobotController))]
public class GhostPlayback : MonoBehaviour
{
    /// Fired whenever a ghost (re)starts its loop, so pulsing doors can stay in sync.
    public static event System.Action Looped;

    public Color tint = new Color(0.5f, 0.8f, 1f, 0.55f);

    RobotController robot;
    Collider2D myCollider, playerCollider;
    InputFrame[] frames;
    Vector2 startPos, startVel;
    int tick;
    bool ignoringPlayer;

    void Awake()
    {
        robot = GetComponent<RobotController>();
        myCollider = GetComponent<Collider2D>();
        GetComponent<SpriteRenderer>().color = tint;
    }

    /// Called by the recorder right after the ghost spawns.
    public void Init(InputFrame[] recorded, Vector2 pos, Vector2 vel, Collider2D player)
    {
        frames = recorded;
        startPos = pos;
        startVel = vel;
        playerCollider = player;
        tick = 0;
        robot.ResetState(startPos, startVel);
        IgnorePlayerUntilClear();
        Looped?.Invoke();
    }

    void FixedUpdate()
    {
        if (frames == null) return;

        // End of recording: snap back to the exact start state and loop
        if (tick >= frames.Length)
        {
            robot.ResetState(startPos, startVel);
            tick = 0;
            IgnorePlayerUntilClear();
            Looped?.Invoke();
        }

        robot.Simulate(frames[tick++]); // same code path as the player

        // Re-enable ghost/player collision once they no longer overlap
        if (ignoringPlayer && !myCollider.bounds.Intersects(playerCollider.bounds))
        {
            Physics2D.IgnoreCollision(myCollider, playerCollider, false);
            ignoringPlayer = false;
        }
    }

    /// Spawning or looping on top of the player would shove them apart (and break
    /// determinism), so skip collision between them until they separate.
    void IgnorePlayerUntilClear()
    {
        if (playerCollider == null || !myCollider.bounds.Intersects(playerCollider.bounds)) return;
        Physics2D.IgnoreCollision(myCollider, playerCollider, true);
        ignoringPlayer = true;
    }

    void OnDestroy()
    {
        if (ignoringPlayer && playerCollider != null)
            Physics2D.IgnoreCollision(myCollider, playerCollider, false);
    }
}