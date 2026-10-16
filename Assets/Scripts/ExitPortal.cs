using UnityEngine;

/// Completes the level when the real player touches it.
[RequireComponent(typeof(Collider2D))]
public class ExitPortal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<PlayerInputReader>() != null)
            LevelManager.Instance.Complete();
    }
}