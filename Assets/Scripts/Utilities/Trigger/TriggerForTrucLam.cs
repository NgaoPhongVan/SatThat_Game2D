using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerForTrucLam : MonoBehaviour
{
    private bool isTriggered = false;

    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private string text;
    // Start is called before the first frame update
    private bool dialogueTriggered = false; 

    void TriggerDialogue()
    {
        Dialogue dialogue = Resources.Load<Dialogue>("Dialogues/Rung2");
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

        dialogueManager.StartDialogue(dialogue); // B?t ??u h?i tho?i
    }

    IEnumerator DelayEndGame() { yield return new WaitForSeconds(0f); TriggerDialogue(); }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            
            isTriggered = true;
            if (!dialogueTriggered) // Ch? g?i m?t l?n
            {
                dialogueTriggered = true;
                //TriggerDialogue();
                StartCoroutine(DelayEndGame());
                dialogueManager.OnDialogueEnd += OnDialogueCompleted;
            }
            
        }
    }


    private void OnDialogueCompleted()
    {
        
        if (dialogueManager.isCompletedDialouge == true)
        {
            if (QuestSystem.QuestManager.Instance.CanStartQuest("Rescue_TrucLam"))
            {
              
                QuestSystem.QuestManager.Instance.CompleteQuest("Rescue_TrucLam");
                QuestSystem.QuestManager.Instance.StartQuest("Destroy_BossTiger");
                Destroy(gameObject, 1f);
            }
            else
            {
                PickupTextManager.Instance.ShowPickupText("B?n c?n hoàn thành nhi?m v? rèn ki?m tr??c!");
                Debug.Log("B?n c?n hoàn thành nhi?m v? rèn ki?m tr??c!");
            }
        }
    }
}
