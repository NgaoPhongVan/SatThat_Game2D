using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    [SerializeField] private Image backgroundImage; // Ảnh nền của thanh máu
    [SerializeField] private Image fillImage;       // Ảnh hiển thị máu
    [SerializeField] private HealthSystem healthSystem; // Tham chiếu đến HealthSystem của Boss

    [Header("Animation Settings")]
    [SerializeField] private float updateSpeed = 5f; // Tốc độ cập nhật thanh máu
    [SerializeField] private bool useSmoothing = true; // Sử dụng hiệu ứng mượt không

    private float targetFill; // Giá trị fill cần đạt tới

    private void Awake()
    {
        // Tìm HealthSystem của Boss trên cùng GameObject nếu chưa được gán
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
            Debug.LogError("No HealthSystem found for Boss HealthBar!");
        }
    }

    private void Update()
    {
        // Nếu có hiệu ứng mượt, cập nhật fillImage mượt mà từ giá trị hiện tại tới targetFill
        if (useSmoothing && fillImage.fillAmount != targetFill)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * updateSpeed);
        }
    }

    // Cập nhật giá trị thanh máu
    //public void UpdateHealthBar(float healthPercentage)
    //{
    //    if (useSmoothing)
    //    {
    //        targetFill = healthPercentage;
    //    }
    //    else
    //    {
    //        fillImage.fillAmount = healthPercentage;
    //    }
    //}

    public void UpdateHealthBar(float healthPercentage)
    {
        Debug.Log("Updating Boss Health: " + healthPercentage);  // Log giá trị máu
        fillImage.fillAmount = healthPercentage;
    }
    public void UpdateFillAmount(float fillAmount)
    {
        fillImage.fillAmount = fillAmount;
    }

    public float GetFillAmount()
    {
        return fillImage.fillAmount;
    }

    private void OnDestroy()
    {
        // Gỡ sự kiện khi HealthBar bị hủy
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }
}
