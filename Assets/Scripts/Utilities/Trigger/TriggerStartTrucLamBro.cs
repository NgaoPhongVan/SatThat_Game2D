using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerStartTrucLamBro : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    

    private bool isTriggered = false;

    [SerializeField] private DialogueManager dialogueManager; // Tham chi?u ??n DialogueManager
    [SerializeField] private Transform playerCharacter;
    private PlayerMovement playerMovement;
    // Start is called before the first frame update
    private bool dialogueTriggered = false; // Tr?ng thái ?ã g?i h?i tho?i

    private void Start()
    {  
        playerMovement = playerCharacter.GetComponent<PlayerMovement>();
    }

    void TriggerDialogue()
    {
        Dialogue dialogue = Resources.Load<Dialogue>("Dialogues/Rung1");
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

        playerMovement.enabled = false;

        dialogueManager.StartDialogue(dialogue); // B?t ??u h?i tho?i
    }

    IEnumerator DelayEndGame() { yield return new WaitForSeconds(0f); TriggerDialogue(); }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered)
        {
            isTriggered = true;

            CameraChap2Controller finalCameraFollow = mainCamera.GetComponent<CameraChap2Controller>();

            if (finalCameraFollow != null)
            {
                finalCameraFollow.ChangeTarget("TrucLam");
                if (!dialogueTriggered) // Ch? g?i m?t l?n
                {
                    dialogueTriggered = true;
                    //TriggerDialogue();
                    StartCoroutine(DelayEndGame());
                    //QuestSystem.QuestManager.Instance.StartQuest("forge_quest");
                    dialogueManager.OnDialogueEnd += OnDialogueCompleted;
                }
            }

            

        }
    }
    

    private void OnDialogueCompleted()
    {
        if (dialogueManager.isCompletedDialouge == true)
        {
            CameraChap2Controller finalCameraFollow = mainCamera.GetComponent<CameraChap2Controller>();
            if (finalCameraFollow != null)
            {
                finalCameraFollow.ChangeTarget("Player");
            }
            QuestSystem.QuestManager.Instance.CompleteQuest("Hide_Enemy");
            QuestSystem.QuestManager.Instance.StartQuest("Rescue_TrucLam");
            playerMovement.enabled = true;
        }
    }
}
