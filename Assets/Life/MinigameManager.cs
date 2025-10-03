// ���ϸ�: MinigameManager.cs (���� ���׷��̵� ����)
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class MinigameManager : MonoBehaviour
{
    [Header("��ȭ UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("�̴ϰ��� UI")]
    [SerializeField] private GameObject minigameUI;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI tapCountText;

    [Header("���� UI")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private TextMeshProUGUI rewardText;

    [Header("��ȭ ����")]
    [SerializeField, TextArea(3, 10)] private string[] storyDialogue;
    [SerializeField, TextArea(3, 10)] private string[] instructionDialogue;

    [Header("�̴ϰ��� ����")]
    [SerializeField] private float gameDuration = 10f;

    private int finalTapCount = 0;

    void Start()
    {
        // ���� �� ��� UI�� �� ���·� ���
        dialoguePanel.SetActive(false);
        minigameUI.SetActive(false);
        rewardPanel.SetActive(false);
    }

    // DialogueTrigger�� �� �Լ��� ȣ���Ͽ� ��ü �������� ����
    public void StartEventSequence()
    {
        StartCoroutine(FullSequenceCoroutine());
    }

    // ��ü �帧�� �����ϴ� ���� �ڷ�ƾ
    private IEnumerator FullSequenceCoroutine()
    {
        yield return StartCoroutine(ShowDialogue(storyDialogue));
        yield return StartCoroutine(ShowDialogue(instructionDialogue));
        yield return StartCoroutine(MinigameCoroutine());

        // LifeGameManager�� ProcessMinigameResult�� ���� ȣ��
        if (LifeGameManager.Instance != null)
        {
            LifeGameManager.Instance.ProcessMinigameResult(finalTapCount);
        }

        ProcessAndShowReward();
    }

    // ��ȭâ�� ���� �����ϴ� �ڷ�ƾ
    private IEnumerator ShowDialogue(string[] dialogueLines)
    {
        dialoguePanel.SetActive(true);
        foreach (var line in dialogueLines)
        {
            dialogueText.text = line;
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return));
            yield return null;
        }
        dialoguePanel.SetActive(false);
    }

    // �̴ϰ��� ���� �ڷ�ƾ
    private IEnumerator MinigameCoroutine()
    {
        minigameUI.SetActive(true);
        float currentTime = gameDuration;
        int tapCount = 0;
        tapCountText.text = $"Hit Count: {tapCount}";

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            timerText.text = $"Remaining Time: {currentTime:F0}";
            if (Input.GetKeyDown(KeyCode.Space))
            {
                tapCount++;
                tapCountText.text = $"Hit Count: {tapCount}";
            }
            yield return null;
        }
        finalTapCount = tapCount;
        minigameUI.SetActive(false);
    }

    // ���� ���� ó�� �� ǥ��
    private void ProcessAndShowReward()
    {
        string rewardMessage = "No rewards have been obtained.";
        if (LifeGameManager.Instance != null)
        {
            float buffValue = LifeGameManager.Instance.regenBuffValue;
            if (buffValue >= 1.0f) rewardMessage = "I can feel the overflowing power of life!\n[Heal] Get buffs (Strong) (Boss play)";
            else if (buffValue >= 0.5f) rewardMessage = "I won the tears of an old tree!\n[Heal] Get buffs(Weak)(Boss play)";
            else if (buffValue > 0f) rewardMessage = "I barely felt the energy of life.";
        }

        rewardText.text = rewardMessage;
        rewardPanel.SetActive(true);
    }

    // ���� â�� "���ư���" ��ư�� ������ �Լ�
    public void OnClickReturnButton()
    {
        if (LifeGameManager.Instance != null && !string.IsNullOrEmpty(LifeGameManager.Instance.sceneNameBeforePortal))
        {
            // PlayerSpawner�� ������� �ʰ�, LifeGameManager�� ������ ���� �̵�
            SceneManager.LoadScene(LifeGameManager.Instance.sceneNameBeforePortal);
            // ���� �ε�� �� �÷��̾� ��ġ�� �����ϴ� ������ �ʿ� (��: PlayerStartPoint ��ũ��Ʈ)
        }
    }
}
