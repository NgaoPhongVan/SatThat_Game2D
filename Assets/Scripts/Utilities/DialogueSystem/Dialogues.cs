using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogues : MonoBehaviour
{
    [SerializeField] private DialogueManager dialogueManager; // Tham chiếu đến DialogueManager
    [SerializeField] private string text;
    // Start is called before the first frame update
    private bool dialogueTriggered = false; // Trạng thái đã gọi hội thoại
    void Update()
    {
        if (!dialogueTriggered) // Chỉ gọi một lần
        {
            dialogueTriggered = true;
            TriggerDialogue();
            QuestSystem.QuestManager.Instance.StartQuest("forge_quest");
        }
    }
    void TriggerDialogue()
    {
        Dialogue dialogue = Resources.Load<Dialogue>("Dialogues/" + text);

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

        dialogueManager.StartDialogue(dialogue); // Bắt đầu hội thoại
    }

    //IEnumerator DelayEndGame() { yield return new WaitForSeconds(2f); TriggerDialogue(); }
}
