using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ManaSystem : MonoBehaviour
{
    [SerializeField] private float maxMana = 50f;
    private float currentMana;

    public UnityEvent<float> OnManaChanged;

    private Coroutine manaRecoveryCoroutine;

    private void Start()
    {
        currentMana = maxMana;
        OnManaChanged?.Invoke(GetManaPercentage());
    }

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

        // Kích hoạt event hit
        

        if (currentMana <= 0)
        {
            //OnDeath?.Invoke();
        }
    }

    public void Manarecovery()
    {
        currentMana = Mathf.Min(maxMana, currentMana + 3);
        OnManaChanged?.Invoke(GetManaPercentage());
    }

    public void ManaBonus(float manaBonus)
    {
        currentMana = Mathf.Min(maxMana, currentMana + manaBonus);
        OnManaChanged?.Invoke(GetManaPercentage());
    }

    public float GetManaPercentage()
    {
        return currentMana / maxMana;
    }

    public void StartManaRecovery()
    {
        if (manaRecoveryCoroutine == null) // Đảm bảo không chạy trùng Coroutine
        {
            manaRecoveryCoroutine = StartCoroutine(ManaRecoveryRoutine());
        }
    }

    public void StopManaRecovery()
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
            currentMana = Mathf.Min(maxMana, currentMana + 2); // Hồi 2 mana mỗi giây
            OnManaChanged?.Invoke(GetManaPercentage()); // Cập nhật event
            yield return new WaitForSeconds(1f); // Đợi 1 giây trước khi hồi tiếp
        }

        manaRecoveryCoroutine = null; // Kết thúc Coroutine khi mana đầy
    }
}