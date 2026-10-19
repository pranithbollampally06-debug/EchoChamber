using UnityEngine;
using UnityEngine.SceneManagement;

/// Button handlers for the menu scenes. Hook these up to UI Button OnClick events.
public class MenuManager : MonoBehaviour
{
    public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);

    /// Loads LevelN, e.g. LoadLevel(3) opens "Level3".
    public void LoadLevel(int n) => SceneManager.LoadScene("Level" + n);

    public void Quit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // so Quit works in the editor too
#endif
    }
}