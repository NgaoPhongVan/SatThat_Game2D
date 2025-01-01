using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private Image backgroundImage; // Ảnh màu đen làm nền
    [SerializeField] private Image fillImage;       // Ảnh màu đỏ hiển thị máu
    [SerializeField] private HealthSystem healthSystem; // Tham chiếu đến HealthSystem

    [Header("Animation Settings")]
    [SerializeField] private float updateSpeed = 5f; // Tốc độ cập nhật thanh máu
    [SerializeField] private bool useSmoothing = true; // Có sử dụng hiệu ứng mượt không

    private float targetFill; // Giá trị fill cần đạt tới

    private void Awake()
    {
        // Nếu không gán healthSystem, tự động tìm trên cùng GameObject
        if (healthSystem == null)
        {
            healthSystem = GetComponentInParent<HealthSystem>();
        }

        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.AddListener(UpdateHealthBar);
        }
        else
        {
            Debug.LogError("No HealthSystem found for HealthBar!");
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

    public void UpdateHealthBar(float healthPercentage)
    {
        if (useSmoothing)
        {
            targetFill = healthPercentage;
        }
        else
        {
            fillImage.fillAmount = healthPercentage;
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }
}