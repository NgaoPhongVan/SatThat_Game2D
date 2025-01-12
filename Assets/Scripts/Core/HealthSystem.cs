using UnityEngine.Events;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDeath;
    public UnityEvent OnHit;

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(GetHealthPercentage());
    }

    public void SetHealth(float healthPercentage)
    {
        // Đảm bảo giá trị nằm trong khoảng 0-100
        healthPercentage = Mathf.Clamp(healthPercentage, 0f, 100f);

        // Chuyển đổi từ phần trăm sang giá trị thực
        currentHealth = (healthPercentage / 100f) * maxHealth;

        // Thông báo thay đổi máu
        OnHealthChanged?.Invoke(GetHealthPercentage());

        // Kiểm tra nếu máu về 0
        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }

        Debug.Log($"Health set to {healthPercentage}% ({currentHealth}/{maxHealth})");
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(GetHealthPercentage());

        OnHit?.Invoke();

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(float healAmount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        OnHealthChanged?.Invoke(GetHealthPercentage());
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    // Thêm getter cho maxHealth nếu cần
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    // Thêm getter cho currentHealth nếu cần
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}