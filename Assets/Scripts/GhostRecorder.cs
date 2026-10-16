using System;
using System.Collections.Generic;
using UnityEngine;

/// Records the player's inputs (R toggles) and spawns a ghost that replays them.
[RequireComponent(typeof(PlayerInputReader), typeof(RobotController))]
public class GhostRecorder : MonoBehaviour
{
    public GameObject ghostPrefab;
    public int maxTicks = 250; // 5 seconds at 50 ticks per second

    public bool IsRecording { get; private set; }
    public float Progress => (float)frames.Count / maxTicks; // 0..1, for the UI timer bar

    public event Action RecordingStarted;
    public event Action RecordingStopped;
    public event Action GhostSpawned;

    PlayerInputReader reader;
    RobotController robot;
    Collider2D playerCollider;
    readonly List<InputFrame> frames = new List<InputFrame>();
    Vector2 startPos, startVel;
    GameObject currentGhost;
    bool toggleLatch;

    void Awake()
    {
        reader = GetComponent<PlayerInputReader>();
        robot = GetComponent<RobotController>();
        playerCollider = GetComponent<Collider2D>();
    }

    void OnEnable() => reader.FrameSampled += OnFrame;
    void OnDisable() => reader.FrameSampled -= OnFrame;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R)) toggleLatch = true;
    }

    void FixedUpdate()
    {
        if (!toggleLatch) return;
        toggleLatch = false;
        if (IsRecording) StopRecording(); else StartRecording();
    }

    /// Called once per physics tick with the input the player just used.
    void OnFrame(InputFrame frame)
    {
        if (!IsRecording) return;
        frames.Add(frame);
        if (frames.Count >= maxTicks) StopRecording(); // auto-stop at 5 seconds
    }

    /// Remember the starting state and begin capturing inputs.
    void StartRecording()
    {
        frames.Clear();
        startPos = robot.Body.position;
        startVel = robot.Body.linearVelocity;
        // Clear coyote/buffer timers so the player starts in the same state the ghost will
        robot.ResetState(startPos, startVel);
        IsRecording = true;
        RecordingStarted?.Invoke();
    }

    /// Send the player back to the start and spawn a ghost that replays the recording.
    void StopRecording()
    {
        IsRecording = false;
        RecordingStopped?.Invoke();
        if (frames.Count == 0) return;

        robot.ResetState(startPos, startVel);

        if (currentGhost != null) Destroy(currentGhost); // only one ghost at a time
        currentGhost = Instantiate(ghostPrefab, startPos, Quaternion.identity);
        currentGhost.GetComponent<GhostPlayback>()
            .Init(frames.ToArray(), startPos, startVel, playerCollider);
        GhostSpawned?.Invoke();
    }

    /// Remove the ghost and cancel any recording (call when the level restarts).
    public void ClearGhost()
    {
        IsRecording = false;
        if (currentGhost != null) Destroy(currentGhost);
    }
}