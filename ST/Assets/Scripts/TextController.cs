using UnityEngine;
using TMPro;
using System.Collections;

// Đặt cả 2 class trong cùng một file để dễ quản lý
public class TextController : MonoBehaviour
{
    [SerializeField] private string textToShow = "Hello! This is a test...";
    [SerializeField] private float typeSpeed = 0.05f;

    private TMP_Text tmpText;
    private Coroutine typewriterCoroutine;

    void Start()
    {
        tmpText = GetComponent<TMP_Text>();
        StartTyping(textToShow);
    }

    public void StartTyping(string text)
    {
        // Dừng coroutine cũ nếu có
        if (typewriterCoroutine != null)
            StopCoroutine(typewriterCoroutine);

        // Reset và bắt đầu typing mới
        tmpText.text = text;
        tmpText.maxVisibleCharacters = 0;
        typewriterCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        // Đợi một frame để đảm bảo text đã được set
        yield return null;

        int totalCharacters = tmpText.textInfo.characterCount;
        Debug.Log($"Starting to type {totalCharacters} characters");

        for (int i = 0; i <= totalCharacters; i++)
        {
            tmpText.maxVisibleCharacters = i;
            Debug.Log($"Character {i} revealed");

            // Thêm delay dài hơn cho dấu câu
            if (i < totalCharacters)
            {
                char currentChar = tmpText.text[i];
                if (currentChar == '.' || currentChar == '!' || currentChar == '?' || currentChar == ',')
                    yield return new WaitForSeconds(typeSpeed * 4);
                else
                    yield return new WaitForSeconds(typeSpeed);
            }
        }

        Debug.Log("Finished typing");
    }

    // Phương thức này để test trong Editor
    public void TestNewText(string newText)
    {
        StartTyping(newText);
    }
}