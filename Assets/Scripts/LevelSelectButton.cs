using UnityEngine;
using UnityEngine.UI;

/// Locks a level button until the player has reached that level.
[RequireComponent(typeof(Button))]
public class LevelSelectButton : MonoBehaviour
{
    public int levelNumber;

    void Start()
    {
        int unlocked = PlayerPrefs.GetInt("HighestLevel", 1);
        GetComponent<Button>().interactable = levelNumber <= unlocked;
    }
}