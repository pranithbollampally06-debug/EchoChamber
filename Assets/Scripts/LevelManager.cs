using UnityEngine;
using UnityEngine.SceneManagement;

/// Handles restart (R is for recording, so Backspace restarts), level complete,
/// and unlocking progress. One per level scene.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Tooltip("Scene to load after finishing this level. Leave empty to reload the menu later.")]
    public string nextSceneName;

    bool finished;

    void Awake() => Instance = this;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace)) Restart();
    }

    /// Reload the current level instantly (death or manual restart).
    public void Restart()
    {
        if (finished) return;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// Level cleared: save progress and load the next scene.
    public void Complete()
    {
        if (finished) return;
        finished = true;

        int next = SceneManager.GetActiveScene().buildIndex + 1;
        int unlocked = PlayerPrefs.GetInt("HighestUnlocked", 1);
        PlayerPrefs.SetInt("HighestUnlocked", Mathf.Max(unlocked, next));
        PlayerPrefs.Save();

        if (!string.IsNullOrEmpty(nextSceneName)) SceneManager.LoadScene(nextSceneName);
        else Debug.Log("Level complete! (set Next Scene Name to continue)");
    }
}