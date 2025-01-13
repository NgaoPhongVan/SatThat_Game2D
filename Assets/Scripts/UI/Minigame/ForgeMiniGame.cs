using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForgeMiniGame : MonoBehaviour
{
    [SerializeField] private GameObject miniGameUI;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text retryText;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private string[] rewardItems;

    private float progressSpeed = 1f;
    private float perfectZoneStart = 0.4f;
    private float perfectZoneEnd = 0.6f;
    private bool isPlaying = false;
    private float timer = 0f;
    private AudioSource hammerSound;
    private int attempts = 3;

    // Biến để theo dõi trạng thái hoàn thành vĩnh viễn của minigame
    private static bool isForeverCompleted = false;
    private bool isDialogueEventRegistered = false;

    void Start()
    {
        hammerSound = GetComponent<AudioSource>();
        //QuestSystem.QuestManager.Instance.StartQuest("forge_quest"); // Sử dụng quest ID
    }

    private void Update()
    {
        if (!isPlaying)
        {
            // Chỉ cho phép nhấn R để chơi lại nếu minigame chưa hoàn thành vĩnh viễn
            if (Input.GetKeyDown(KeyCode.R) && !isForeverCompleted)
            {
                StartMiniGame();
            }
            return;
        }

        timer += Time.deltaTime * progressSpeed;
        progressBar.value = Mathf.PingPong(timer, 1f);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            CheckTiming();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelGame();
        }
    }

    public void StartGame()
    {
        // Kiểm tra nếu minigame đã hoàn thành vĩnh viễn
        if (isForeverCompleted)
        {
            // Chuyển thẳng tới hội thoại sau minigame
            TriggerDialogue("LoRen3");
            return;
        }

        // Đăng ký sự kiện nếu chưa được đăng ký
        if (!isDialogueEventRegistered)
        {
            dialogueManager.OnDialogueEnd += StartMiniGame;
            isDialogueEventRegistered = true;
        }

        // Bắt đầu với hội thoại trước mini-game
        TriggerDialogue("LoRen2");
    }

    private void StartMiniGame()
    {
        // Hủy đăng ký để tránh gọi nhiều lần
        if (isDialogueEventRegistered)
        {
            dialogueManager.OnDialogueEnd -= StartMiniGame;
            isDialogueEventRegistered = false;
        }

        // Nếu đã hoàn thành vĩnh viễn, không cho chơi lại
        if (isForeverCompleted)
        {
            return;
        }

        miniGameUI.SetActive(true);
        isPlaying = true;
        timer = 0f;
        progressBar.value = 0f;
    }

    private void CancelGame()
    {
        isPlaying = false;
        miniGameUI.SetActive(false);
        attempts = 3;
    }

    private void EndGame()
    {
        Debug.Log("EndGame called");

        // Thêm phần thưởng trước
        InventoryManager.Instance.AddItem("Sổ tay gia truyền");
        InventoryManager.Instance.AddItem("Kim Linh Sơn");
        InventoryManager.Instance.AddItem("Âm Dương Bản");

        PickupTextManager.Instance.ShowPickupText("Bạn đã vừa có vật phẩm mới (Nhấn I để mở)");

        // Cập nhật quest trước khi tắt UI
        var questManager = QuestSystem.QuestManager.Instance;
        if (questManager != null)
        {
            questManager.CompleteQuest("forge_quest");
            questManager.StartQuest("water_forge_quest");
        }
        else
        {
            Debug.LogError("QuestManager is null!");
        }

        // Trigger dialogue và tắt UI sau cùng
        StartCoroutine(DelayDialog());
        TriggerDialogue("LoRen3");
        miniGameUI.SetActive(false);
        isForeverCompleted = true;
    }

    private void TriggerDialogue(string dialogueId)
    {
        // Tìm DialogueManager trong scene
        var dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            Dialogue dialogue = Resources.Load<Dialogue>("Dialogues/" + dialogueId);
            if (dialogue != null)
            {
                dialogueManager.StartDialogue(dialogue);
            }
            else
            {
                Debug.LogError($"Dialogue {dialogueId} not found!");
            }
        }
        else
        {
            Debug.LogError("DialogueManager not found in scene!");
        }
    }

    private void CheckTiming()
    {
        float progress = progressBar.value;
        hammerSound.Play();

        if (progress >= perfectZoneStart && progress <= perfectZoneEnd)
        {
            attempts--;
            retryText.text = "Hoàn hảo!";

            if (attempts <= 0)
            {
                retryText.text = "Thành công!";
                StartCoroutine(DelayEndGame());
            }
            else
            {
                // Reset cho lượt tiếp theo
                timer = 0f;
                progressBar.value = 0f;
            }
        }
        else
        {
            attempts = 3;
            retryText.text = "Thất bại! Nhấn R để thử lại!";
            isPlaying = false;
        }
    }

    private IEnumerator DelayEndGame()
    {
        yield return new WaitForSeconds(2f);
        EndGame();
    }
    private IEnumerator DelayDialog()
    {
        yield return new WaitForSeconds(2f);
    }

    private void OnDestroy()
    {
        // Đảm bảo hủy đăng ký sự kiện khi object bị hủy
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueEnd -= StartMiniGame;
            dialogueManager.OnDialogueEnd -= EndGame;
        }
    }
}