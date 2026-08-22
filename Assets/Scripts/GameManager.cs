using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject nextLevel;
    public GameObject replay;

    public void StartLevel(int levelIndex)
    {
        string levelName = "Level" + levelIndex;

        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void WinMenu()
    {
        nextLevel.SetActive(true);
    }

    public void LoseMenu()
    {
        replay.SetActive(true);
    }

    public void MainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
    }
}
