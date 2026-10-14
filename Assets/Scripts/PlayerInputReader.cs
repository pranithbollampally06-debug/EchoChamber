using UnityEngine;

/// Reads the keyboard for the real player and feeds the controller each physics tick.
public class PlayerInputReader : MonoBehaviour
{
    RobotController robot;
    bool jumpLatch; // Update sees the key press, FixedUpdate consumes it

    void Awake() => robot = GetComponent<RobotController>();

    void Update()
    {
        // Key-down events only exist in Update, so latch them or fast taps get dropped
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.Space))
            jumpLatch = true;
    }

    void FixedUpdate() => robot.Simulate(Sample());

    /// Builds this tick's InputFrame. The recorder will call this too.
    public InputFrame Sample()
    {
        InputFrame f;
        f.move = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
        f.jumpPressed = jumpLatch;
        f.jumpHeld = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space);
        jumpLatch = false;
        return f;
    }
}