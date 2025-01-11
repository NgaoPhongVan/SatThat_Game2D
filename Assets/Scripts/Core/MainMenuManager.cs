using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject windowControlsPanel;
    private string savePath;

    private void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, "gamesave.dat");
        CheckSaveFileExists();

        // Kiểm tra nếu đang chạy trên Windows standalone
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if (windowControlsPanel != null)
                windowControlsPanel.SetActive(true);
        }
        else
        {
            if (windowControlsPanel != null)
                windowControlsPanel.SetActive(false);
        }
    }

    public void StartNewGame()
    {
        // Reset tất cả managers
        ResetAllManagers();

        // Xóa file save cũ
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
        }

        SceneManager.LoadScene("Scene1_LoRen");
    }

    private void ResetAllManagers()
    {
        // Reset Inventory
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.InitializeInventory();
        }

        // Reset Quests
        if (QuestSystem.QuestManager.Instance != null)
        {
            QuestSystem.QuestManager.Instance.ResetAllQuests();
        }
    }

    public void ContinueGame()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadGame();
        }
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }

    private void CheckSaveFileExists()
    {
        GameObject continueButton = GameObject.Find("ContinueButton");
        if (continueButton != null)
        {
            continueButton.SetActive(File.Exists(savePath));
        }
    }
}