using UnityEngine;
using TMPro;

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
            questText.text = $"Nhiệm vụ hiện tại: {currentQuest.questName}\n{currentQuest.description}";
        }
        else
        {
            questText.text = "Không có nhiệm vụ!";
        }
    }
}