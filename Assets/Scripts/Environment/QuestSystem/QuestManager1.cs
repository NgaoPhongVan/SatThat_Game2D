using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; } // Singleton Instance

    [System.Serializable]
    public class Quest
    {
        public string questName; // Tên nhiệm vụ
        public string description; // Mô tả nhiệm vụ
        public bool isCompleted; // Trạng thái hoàn thành
    }

    public List<Quest> quests = new List<Quest>(); // Danh sách nhiệm vụ

    private void Awake()
    {
        // Đảm bảo chỉ có một phiên bản QuestManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Xóa phiên bản mới nếu đã tồn tại
            return;
        }

        Instance = this; // Gán phiên bản Singleton
        DontDestroyOnLoad(gameObject); // Giữ qua các scene
    }

    // Thêm nhiệm vụ mới
    public void AddQuest(string questName, string description = "")
    {
        if (quests.Exists(q => q.questName == questName))
        {
            Debug.LogWarning($"Quest '{questName}' already exists!");
            return;
        }

        Quest newQuest = new Quest
        {
            questName = questName,
            description = description,
            isCompleted = false
        };
        quests.Add(newQuest);
        Debug.Log($"Added new quest: {questName}");
    }

    // Đánh dấu nhiệm vụ hoàn thành
    public void CompleteQuest(string questName)
    {
        Quest quest = quests.Find(q => q.questName == questName);
        if (quest != null)
        {
            quest.isCompleted = true;
            Debug.Log($"Quest '{questName}' completed!");
        }
        else
        {
            Debug.LogWarning($"Quest '{questName}' not found!");
        }
    }

    // Lấy nhiệm vụ hiện tại (nhiệm vụ chưa hoàn thành đầu tiên)
    public string GetCurrentQuest()
    {
        Quest currentQuest = quests.Find(q => !q.isCompleted);
        return currentQuest != null ? currentQuest.questName : "Không có nhiệm vụ!";
    }
}