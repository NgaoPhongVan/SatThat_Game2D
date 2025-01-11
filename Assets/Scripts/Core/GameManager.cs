using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Start()
    {
        Debug.Log("Checking Managers...");

        if (InventoryManager.Instance != null)
            Debug.Log("InventoryManager is initialized");
        else
            Debug.LogError("InventoryManager is missing!");

        if (QuestSystem.QuestManager.Instance != null)
            Debug.Log("QuestManager is initialized");
        else
            Debug.LogError("QuestManager is missing!");

        if (SaveSystem.Instance != null)
            Debug.Log("SaveSystem is initialized");
        else
            Debug.LogError("SaveSystem is missing!");
    }

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

    private void Update()
    {
        // Kiểm tra nhấn ESC để tạm dừng game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Chỉ pause khi không ở MainMenu
            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        // Load scene PauseMenu một cách additive
        SceneManager.LoadSceneAsync("PauseMenu", LoadSceneMode.Additive);
    }
}