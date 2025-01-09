// PlayerCombat.cs - Gắn vào GameObject Player
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private HealthSystem healthSystem;
    private float horizontalInput;

    void Start()
    {
        // Lấy reference tới component HealthSystem trên cùng GameObject
        healthSystem = GetComponent<HealthSystem>();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("nhay");
            // Nhận 20 sát thương
            healthSystem.Heal(10f);
        }
    }
    

    // Xử lý va chạm
    private void OnCollisionEnter2D(Collision2D collision)
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            // Nếu va chạm với enemy
        

            //// Nếu va chạm với item hồi máu
            //if (collision.gameObject.CompareTag("HealthPotion"))
            //{
            //    // Hồi 10 máu
            //    healthSystem.Heal(10f);
            //    // Xóa item
            //    Destroy(collision.gameObject);
            //}
        }
}