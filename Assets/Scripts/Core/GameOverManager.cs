using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    private void Start()
    {
        // Đảm bảo game không bị pause
        Time.timeScale = 1f;
    }

    public void RetryGame()
    {
        // Lấy scene cuối cùng từ PlayerPrefs và load lại
        string lastScene = PlayerPrefs.GetString("LastPlayedScene");
        SceneManager.LoadScene(lastScene);
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