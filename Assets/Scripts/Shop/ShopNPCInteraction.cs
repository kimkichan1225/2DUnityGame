using UnityEngine;
using TMPro; // TextMeshPro 사용

public class ShopNPCInteraction : MonoBehaviour
{
    [Header("대화 내용")]
    [SerializeField, TextArea(5, 10)] // Inspector에서 여러 줄로 편하게 입력
    private string shopExplanationDialogue = "안녕하세요! 이곳은 상점입니다.\n발판 위에 있는 물건에 다가가 W키를 누르면 구매할 수 있습니다.\n오른쪽의 회전하는 받침대에서는 골드를 소모하여 상품 목록을 새로고침할 수 있습니다.";
    // ↑↑↑ 여기에 원하는 설명 텍스트를 입력하세요. \n은 줄바꿈입니다.

    [Header("UI 연결")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject interactionPrompt; // "[W] 대화하기" 같은 UI (선택사항)

    private bool isPlayerNear = false;
    private bool isDialogueActive = false;
    private PlayerController playerController; // 플레이어 이동 제어용

    void Start()
    {
        // 시작 시 UI 비활성화
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        // 플레이어가 근처에 있고, 아직 대화 중이 아니며, W키를 누르면
        if (isPlayerNear && !isDialogueActive && Input.GetKeyDown(KeyCode.W))
        {
            StartDialogue();
        }
        // 대화가 활성화된 상태에서 W키를 다시 누르면
        else if (isDialogueActive && Input.GetKeyDown(KeyCode.W))
        {
            EndDialogue();
        }
    }

    private void StartDialogue()
    {
        isDialogueActive = true;

        // 플레이어 찾기 및 이동 멈춤
        playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            playerController.canMove = false;
        }

        // 상호작용 안내 UI 숨기기
        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        // 대화창 띄우고 텍스트 설정
        if (dialoguePanel != null && dialogueText != null)
        {
            dialogueText.text = shopExplanationDialogue;
            dialoguePanel.SetActive(true);
        }
    }

    private void EndDialogue()
    {
        isDialogueActive = false;

        // 대화창 숨기기
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // 플레이어 이동 다시 허용
        if (playerController != null)
        {
            playerController.canMove = true;
        }

        // 플레이어가 여전히 근처에 있다면 상호작용 안내 UI 다시 표시
        if (isPlayerNear && interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
        }
    }

    // 플레이어가 감지 범위에 들어왔을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (interactionPrompt != null && !isDialogueActive) // 대화 중이 아닐 때만 표시
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    // 플레이어가 감지 범위에서 나갔을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            // 플레이어가 멀어지면 대화 강제 종료 및 안내 UI 숨기기
            if (isDialogueActive)
            {
                EndDialogue();
            }
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
}
