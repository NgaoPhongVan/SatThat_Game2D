using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject healthBarUI;
    [SerializeField] private GameObject questUI;
    [SerializeField] private GameObject inventoryUI;
    // Thêm các UI khác nếu cần

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

    public void ShowGameUI(bool show)
    {
        if (healthBarUI != null) healthBarUI.SetActive(show);
        if (questUI != null) questUI.SetActive(show);
        if (inventoryUI != null) inventoryUI.SetActive(show);
    }
}