using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DialogSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialogPanel; // Panel chứa toàn bộ UI dialog
    [SerializeField] private TMP_Text dialogText; // Text hiển thị nội dung
    [SerializeField] private TMP_Text nameText; // Text hiển thị tên nhân vật (nếu có)
    [SerializeField] private Image characterImage; // Ảnh nhân vật (nếu có)

    [Header("Dialog Settings")]
    [SerializeField] private float typeSpeed = 0.05f;

    private Queue<DialogLine> dialogLines = new Queue<DialogLine>();
    private bool isTyping = false;
    private bool isDialogActive = false;
    private Coroutine typewriterCoroutine;

    // Struct để chứa thông tin của mỗi dòng đối thoại
    [System.Serializable]
    public struct DialogLine
    {
        public string characterName;
        public string text;
        public Sprite characterSprite;
    }

    void Start()
    {
        // Ẩn dialog panel khi bắt đầu
        dialogPanel.SetActive(false);
    }

    void Update()
    {
        // Nếu nhấn space/click chuột
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                // Nếu đang typing, hiển thị toàn bộ text
                CompleteCurrentLine();
            }
            else if (isDialogActive)
            {
                // Nếu không typing và còn dialog, hiển thị dòng tiếp theo
                ShowNextLine();
            }
        }
    }

    // Phương thức để bắt đầu một đoạn hội thoại mới
    public void StartDialog(DialogLine[] lines)
    {
        isDialogActive = true;
        dialogLines.Clear();

        foreach (DialogLine line in lines)
        {
            dialogLines.Enqueue(line);
        }

        dialogPanel.SetActive(true);
        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (dialogLines.Count == 0)
        {
            EndDialog();
            return;
        }

        DialogLine currentLine = dialogLines.Dequeue();

        // Cập nhật UI
        nameText.text = currentLine.characterName;
        if (currentLine.characterSprite != null)
            characterImage.sprite = currentLine.characterSprite;

        // Bắt đầu hiệu ứng typewriter
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        typewriterCoroutine = StartCoroutine(TypeText(currentLine.text));
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        dialogText.text = text;
        dialogText.maxVisibleCharacters = 0;

        for (int i = 0; i <= text.Length; i++)
        {
            dialogText.maxVisibleCharacters = i;

            if (i < text.Length)
            {
                char currentChar = text[i];
                if (currentChar == '.' || currentChar == '!' || currentChar == '?' || currentChar == ',')
                    yield return new WaitForSeconds(typeSpeed * 4);
                else
                    yield return new WaitForSeconds(typeSpeed);
            }
        }

        isTyping = false;
    }

    private void CompleteCurrentLine()
    {
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        dialogText.maxVisibleCharacters = dialogText.text.Length;
        isTyping = false;
    }

    private void EndDialog()
    {
        isDialogActive = false;
        dialogPanel.SetActive(false);
    }

    // Phương thức để test dialog trong game
    public void TestDialog()
    {
        DialogLine[] testLines = new DialogLine[]
        {
            new DialogLine
            {
                characterName = "NPC",
                text = "Xin chào! Đây là một đoạn hội thoại test."
            },
            new DialogLine
            {
                characterName = "Player",
                text = "Chào bạn! Tôi đang test hệ thống dialog."
            }
        };

        StartDialog(testLines);
    }
}