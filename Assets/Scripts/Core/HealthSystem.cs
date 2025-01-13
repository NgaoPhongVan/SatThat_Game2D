using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    public float currentHealth;

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
        currentHealth = Mathf.Max(0, currentHealth - damage);
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