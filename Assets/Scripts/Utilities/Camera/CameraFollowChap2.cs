using System;
using System.Collections;
using UnityEngine;

public class CameraChap2Controller : MonoBehaviour
{

    private Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);

    //Thanh thay doi
    [SerializeField] private Transform Player; // Player ho?c m?c tiêu
    [SerializeField] private Transform BossTiger;
    [SerializeField] private Transform PointSetUpCamera;
    [SerializeField] private Transform TrucLam;
    [SerializeField] private TrucLamController trucLamController;

    [Header("Position Constraints")]
    [SerializeField] private float minX = 0f;
    [SerializeField] private float maxX = 108f;

    [SerializeField] private DialogueManager dialogueManager; // Tham chi?u ??n DialogueManager
    private PlayerMovement playerMovement;

    [SerializeField] private int scene = 1;
    // Start is called before the first frame update
    private bool dialogueTriggered = false; // Tr?ng thái ?ã g?i h?i tho?i
    void Update()
    {
        if (!dialogueTriggered) // Ch? g?i m?t l?n
        {
            dialogueTriggered = true;
            //TriggerDialogue();
            StartCoroutine(DelayEndGame());
            //QuestSystem.QuestManager.Instance.StartQuest("forge_quest");
            dialogueManager.OnDialogueEnd += OnDialogueCompleted;
        }
    }
    void TriggerDialogue()
    {
        Dialogue dialogue = scene==1 ? Resources.Load<Dialogue>("Dialogues/Chapter2Begin") : Resources.Load<Dialogue>("Dialogues/Chapter2Scene2Begin");

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

    IEnumerator DelayEndGame() { yield return new WaitForSeconds(1f); TriggerDialogue(); }

    private void Start()
    {
        target = Player;
        playerMovement = Player.GetComponent<PlayerMovement>();
        trucLamController = TrucLam.GetComponent<TrucLamController>();
    }


    private void LateUpdate()
    {
        if (target == null) return;

        // Tính toán v? trí m?i cho camera
        Vector3 desiredPosition = target.position + offset;

        // Gi?i h?n v? trí X
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);

        // Làm m??t chuy?n ??ng camera
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    public void ChangeTarget(String a)
    {
        switch (a)
        {
            case "Player":
                target = Player;
                break;
            case "BossTiger":
                target = BossTiger;
                break;
            case "Point":
                target = PointSetUpCamera;
                break;
            case "TrucLam":
                target = TrucLam;
                break;
        }

    }

    public void ChangeSizeCamera(int newSize = 5)
    {
        Camera camera = Camera.main;
        camera.orthographicSize = newSize;
    }

    private void OnDialogueCompleted()
    {
        if (dialogueManager.isCompletedDialouge == true)
        {
            if(scene == 1)
            {
                playerMovement.enabled = true;
            }
            else
            {
                trucLamController.ActiveTrucLam();
                playerMovement.enabled = true;
            }
        }
    }

}

