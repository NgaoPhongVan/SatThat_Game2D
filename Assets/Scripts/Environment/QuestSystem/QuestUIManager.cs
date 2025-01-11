using TMPro;
using UnityEngine;

public class QuestUIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text questText;

    private void Start()
    {
        UpdateCurrentQuest();
    }

    private void Update()
    {
        UpdateCurrentQuest();
    }

    public void UpdateCurrentQuest()
    {
        Quest currentQuest = QuestSystem.QuestManager.Instance.GetCurrentMainQuest();
        if (currentQuest != null)
        {
            Debug.Log($"Current Quest UI - ID: {currentQuest.questId}, Name: {currentQuest.questName}, State: {currentQuest.state}");
            questText.text = $"Nhiệm vụ hiện tại: {currentQuest.questName}\n{currentQuest.description}";
        }
        else
        {
            Debug.Log("No active quest found");
            questText.text = "Không có nhiệm vụ!";
        }
    }
}