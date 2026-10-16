using UnityEngine;

/// Kills the player on contact. The ghost ignores spikes (it just replays inputs).
[RequireComponent(typeof(Collider2D))]
public class Spikes : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only the real player dies; the ghost has no PlayerInputReader
        if (other.GetComponent<PlayerInputReader>() != null)
            LevelManager.Instance.Restart();
    }
}