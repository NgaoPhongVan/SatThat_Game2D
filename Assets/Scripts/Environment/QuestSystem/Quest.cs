using System.Collections.Generic;

[System.Serializable]
public class Quest
{
    public string questId;        // Định danh duy nhất cho quest
    public string questName;      // Tên hiển thị
    public string description;    // Mô tả nhiệm vụ
    public QuestType questType;   // Loại nhiệm vụ
    public QuestState state;      // Trạng thái hiện tại
    public List<string> objectives; // Các mục tiêu cần hoàn thành
    public Dictionary<string, int> requirements; // Yêu cầu để hoàn thành (ví dụ: thu thập 3 item X)
    public List<string> rewards;  // Phần thưởng khi hoàn thành

    public Quest(string questId, string questName, string description)
    {
        this.questId = questId;
        this.questName = questName;
        this.description = description;
        this.state = QuestState.NotStarted;
        this.objectives = new List<string>();
        this.requirements = new Dictionary<string, int>();
        this.rewards = new List<string>();
    }
}

public enum QuestType
{
    MainQuest,
    SideQuest,
    Tutorial,
    Hidden
}

public enum QuestState
{
    NotStarted,
    InProgress,
    Completed,
    Failed
}