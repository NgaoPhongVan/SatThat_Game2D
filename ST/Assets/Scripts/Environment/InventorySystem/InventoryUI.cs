using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TMP_Text itemListText;

    private void Start()
    {
        inventoryPanel.SetActive(false); // Ẩn panel lúc đầu
        InventoryManager.Instance.OnInventoryChanged += UpdateItemList;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("Phím I đã được nhấn");
            ToggleInventory();
        }
    }

    private void ToggleInventory()
    {
        bool isActive = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!isActive);

        if (!isActive) // Nếu vừa mở inventory
        {
            UpdateItemList();
        }
    }

    private void UpdateItemList()
    {
        string itemList = "";
        foreach (string item in InventoryManager.Instance.items)
        {
            itemList += $"* {item}\n";
        }
        itemListText.text = itemList;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateItemList;
        }
    }
}