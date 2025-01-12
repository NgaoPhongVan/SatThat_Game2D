using System.Collections;
using TMPro.Examples;
using UnityEngine;

public class FatherDeathCutsceneController : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private GameObject fatherCharacter;
    [SerializeField] private GameObject playerCharacter;
    [SerializeField] private GameObject miniBoss;

    [Header("Components")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private BoxCollider2D triggerArea;
    [SerializeField] private Vector2 fatherPosition;

    private PlayerMovement playerMovement;
    private HealthSystem fatherHealth;
    private Transform originalCameraTarget;
    private bool cutsceneTriggered = false;

    private void Start()
    {
        playerMovement = playerCharacter.GetComponent<PlayerMovement>();
        fatherHealth = fatherCharacter.GetComponent<HealthSystem>();

        // Lưu target gốc của camera (thường là player)
        if (cameraController != null)
        {
            originalCameraTarget = cameraController.target;
        }

        // Ban đầu ẩn cha đi
        //fatherCharacter.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !cutsceneTriggered)
        {
            cutsceneTriggered = true;
            StartCoroutine(PlayCutscene());
        }
    }

    private IEnumerator PlayCutscene()
    {
        // Vô hiệu hóa điều khiển của player
        playerMovement.enabled = false;

        // Hiện cha tại vị trí đã định
        fatherCharacter.transform.position = fatherPosition;
        fatherCharacter.SetActive(true);

        // Set máu cha còn ít
        if (fatherHealth != null)
        {
            fatherHealth.SetHealth(20f);
        }

        // Chuyển camera sang MiniBoss
        if (cameraController != null && miniBoss != null)
        {
            cameraController.ChangeTarget(miniBoss.transform);
        }

        yield return new WaitForSeconds(1f);

        // Phát dialogue
        Dialogue dialogue = Resources.Load<Dialogue>("Dialogues/DoiTruong");
        if (dialogue != null)
        {
            dialogueManager.StartDialogue(dialogue);
            while (dialogueManager.IsDialogueActive())
            {
                yield return null;
            }
        }

        // Kích hoạt cha để có thể bị tấn công
        SetFatherVulnerable();

        // Đợi cha chết
        while (fatherHealth != null && fatherHealth.GetHealthPercentage() > 0)
        {
            yield return null;
        }
        fatherCharacter.layer = LayerMask.NameToLayer("NPC");
        fatherCharacter.tag = "Untagged";
        // Xử lý cái chết của cha
        yield return new WaitForSeconds(1f);
        Destroy(fatherCharacter);

        // Chuyển camera về player
        if (cameraController != null && originalCameraTarget != null)
        {
            cameraController.ChangeTarget(originalCameraTarget);
        }

        yield return new WaitForSeconds(1f);

        // Kích hoạt lại điều khiển của player
        playerMovement.enabled = true;
    }

    private void SetFatherVulnerable()
    {
        if (fatherCharacter != null)
        {
            fatherCharacter.layer = LayerMask.NameToLayer("NPC");
            fatherCharacter.tag = "NPC";
            Debug.Log("Father is now vulnerable to attacks");
        }
    }
}