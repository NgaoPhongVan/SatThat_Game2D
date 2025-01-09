using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Dialogue System/Dialogue")]
public class Dialogue : ScriptableObject
{
    [System.Serializable]
    public struct DialogLine
    {
        public string characterName;
        [TextArea] public string text;
        public Sprite characterSprite;
    }

    public DialogLine[] lines;
}