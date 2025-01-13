using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    private string savePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        savePath = Path.Combine(Application.persistentDataPath, "gamesave.dat");
    }

    public void SaveGame()
    {
        GameSaveData saveData = new GameSaveData
        {
            completedQuests = QuestSystem.QuestManager.Instance.GetCompletedQuestIds(),
            inventoryItems = InventoryManager.Instance.items,
            itemCounts = InventoryManager.Instance.itemCounts,
            currentSceneName = SceneManager.GetActiveScene().name
        };

        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(savePath, FileMode.Create))
        {
            formatter.Serialize(stream, saveData);
        }
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(savePath, FileMode.Open))
            {
                GameSaveData saveData = formatter.Deserialize(stream) as GameSaveData;

                // Khôi phục dữ liệu quest
                QuestSystem.QuestManager.Instance.LoadQuestProgress(saveData.completedQuests);

                // Khôi phục inventory
                InventoryManager.Instance.LoadInventoryData(saveData.inventoryItems, saveData.itemCounts);

                // Load scene đã lưu
                if (!string.IsNullOrEmpty(saveData.currentSceneName))
                {
                    SceneManager.LoadScene(saveData.currentSceneName);
                }
            }
        }
        else
        {
            Debug.LogWarning("No save file found!");
        }
    }

    public bool HasSaveFile()
    {
        return File.Exists(savePath);
    }
}