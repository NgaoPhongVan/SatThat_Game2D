using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public Dictionary<string, int> itemCounts = new Dictionary<string, int>();
    public List<string> items = new List<string>();
    public event System.Action OnInventoryChanged;

    private const string HEALTH_POTION = "Vật phẩm hồi máu";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeInventory();
    }

    public void InitializeInventory()
    {
        itemCounts.Clear();
        items.Clear();
        OnInventoryChanged?.Invoke();
    }

    public void LoadInventoryData(List<string> savedItems, Dictionary<string, int> savedCounts)
    {
        // Clear current inventory
        InitializeInventory();

        // Load saved data
        items = new List<string>(savedItems);
        itemCounts = new Dictionary<string, int>(savedCounts);

        // Notify UI to update
        if (OnInventoryChanged != null)
        {
            OnInventoryChanged.Invoke();
        }
    }

    public void AddHealthPotion()
    {
        // Nếu item chưa có trong danh sách, thêm vào
        if (!items.Contains(HEALTH_POTION))
        {
            items.Add(HEALTH_POTION);
        }

        // Tăng số lượng
        if (!itemCounts.ContainsKey(HEALTH_POTION))
        {
            itemCounts[HEALTH_POTION] = 0;
        }
        itemCounts[HEALTH_POTION]++;

        OnInventoryChanged?.Invoke();
        Debug.Log($"Added health potion. Current count: {itemCounts[HEALTH_POTION]}");
    }

    public bool UseHealthPotion()
    {
        if (itemCounts.ContainsKey(HEALTH_POTION) && itemCounts[HEALTH_POTION] > 0)
        {
            itemCounts[HEALTH_POTION]--;
            Debug.Log($"Used health potion. Remaining: {itemCounts[HEALTH_POTION]}");

            // Chỉ xóa khỏi danh sách items khi số lượng = 0
            if (itemCounts[HEALTH_POTION] == 0)
            {
                items.Remove(HEALTH_POTION);
                itemCounts.Remove(HEALTH_POTION);
            }

            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    public int GetHealthPotionCount()
    {
        return itemCounts.ContainsKey(HEALTH_POTION) ? itemCounts[HEALTH_POTION] : 0;
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