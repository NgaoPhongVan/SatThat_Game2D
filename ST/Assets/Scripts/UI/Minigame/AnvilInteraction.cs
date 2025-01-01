using UnityEngine;

public class AnvilInteraction : MonoBehaviour
{
    [SerializeField] private ForgeMiniGame miniGame; // Tham chiếu mini-game

    private void OnMouseDown()
    {
        Debug.Log("Anvil clicked!");
        miniGame.StartGame();
    }
}