using UnityEngine;

public class GiantClam : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private int requiredPearls = 3;
    [SerializeField] private GameObject portalToActivate;
    [SerializeField] private GameObject interactionPrompt;

    [Header("오브젝트 설정")]
    [SerializeField] private GameObject closedClamObject; // 닫힌 상태의 자식 오브젝트
    [SerializeField] private GameObject openClamObject;   // 열린 상태의 자식 오브젝트

    private bool playerIsNear = false;
    private bool isOpen = false;

    void Start()
    {
        // 시작할 때, 닫힌 조개는 켜고 열린 조개는 끈 상태로 만듭니다.
        if (closedClamObject != null) closedClamObject.SetActive(true);
        if (openClamObject != null) openClamObject.SetActive(false);

        if (portalToActivate != null) portalToActivate.SetActive(false);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (playerIsNear && Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            if (PearlDisplayUI.instance.UsePearls(requiredPearls))
            {
                Open();
            }
            else
            {
                Debug.Log("진주가 부족하여 조개를 열 수 없습니다.");
            }
        }
    }

    private void Open()
    {
        isOpen = true;
        Debug.Log("대왕조개가 열립니다!");

        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        // --- 변경된 부분: 닫힌 오브젝트를 끄고, 열린 오브젝트를 켭니다. ---
        if (closedClamObject != null) closedClamObject.SetActive(false);
        if (openClamObject != null) openClamObject.SetActive(true);
        // --------------------------------------------------------

        if (portalToActivate != null)
        {
            portalToActivate.SetActive(true);
            Debug.Log("포탈이 활성화되었습니다!");
        }

        GetComponent<Collider2D>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            playerIsNear = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNear = false;
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
        }
    }
}
