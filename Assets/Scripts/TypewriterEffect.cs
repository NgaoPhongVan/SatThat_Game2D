using System;
using System.Collections;
using UnityEngine;
using TMPro;
using Object = UnityEngine.Object;

public class TypewriterEffect : MonoBehaviour
{
    private TMP_Text _textBox;
    private int _currentVisibleCharacterIndex;
    private Coroutine _typewriterCoroutine;
    private bool _readyForNewText = true;  // Set this to true by default

    private WaitForSeconds _simpleDelay;
    private WaitForSeconds _interpunctuationDelay;

    [Header("Typewriter Settings")]
    [SerializeField] private float charactersPerSecond = 15f;
    [SerializeField] private float interpunctuationDelay = 0.5f;

    private void Awake()
    {
        _textBox = GetComponent<TMP_Text>();
        _textBox.maxVisibleCharacters = 0;

        _simpleDelay = new WaitForSeconds(1f / charactersPerSecond);
        _interpunctuationDelay = new WaitForSeconds(interpunctuationDelay);
    }

    private void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(PrepareForNewText);
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(PrepareForNewText);
    }

    private void PrepareForNewText(Object obj)
    {
        Debug.Log("PrepareForNewText called");

        if (obj != _textBox)
        {
            Debug.Log("Different text object, ignoring");
            return;
        }

        // Stop any existing coroutine
        if (_typewriterCoroutine != null)
        {
            StopCoroutine(_typewriterCoroutine);
        }

        // Reset everything
        _currentVisibleCharacterIndex = 0;
        _textBox.maxVisibleCharacters = 0;

        // Start new typewriter effect
        _typewriterCoroutine = StartCoroutine(TypewriterRoutine());
        Debug.Log("Started new typewriter routine");
    }

    private IEnumerator TypewriterRoutine()
    {
        // Wait one frame to ensure text is properly set
        yield return null;

        int totalCharacters = _textBox.textInfo.characterCount;
        Debug.Log($"Total characters to reveal: {totalCharacters}");

        for (_currentVisibleCharacterIndex = 0;
             _currentVisibleCharacterIndex <= totalCharacters;
             _currentVisibleCharacterIndex++)
        {
            _textBox.maxVisibleCharacters = _currentVisibleCharacterIndex;

            if (_currentVisibleCharacterIndex < totalCharacters)
            {
                char currentChar = _textBox.text[_currentVisibleCharacterIndex];
                if (currentChar == '.' || currentChar == '?' || currentChar == '!' ||
                    currentChar == ',' || currentChar == ';')
                {
                    yield return _interpunctuationDelay;
                }
                else
                {
                    yield return _simpleDelay;
                }
            }
        }

        Debug.Log("Finished revealing text");
    }
}