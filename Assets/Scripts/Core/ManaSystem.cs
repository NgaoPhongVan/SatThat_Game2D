using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ManaSystem : MonoBehaviour
{
    [SerializeField] private float maxMana = 50f;
    private float currentMana;

    public UnityEvent<float> OnManaChanged;
    public UnityEvent OutOfMana;

    private Coroutine manaRecoveryCoroutine;

    private void Start()
    {
        currentMana = maxMana;
        OnManaChanged?.Invoke(GetManaPercentage());
    }
    
    public float getCurrentMana() {  return currentMana; }

    public void UseBuff(float manaUsed)
    {
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        float finalMana = manaUsed;

        if (playerMovement != null)
        {
            // Áp dụng giảm sát thương nếu đang block
            finalMana *= playerMovement.GetDamageReduction();
        }

        currentMana = Mathf.Max(0, currentMana - finalMana);
        OnManaChanged?.Invoke(GetManaPercentage());

        if (currentMana <= 0)
        {
            OutOfMana?.Invoke(); // Kích hoạt sự kiện hết mana
        }

        // Kiểm tra và khởi động hồi phục mana nếu cần
        if (manaRecoveryCoroutine == null)
        {
            StartManaRecovery();
        }
    }

    public void ManaBonus(float manaBonus)
    {
        currentMana = Mathf.Min(maxMana, currentMana + manaBonus);
        OnManaChanged?.Invoke(GetManaPercentage());

        // Kiểm tra nếu đầy mana thì dừng hồi phục
        if (currentMana >= maxMana && manaRecoveryCoroutine != null)
        {
            StopManaRecovery();
        }
    }

    public float GetManaPercentage()
    {
        return currentMana / maxMana;
    }

    private void StartManaRecovery()
    {
        if (manaRecoveryCoroutine == null) // Đảm bảo không chạy trùng Coroutine
        {
            manaRecoveryCoroutine = StartCoroutine(ManaRecoveryRoutine());
        }
    }

    private void StopManaRecovery()
    {
        if (manaRecoveryCoroutine != null)
        {
            StopCoroutine(manaRecoveryCoroutine);
            manaRecoveryCoroutine = null;
        }
    }

    private IEnumerator ManaRecoveryRoutine()
    {
        while (currentMana < maxMana) // Chỉ hồi mana nếu chưa đầy
        {
            currentMana = Mathf.Min(maxMana, currentMana + 0.5f); // Hồi 2 mana mỗi giây
            OnManaChanged?.Invoke(GetManaPercentage()); // Cập nhật event
            yield return new WaitForSeconds(1f); // Đợi 1 giây trước khi hồi tiếp
        }

        StopManaRecovery(); // Kết thúc Coroutine khi mana đầy
    }
}
