using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text dialogueText; // Text hiển thị nội dung
    [SerializeField] private TMP_Text nameText; // Text hiển thị tên nhân vật
    [SerializeField] private Image characterImage; // Ảnh nhân vật
    [SerializeField] private GameObject dialogueUI; // Panel hội thoại
    [SerializeField] private Button nextButton; // Nút chuyển câu

    [Header("Typewriter Settings")]
    [SerializeField] private float typeSpeed = 0.05f; // Tốc độ hiệu ứng typewriter

    private Queue<Dialogue.DialogLine> dialogueLines; // Hàng đợi các dòng thoại
    private bool isTyping = false; // Kiểm tra trạng thái gõ chữ
    private Coroutine typewriterCoroutine;
    private bool isDialogueActive = false; // Thêm biến theo dõi trạng thái dialogue

    // Thêm delegate và sự kiện callback
    public delegate void DialogueEndCallback();
    public event DialogueEndCallback OnDialogueEnd;

    private void Start()
    {
        dialogueLines = new Queue<Dialogue.DialogLine>();
        dialogueUI.SetActive(false);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(HandleNext); // Gắn sự kiện cho nút
        isDialogueActive = false;
    }

    private void Update()
    {
        // Kiểm tra phím Tab
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            HandleNext();
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialogueUI.SetActive(true);    // Hiển thị UI hội thoại
        isDialogueActive = true;       // Đánh dấu đang trong hội thoại
        dialogueLines.Clear();         // Xóa các dòng hội thoại cũ

        // Thêm từng dòng hội thoại vào queue
        foreach (var line in dialogue.lines)
        {
            dialogueLines.Enqueue(line);
        }

        DisplayNextSentence();         // Hiển thị câu đầu tiên
    }

    public void DisplayNextSentence()
    {
        if (isTyping)
        {
            CompleteCurrentLine();  
            return;
        }

        if (dialogueLines.Count == 0)
        {
            EndDialogue();  
            return;
        }

        // Lấy và hiển thị dòng tiếp theo
        var currentLine = dialogueLines.Dequeue();
        nameText.text = currentLine.characterName;           // Cập nhật tên
        characterImage.sprite = currentLine.characterSprite; // Cập nhật hình ảnh

        // Dừng hiệu ứng đánh chữ cũ nếu có
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        // Bắt đầu hiệu ứng đánh chữ mới
        typewriterCoroutine = StartCoroutine(TypeText(currentLine.text));
    }

    private void EndDialogue()
    {
        dialogueUI.SetActive(false);
        isDialogueActive = false;
        Debug.Log("Dialogue ended.");
        OnDialogueEnd?.Invoke();
    }

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    private void CompleteCurrentLine()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        dialogueText.maxVisibleCharacters = dialogueText.text.Length;
        isTyping = false;
    }

    // Xử lý khi nhấn nút hoặc phím Tab
    private void HandleNext()
    {
        if (isTyping)
        {
            // Nếu đang typing, hiển thị toàn bộ text còn lại
            CompleteCurrentLine();
        }
        else
        {
            // Nếu đã hiển thị xong, chuyển sang câu tiếp theo
            DisplayNextSentence();
        }
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i < text.Length && isTyping; i++)
        {
            dialogueText.maxVisibleCharacters++;

            char currentChar = text[i];
            if (currentChar == '.' || currentChar == ',' || currentChar == '!' || currentChar == '?')
            {
                yield return new WaitForSeconds(typeSpeed * 3);
            }
            else
            {
                yield return new WaitForSeconds(typeSpeed);
            }
        }

        isTyping = false;
    }
}
