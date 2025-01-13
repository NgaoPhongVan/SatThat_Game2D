using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerForTrap : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private HealthSystem playerHealthSystem;
    [SerializeField] private bool isDead = true;

    // Start is called before the first frame update
    private bool isTriggered = false;

    private void Start()
    {
        playerHealthSystem = player.GetComponent<HealthSystem>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTriggered && isDead)
        {
            isTriggered = true;       
            playerHealthSystem.TakeDamage(100f);
        }
        if (other.CompareTag("Player") && !isDead && !isTriggered)
        {
            playerHealthSystem.TakeDamage(10f);
        }
    }
}
