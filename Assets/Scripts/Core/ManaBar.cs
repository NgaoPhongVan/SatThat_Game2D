using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    [Header("Mana Bar Settings")]
    [SerializeField] private Image backgroundImage; // Ảnh màu đen làm nền
    [SerializeField] private Image fillImage;       // Ảnh màu xanh hiển thị mana
    [SerializeField] private ManaSystem manaSystem;

    [Header("Animation Settings")]
    [SerializeField] private float updateSpeed = 5f; // Tốc độ cập nhật thanh máu
    [SerializeField] private bool useSmoothing = true; // Có sử dụng hiệu ứng mượt không

    private float targetFill; // Giá trị fill cần đạt tới

    private void Awake()
    {
        // Nếu không gán healthSystem, tự động tìm trên cùng GameObject
        if (manaSystem == null)
        {
            manaSystem = GetComponentInParent<ManaSystem>();
        }

        if (manaSystem != null)
        {
            manaSystem.OnManaChanged.AddListener(UpdateManaBar);
        }
        else
        {
            Debug.LogError("No ManaSystem found for ManaBar!");
        }
    }

    private void Update()
    {
        if (useSmoothing && fillImage.fillAmount != targetFill)
        {
            // Cập nhật mượt thanh máu
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * updateSpeed);
        }
    }

    public void UpdateManaBar(float manaPercentage)
    {
        if (useSmoothing)
        {
            targetFill = manaPercentage;
        }
        else
        {
            fillImage.fillAmount = manaPercentage;
        }
    }

    private void OnDestroy()
    {
        if (manaSystem != null)
        {
            manaSystem.OnManaChanged.RemoveListener(UpdateManaBar);
        }
    }
}