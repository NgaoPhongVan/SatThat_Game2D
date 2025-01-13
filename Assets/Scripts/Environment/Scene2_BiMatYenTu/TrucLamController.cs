using System.Collections;
using UnityEngine;

public class TrucLamController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    public Transform activePoint;
    public Transform pointToDead;
    public float speed = 2f;
    public Rigidbody2D rb;
    public Transform player;

    // Animation
    private Animator animator;
    private bool isDeath = false;

    // Health Bar
    private HealthSystem healthSystem;

    // Active
    private bool hasReachedActivePoint = false;
    private bool isActived = false;
    private bool activeDialogue = false;

    [SerializeField] private DialogueManager dialogueManager; // Tham chi?u ??n DialogueManager
    [SerializeField] private Transform Player;
    private PlayerMovement playerMovement;

    // Start is called before the first frame update
    private bool dialogueTriggered = false; // Tr?ng thái ?ã g?i h?i tho?i
   
    void TriggerDialogue()
    {
        Dialogue dialogue = Resources.Load<Dialogue>("Dialogues/Chapter2Scene2End") ;

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

    private void OnDialogueCompleted()
    {
        if (dialogueManager.isCompletedDialouge == true)
        {
            healthSystem.TakeDamage(100f);
            CameraChap2Controller finalCameraFollow = mainCamera.GetComponent<CameraChap2Controller>();
            Destroy(gameObject);
            playerMovement.enabled = true;
            if (finalCameraFollow != null)
            {
                finalCameraFollow.ChangeTarget("Player");
            }

        }
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Kiểm tra xem player hoặc activePoint có bị thiếu hay không
        if (player == null || activePoint == null)
        {
            Debug.LogError("Player or ActivePoint is not assigned in the Inspector!");
        }

        playerMovement = Player.GetComponent<PlayerMovement>();

        healthSystem = GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.AddListener(CheckHealth);
            healthSystem.OnDeath.AddListener(OnDeath);
        }
        if (healthSystem == null)
        {
            Debug.LogError("HealthSystem component is missing on player!");
        }
    }

    private void FixedUpdate()
    {
        if (isDeath) return;

        if (isActived)
        {
            MoveToActivePoint();
        }

        if (hasReachedActivePoint)
        {
            transform.position = pointToDead.position;
        }

        if (activeDialogue)
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
    }

    private void CheckHealth(float healthPercentage)
    {
        if (healthPercentage <= 0 && !isDeath)
        {
            OnDeath();
        }
    }

    public void OnDeath()
    {
        if (isDeath) return;
        isDeath = true;
    }

    public void ActiveTrucLam()
    {
        isActived = true;
    }

    public void ActiveDialogue()
    {
        healthSystem.TakeDamage(80f);
        activeDialogue = true;
    }

    public void MoveToActivePoint()
    {
        if (!hasReachedActivePoint)
        {
            float distanceToActivePoint = Vector3.Distance(transform.position, activePoint.position);

            // Thay vì chỉ kiểm tra > 1f, hãy dùng khoảng cách đủ nhỏ để dừng
            if (distanceToActivePoint > 1f)  // Điều chỉnh khoảng cách nhỏ hơn
            {
                // Di chuyển Trúc Lâm bằng Rigidbody2D
                Vector3 direction = activePoint.position - transform.position;
                transform.position += direction.normalized * speed * Time.deltaTime;

                animator.SetBool("isRunning", true);
                return;
            }
            else
            {
                // Khi đã đến gần activePoint
                transform.position = activePoint.position;// Đảm bảo đối tượng dừng chính xác tại điểm

                animator.SetBool("isRunning", false);

                hasReachedActivePoint = true; // Cập nhật trạng thái
                return;
            }
        }
    }


    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.RemoveListener(CheckHealth);
            healthSystem.OnDeath.RemoveListener(OnDeath);
        }
    }
}
