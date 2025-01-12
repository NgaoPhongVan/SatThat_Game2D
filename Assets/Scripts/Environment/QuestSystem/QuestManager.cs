using System.Collections.Generic;
using UnityEngine;

namespace QuestSystem
{
    using System.Collections.Generic;
    using UnityEngine;

    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }
        private Dictionary<string, Quest> allQuests = new Dictionary<string, Quest>();
        private List<Quest> activeQuests = new List<Quest>();
        private HashSet<string> completedQuestIds = new HashSet<string>();

        public event System.Action<Quest> OnQuestStarted;
        public event System.Action<Quest> OnQuestCompleted;
        public event System.Action<Quest> OnQuestFailed;
        public event System.Action<Quest> OnQuestUpdated;

        // Định nghĩa thứ tự các nhiệm vụ
        private readonly string[] questSequence = new string[]
        {
           "forge_quest",           // Nhiệm vụ rèn kiếm
           "water_forge_quest",     // Nhiệm vụ tôi luyện kiếm
           "Clear_EnemyPatrol",
           "Hide_Enemy"
        };

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeQuests();
        }

        private void InitializeQuests()
        {
            Debug.Log("Initializing Quests");

            // Khởi tạo quest rèn kiếm
            Quest forgeQuest = new Quest(
                "forge_quest",
                "Rèn Kiếm",
                "Hoàn thành mini-game rèn kiếm để tiếp tục. (Nhấn vào đe)"
            );
            forgeQuest.questType = QuestType.MainQuest;
            allQuests.Add(forgeQuest.questId, forgeQuest);
            Debug.Log($"Added quest: {forgeQuest.questId}");

            // Khởi tạo quest tôi luyện kiếm
            Quest waterForgeQuest = new Quest(
                "water_forge_quest",
                "Tôi luyện kiếm trong nước",
                "Hoàn thành mini-game tôi luyện kiếm trong nước. (Nhấn vào thùng nước)"
            );
            waterForgeQuest.questType = QuestType.MainQuest;
            allQuests.Add(waterForgeQuest.questId, waterForgeQuest);
            Debug.Log($"Added quest: {waterForgeQuest.questId}");

            Quest ClearEnemyPatrol = new Quest(
                "Clear_EnemyPatrol",
                "Tiêu diệt toán lính của địch trong làng",
                "Tìm và tiêu diệt kẻ địch trong làng."
            );
            ClearEnemyPatrol.questType = QuestType.MainQuest;
            allQuests.Add(ClearEnemyPatrol.questId, ClearEnemyPatrol);

            Quest HideEnemy = new Quest(
                "Hide_Enemy",
                "Tiến hành lẩn trốn trong bóng tối và tiêu diệt thủ lĩnh",
                "Ẩn nấp tìm và tiêu diệt kẻ địch trong làng."
            );
            HideEnemy.questType = QuestType.MainQuest;
            allQuests.Add(HideEnemy.questId, HideEnemy);
        }

        // Kiểm tra xem một nhiệm vụ có thể bắt đầu hay không
        public bool CanStartQuest(string questId)
        {
            // Kiểm tra quest có tồn tại
            if (!allQuests.ContainsKey(questId)) return false;

            // Kiểm tra quest đã hoàn thành chưa
            if (completedQuestIds.Contains(questId)) return false;

            int questIndex = System.Array.IndexOf(questSequence, questId);

            // Nếu là nhiệm vụ đầu tiên
            if (questIndex == 0) return true;

            // Kiểm tra nhiệm vụ trước đã hoàn thành chưa
            string previousQuestId = questSequence[questIndex - 1];
            return completedQuestIds.Contains(previousQuestId);
        }

        // Bắt đầu một nhiệm vụ mới
        public void StartQuest(string questId)
        {
            if (!CanStartQuest(questId)) return;

            Quest quest = allQuests[questId];
            quest.state = QuestState.InProgress;
            activeQuests.Add(quest);
            OnQuestStarted?.Invoke(quest);
            Debug.Log($"Started quest: {quest.questName}");
        }

        // Hoàn thành một nhiệm vụ
        public void CompleteQuest(string questId)
        {
            Debug.Log($"Attempting to complete quest: {questId}");
            if (!allQuests.ContainsKey(questId))
            {
                Debug.LogError($"Quest {questId} not found in allQuests dictionary");
                return;
            }

            Quest quest = allQuests[questId];
            quest.state = QuestState.Completed;
            completedQuestIds.Add(questId);
            activeQuests.Remove(quest);
            OnQuestCompleted?.Invoke(quest);

            Debug.Log($"Completed quest: {quest.questName}");

            // Debug current quest state
            Debug.Log("Current completed quests:");
            foreach (var completedId in completedQuestIds)
            {
                Debug.Log($"- {completedId}");
            }
        }

        // Thất bại một nhiệm vụ
        public void FailQuest(string questId)
        {
            if (!allQuests.ContainsKey(questId)) return;

            Quest quest = allQuests[questId];
            quest.state = QuestState.Failed;
            activeQuests.Remove(quest);
            OnQuestFailed?.Invoke(quest);
            Debug.Log($"Failed quest: {quest.questName}");
        }

        // Lấy nhiệm vụ chính hiện tại
        public Quest GetCurrentMainQuest()
        {
            return activeQuests.Find(q => q.questType == QuestType.MainQuest);
        }

        // Lấy danh sách các ID nhiệm vụ đã hoàn thành
        public List<string> GetCompletedQuestIds()
        {
            return new List<string>(completedQuestIds);
        }

        // Tải tiến trình nhiệm vụ
        public void LoadQuestProgress(List<string> completedQuests)
        {
            completedQuestIds = new HashSet<string>(completedQuests);

            // Reset tất cả quest về trạng thái ban đầu
            foreach (var quest in allQuests.Values)
            {
                quest.state = QuestState.NotStarted;
            }

            // Đánh dấu các quest đã hoàn thành
            foreach (var questId in completedQuests)
            {
                if (allQuests.ContainsKey(questId))
                {
                    allQuests[questId].state = QuestState.Completed;
                }
            }

            // Tìm và bắt đầu quest tiếp theo chưa hoàn thành
            foreach (string questId in questSequence)
            {
                if (CanStartQuest(questId))
                {
                    StartQuest(questId);
                    break;
                }
            }
        }

        // Kiểm tra xem một nhiệm vụ đã hoàn thành chưa
        public bool IsQuestCompleted(string questId)
        {
            return completedQuestIds.Contains(questId);
        }

        // Lấy trạng thái của một nhiệm vụ
        public QuestState GetQuestState(string questId)
        {
            if (allQuests.TryGetValue(questId, out Quest quest))
            {
                return quest.state;
            }
            return QuestState.NotStarted;
        }

        public void ResetAllQuests()
        {
            completedQuestIds.Clear();
            activeQuests.Clear();

            // Reset all quests to NotStarted state
            foreach (var quest in allQuests.Values)
            {
                quest.state = QuestState.NotStarted;
            }

            // Start the first quest if available
            if (questSequence.Length > 0)
            {
                StartQuest(questSequence[0]);
            }
        }
    }
}