using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public List<string> items = new List<string>();
    public event System.Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddItem(string itemName)
    {
        if (!items.Contains(itemName))
        {
            items.Add(itemName);
            OnInventoryChanged?.Invoke();
            Debug.Log($"Added item: {itemName}");
        }
    }

    public void RemoveItem(string itemName)
    {
        if (items.Contains(itemName))
        {
            items.Remove(itemName);
            OnInventoryChanged?.Invoke();
            Debug.Log($"Removed item: {itemName}");
        }
    }

    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }
}