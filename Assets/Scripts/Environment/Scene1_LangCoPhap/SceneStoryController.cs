using UnityEngine;
using System.Collections;

public class SceneStoryController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject fatherCharacter;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private AudioSource alarmSound;
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private GameObject playerCharacter;

    [Header("Settings")]
    [SerializeField] private float fatherRunSpeed = 5f;
    [SerializeField] private float timeBeforeDestroy = 2f;
    [SerializeField] private Vector2 fatherExitPoint; // Điểm mà cha sẽ chạy đến

    private PlayerMovement playerMovement;
    private bool storyStarted = false;
    private Animator fatherAnimator;

    private void Start()
    {
        fatherAnimator = fatherCharacter.GetComponent<Animator>();
        playerMovement = playerCharacter.GetComponent<PlayerMovement>();
        backgroundMusic.Pause();
        StartCoroutine(StartSceneSequence());
    }

    private IEnumerator StartSceneSequence()
    {
        if (storyStarted) yield break;
        storyStarted = true;
        playerMovement.enabled = false;

        // Đợi một chút trước khi bắt đầu
        yield return new WaitForSeconds(1f);

        // Phát âm thanh tù và
        if (alarmSound != null)
        {
            alarmSound.Play();
        }

        // Đợi âm thanh tù và kết thúc
        yield return new WaitForSeconds(alarmSound != null ? alarmSound.clip.length : 2f);

        // Bắt đầu đoạn hội thoại
        StartDialogue();

        // Đợi hội thoại kết thúc
        while (dialogueManager.IsDialogueActive())
        {
            yield return null;
        }

        playerMovement.enabled = true;
        // Cho cha chạy đi
        StartCoroutine(MoveFatherToExit());
    }

    private void StartDialogue()
    {
        Dialogue dialogue = Resources.Load<Dialogue>("Dialogues/BaoDong");
        if (dialogue != null && dialogueManager != null)
        {
            dialogueManager.StartDialogue(dialogue);
        }
        else
        {
            Debug.LogError("Dialogue or DialogueManager not found!");
        }
    }

    private IEnumerator MoveFatherToExit()
    {
        if (fatherCharacter == null || fatherAnimator == null) yield break;

        // Kích hoạt animation chạy
        fatherAnimator.SetBool("isRunning", true);

        // Di chuyển cha đến điểm thoát
        while (Vector2.Distance(fatherCharacter.transform.position, fatherExitPoint) > 0.1f)
        {
            Vector2 direction = (fatherExitPoint - (Vector2)fatherCharacter.transform.position).normalized;
            fatherCharacter.transform.position += (Vector3)direction * fatherRunSpeed * Time.deltaTime;
            yield return null;
        }

        // Đợi một chút trước khi biến mất
        yield return new WaitForSeconds(timeBeforeDestroy);

        // Ẩn cha đi (không hủy để có thể sử dụng lại sau)
        fatherCharacter.SetActive(false);

        // Bắt đầu phát nhạc nền
        if (backgroundMusic != null)
        {
            backgroundMusic.Play();
        }
    }

    // Phương thức để gọi lại cha (sử dụng khi cần)
    public void ShowFatherAtPosition(Vector2 position)
    {
        if (fatherCharacter != null)
        {
            fatherCharacter.transform.position = position;
            fatherCharacter.SetActive(true);
        }
    }
}