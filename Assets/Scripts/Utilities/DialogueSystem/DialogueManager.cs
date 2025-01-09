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
    // Thêm delegate và sự kiện callback
    public delegate void DialogueEndCallback();
    public event DialogueEndCallback OnDialogueEnd;
    private void Start()
    {
        dialogueLines = new Queue<Dialogue.DialogLine>();
        dialogueUI.SetActive(false);
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(HandleNextButtonClick);
    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialogueUI.SetActive(true);
        dialogueLines.Clear();

        // Thêm từng dòng thoại vào hàng đợi
        foreach (var line in dialogue.lines)
        {
            dialogueLines.Enqueue(line);
        }

        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        // Nếu đang hiển thị từ từ, hoàn thành đoạn hiện tại ngay lập tức
        if (isTyping)
        {
            CompleteCurrentLine();
            return;
        }

        // Nếu không còn câu thoại nào, kết thúc hội thoại
        if (dialogueLines.Count == 0)
        {
            EndDialogue();
            return;
        }

        // Lấy dòng thoại tiếp theo từ hàng đợi
        var currentLine = dialogueLines.Dequeue();

        // Cập nhật tên nhân vật và hình ảnh
        nameText.text = currentLine.characterName;
        characterImage.sprite = currentLine.characterSprite;

        // Bắt đầu hiệu ứng typewriter cho nội dung thoại
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypeText(currentLine.text));
    }


    private void CompleteCurrentLine()
    {
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }

        // Hiển thị toàn bộ text còn lại
        dialogueText.maxVisibleCharacters = dialogueText.text.Length;
        isTyping = false;
    }

    private void HandleNextButtonClick()
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


    private void EndDialogue()
    {
        dialogueUI.SetActive(false);
        Debug.Log("Dialogue ended.");

        // Gọi sự kiện kết thúc hội thoại
        OnDialogueEnd?.Invoke();
    }
}