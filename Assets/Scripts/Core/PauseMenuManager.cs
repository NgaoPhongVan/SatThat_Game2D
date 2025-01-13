using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 0.5f;
        // Ẩn game UI khi pause
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowGameUI(false);
        }
    }

    public void ContinueGame()
    {
        // Hiện lại game UI khi continue
        if (GameUIManager.Instance != null)
        {
            GameUIManager.Instance.ShowGameUI(true);
        }
        Time.timeScale = 1f;
        SceneManager.UnloadSceneAsync("PauseMenu");
    }

    public void SaveAndQuit()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveGame();
        }
        Time.timeScale = 1f;
        Application.Quit();
    }
}