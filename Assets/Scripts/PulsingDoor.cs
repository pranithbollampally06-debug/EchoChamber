using UnityEngine;

/// A door that opens and closes on a fixed tick timer. The timer restarts when recording
/// starts and every time the ghost loops, so the recording and the replay see identical timing.
[RequireComponent(typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class PulsingDoor : MonoBehaviour
{
    public int openTicks = 60;      // 50 ticks = 1 second
    public int closedTicks = 65;
    public int startOffset = 0;     // shifts the phase so two doors can be out of sync
    [Tooltip("Optional: while this plate is held, the door stays open")]
    public PressurePlate holdOpenPlate;
    public LayerMask blockers;      // set to Robot so the door never closes on someone

    BoxCollider2D box;
    SpriteRenderer sprite;
    GhostRecorder recorder;
    int tick;
    bool isOpen;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        GhostPlayback.Looped += ResetClock;
        recorder = FindAnyObjectByType<GhostRecorder>();
        if (recorder != null) recorder.RecordingStarted += ResetClock;
    }

    void OnDisable()
    {
        GhostPlayback.Looped -= ResetClock;
        if (recorder != null) recorder.RecordingStarted -= ResetClock;
    }

    void ResetClock() => tick = 0;

    void FixedUpdate()
    {
        int cycle = openTicks + closedTicks;
        int phase = (tick++ + startOffset) % cycle;
        bool wantOpen = phase < openTicks || (holdOpenPlate != null && holdOpenPlate.IsPressed);

        // Flicker for the last 0.3s before closing so the player can read the timing
        bool warning = wantOpen && holdOpenPlate == null && phase >= openTicks - 15 && phase % 6 < 3;

        if (!wantOpen && isOpen && IsOccupied()) wantOpen = true; // never crush anyone
        if (wantOpen != isOpen) SetOpen(wantOpen);
        if (isOpen) SetAlpha(warning ? 0.6f : 0.2f);
    }

    /// True if a robot is standing where the door would close.
    bool IsOccupied()
    {
        Vector2 size = transform.lossyScale;
        return Physics2D.OverlapBox(transform.position, size, 0f, blockers) != null;
    }

    void SetOpen(bool open)
    {
        isOpen = open;
        box.enabled = !open;
        SetAlpha(open ? 0.2f : 1f);
        AudioManager.Play(open ? Sfx.DoorOpen : Sfx.DoorClose, 0.4f);
    }

    void SetAlpha(float a)
    {
        Color c = sprite.color;
        c.a = a;
        sprite.color = c;
    }
}