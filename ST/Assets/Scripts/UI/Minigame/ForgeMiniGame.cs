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

    private void OnDestroy()
    {
        // Đảm bảo hủy đăng ký sự kiện khi object bị hủy
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueEnd -= StartMiniGame;
            dialogueManager.OnDialogueEnd -= EndGame;
        }
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
        miniGameUI.SetActive(false);
        isForeverCompleted = true;

        // Thêm phần thưởng
        InventoryManager.Instance.AddItem("Sổ tay gia truyền");
        InventoryManager.Instance.AddItem("Kim Linh Sơn");
        InventoryManager.Instance.AddItem("Âm Dương Bản");

        // Cập nhật quest
        QuestSystem.QuestManager.Instance.CompleteQuest("forge_quest");
        TriggerDialogue("LoRen3");
        //QuestSystem.QuestManager.Instance.StartQuest("water_forge_quest");
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

    private void TriggerDialogue(string dialogueName)
    {
        Dialogue dialogue = Resources.Load<Dialogue>($"Dialogues/{dialogueName}");
        if (dialogue == null)
        {
            Debug.LogError($"Dialogue {dialogueName} not found!");
            return;
        }
        dialogueManager.StartDialogue(dialogue);
    }
}