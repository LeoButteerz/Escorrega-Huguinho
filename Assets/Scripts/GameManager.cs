using UnityEngine;

public class GameManager : MonoBehaviour
{
    public void StartLevel(int levelIndex)
    {
        string levelName = "Level" + levelIndex;

        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
