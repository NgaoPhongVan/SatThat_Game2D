// GameSaveData.cs
using System.Collections.Generic;

[System.Serializable]
public class GameSaveData
{
    public List<string> completedQuests = new List<string>();
    public List<string> inventoryItems = new List<string>();
}