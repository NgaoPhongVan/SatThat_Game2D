// SaveSystem.cs
using UnityEngine;
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
            inventoryItems = InventoryManager.Instance.items
        };

        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream stream = new FileStream(savePath, FileMode.Create))
        {
            formatter.Serialize(stream, saveData);
        }
        Debug.Log("Game Saved!");
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(savePath, FileMode.Open))
            {
                GameSaveData saveData = formatter.Deserialize(stream) as GameSaveData;

                // Khôi phục dữ liệu
                QuestSystem.QuestManager.Instance.LoadQuestProgress(saveData.completedQuests);
                foreach (string item in saveData.inventoryItems)
                {
                    InventoryManager.Instance.AddItem(item);
                }
            }
            Debug.Log("Game Loaded!");
        }
    }
}