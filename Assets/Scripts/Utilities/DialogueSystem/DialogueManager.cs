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
        nextButton.onClick.AddListener(HandleNextButtonClick);
        isDialogueActive = false;
    }

    public void StartDialogue(Dialogue dialogue)
    {
        dialogueUI.SetActive(true);
        isDialogueActive = true;
        dialogueLines.Clear();

        foreach (var line in dialogue.lines)
        {
            dialogueLines.Enqueue(line);
        }

        DisplayNextSentence();
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

        var currentLine = dialogueLines.Dequeue();
        nameText.text = currentLine.characterName;
        characterImage.sprite = currentLine.characterSprite;

        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypeText(currentLine.text));
    }

    private void EndDialogue()
    {
        dialogueUI.SetActive(false);
        isDialogueActive = false;
        Debug.Log("Dialogue ended.");
        OnDialogueEnd?.Invoke();
    }

    // Thêm phương thức kiểm tra trạng thái dialogue
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
}