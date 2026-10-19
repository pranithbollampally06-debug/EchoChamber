using UnityEngine;
using UnityEngine.SceneManagement;

/// Handles restart (Backspace), level complete, and progress. One per level scene.
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Tooltip("This level's number, e.g. 1 for Level1")]
    public int levelNumber = 1;
    public int totalLevels = 10;

    bool finished;

    void Awake() => Instance = this;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace)) Restart();
        if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene("LevelSelect"); // pause menu comes later
    }

    /// Reload the current level instantly (death or manual restart).
    public void Restart()
    {
        if (finished) return;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// Level cleared: save progress and move on.
    public void Complete()
    {
        if (finished) return;
        finished = true;

        int unlocked = PlayerPrefs.GetInt("HighestLevel", 1);
        PlayerPrefs.SetInt("HighestLevel", Mathf.Max(unlocked, levelNumber + 1));
        PlayerPrefs.Save();

        if (levelNumber >= totalLevels) SceneManager.LoadScene("End");
        else SceneManager.LoadScene("Level" + (levelNumber + 1));
    }
}