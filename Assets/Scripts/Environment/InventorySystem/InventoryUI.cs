using TMPro;
using UnityEngine;
using System.Collections;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private TMP_Text itemListText;
    private PlayerMovement player;
    private HealthSystem playerHealth;
    private ManaSystem playerMana;
    private Animator playerAnimator;
    private bool isHealing = false;
    private bool isManaRecovery = false;

    private void Start()
    {
        inventoryPanel.SetActive(false);
        InventoryManager.Instance.OnInventoryChanged += UpdateItemList;

        // Lấy reference đến Player và components
        player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            playerHealth = player.GetComponent<HealthSystem>();
            playerAnimator = player.GetComponent<Animator>();
            playerMana = player.GetComponent<ManaSystem>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }

        // Kiểm tra có thể hồi máu không
        if (Input.GetKeyDown(KeyCode.Alpha1) && !isHealing)
        {
            TryUseHealthPotion();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2) && !isManaRecovery)
        {
            TryUseManaPotion();
        }
    }

    private void TryUseHealthPotion()
    {
        // Kiểm tra có vật phẩm không
        if (InventoryManager.Instance.GetHealthPotionCount() <= 0)
        {
            PickupTextManager.Instance.ShowPickupText("Bạn không còn vật phẩm hồi máu");
            return;
        }

        // Bắt đầu hồi máu
        StartCoroutine(HealingSequence());
    }

    private IEnumerator HealingSequence()
    {
        if (playerAnimator != null && playerHealth != null)
        {
            // Kích hoạt animation healing
            playerAnimator.SetTrigger("healing");

            // Đợi animation hoàn thành (điều chỉnh thời gian phù hợp với độ dài animation)
            float healingDuration = 0.55f; // Độ dài của animation healing
            yield return new WaitForSeconds(healingDuration);

            // Hồi máu và trừ vật phẩm
            if (InventoryManager.Instance.UseHealthPotion())
            {
                float maxHealth = 100f;
                float healAmount = maxHealth * 0.2f;
                playerHealth.Heal(healAmount);
            }

            // Đảm bảo isHealing được reset
            if (player != null)
            {
                player.OnHealingComplete();
            }
        }
    }

    private void TryUseManaPotion()
    {
        // Kiểm tra có vật phẩm không
        if (InventoryManager.Instance.GetManaPotionCount() <= 0)
        {
            PickupTextManager.Instance.ShowPickupText("Bạn không còn vật phẩm hồi mana");
            return;
        }

        // Bắt đầu hồi máu
        StartCoroutine(ManaRecoverySequence());
    }

    private IEnumerator ManaRecoverySequence()
    {
        if (playerAnimator != null && playerHealth != null)
        {
            // Kích hoạt animation healing
            playerAnimator.SetTrigger("manaRecovering");

            // Đợi animation hoàn thành (điều chỉnh thời gian phù hợp với độ dài animation)
            float manaRecoveryDuration = 0.55f; // Độ dài của animation healing
            yield return new WaitForSeconds(manaRecoveryDuration);

            // Hồi máu và trừ vật phẩm
            if (InventoryManager.Instance.UseManaPotion())
            {
                float maxMana = 50f;
                float manaAmount = maxMana * 0.2f;
                playerMana.ManaBonus(manaAmount);
            }

            // Đảm bảo isHealing được reset
            if (player != null)
            {
                player.OnHealingComplete();
            }
        }
    }

    private void UpdateItemList()
    {
        string itemList = "";
        foreach (string item in InventoryManager.Instance.items)
        {
            if (item == "Vật phẩm hồi máu")
            {
                int count = InventoryManager.Instance.GetHealthPotionCount();
                itemList += $"* {item} (+{count})\n";
            }
            else
            {
                itemList += $"* {item}\n";
            }
        }
        itemListText.text = itemList;
    }

    private void ToggleInventory()
    {
        bool isActive = inventoryPanel.activeSelf;
        inventoryPanel.SetActive(!isActive);

        if (!isActive)
        {
            UpdateItemList();
        }
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= UpdateItemList;
        }
    }
}