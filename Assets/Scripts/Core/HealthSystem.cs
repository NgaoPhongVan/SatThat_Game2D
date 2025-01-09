using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDeath;
    public UnityEvent OnHit; // Thêm event mới cho hit

    private void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(GetHealthPercentage());
    }

    public void TakeDamage(float damage)
    {
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        float finalDamage = damage;

        if (playerMovement != null)
        {
            // Áp dụng giảm sát thương nếu đang block
            finalDamage *= playerMovement.GetDamageReduction();
        }

        currentHealth = Mathf.Max(0, currentHealth - finalDamage);
        OnHealthChanged?.Invoke(GetHealthPercentage());

        // Kích hoạt event hit
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
}