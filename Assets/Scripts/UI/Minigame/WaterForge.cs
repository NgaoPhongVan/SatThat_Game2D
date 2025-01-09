using UnityEngine;
using UnityEngine.SceneManagement;

public class WaterForge : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private string nextSceneName;
    private bool dialogueTriggered = false;

    //private void Start()
    //{
    //    // Đăng ký sự kiện kết thúc dialogue
    //    dialogueManager.OnDialogueEnd += OnDialogueComplete;
    //}
    void OnMouseDown()
    {
        // Kiểm tra xem nhiệm vụ rèn kiếm đã hoàn thành chưa
        if (QuestSystem.QuestManager.Instance.CanStartQuest("water_forge_quest"))
        {
            if (!dialogueTriggered)
            {
                dialogueTriggered = true;
                TriggerDialogue();
            }
        }
        else
        {
            Debug.Log("Bạn cần hoàn thành nhiệm vụ rèn kiếm trước!");
            // Có thể hiển thị thông báo cho người chơi
        }
    }

    void TriggerDialogue()
    {
        Dialogue dialogue = Resources.Load<Dialogue>("Dialogues/ToiLuyen");

        if (dialogue == null)
        {
            Debug.LogError("Dialogue not found! Check the file name and path in Resources.");
            return;
        }

        if (dialogueManager == null)
        {
            Debug.LogError("DialogueManager is not assigned!");
            return;
        }

        dialogueManager.StartDialogue(dialogue);
    }

    private void OnDialogueComplete()
    {
        // Hủy đăng ký sự kiện để tránh gọi nhiều lần
        dialogueManager.OnDialogueEnd -= OnDialogueComplete;

        // Hoàn thành nhiệm vụ
        QuestSystem.QuestManager.Instance.CompleteQuest("water_forge_quest");

        // Chờ một chút trước khi chuyển scene
        StartCoroutine(DelayedSceneTransition());
    }

    private System.Collections.IEnumerator DelayedSceneTransition()
    {
        yield return new WaitForSeconds(2f); // Đợi 2 giây
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene("Scene1_LangCoPhap");
        }
        else
        {
            Debug.LogWarning("Next scene name is not set!");
        }
    }

    private void OnDestroy()
    {
        // Đảm bảo hủy đăng ký sự kiện khi object bị hủy
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueEnd -= OnDialogueComplete;
        }
    }
}