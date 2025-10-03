// ���ϸ�: GameManager.cs (���� �Ϸ�)
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections; // �ڡڡ� ������ �κ�: �ڷ�ƾ�� ����ϱ� ���� �ʼ�! �ڡڡ�

public class LifeGameManager : MonoBehaviour
{
    public static LifeGameManager Instance { get; private set; }

    public int finalTapCount = 0;
    public float regenBuffValue = 0f;
    public Vector3 playerPositionBeforePortal;
    public string sceneNameBeforePortal;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // �ڡڡ� ������ �κ�: OnSceneLoaded �Լ��� ���� �ڷ�ƾ�� '����'��Ű�� ���Ҹ� �մϴ�. �ڡڡ�
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ���� ���ƿ� ���� ����� �� �̸��� ���ٸ�
        if (!string.IsNullOrEmpty(sceneNameBeforePortal) && scene.name == sceneNameBeforePortal)
        {
            // ���� ���� �۾��� ���� �ʰ�, �ڷ�ƾ�� ���۽�ŵ�ϴ�.
            StartCoroutine(RestoreSceneState());
        }
    }

    // �ڡڡ� ���� �߰��� �ڷ�ƾ �Լ� �ڡڡ�
    // ���� ���� �۾��� �� �Լ����� �� ������ �ڿ� �����ϰ� �̷�����ϴ�.
    IEnumerator RestoreSceneState()
    {
        // �� �� �����Ӹ� ��ٸ��ϴ�.
        // �� �ð� ���� ParallaxBackground ���� �ٸ� ��ũ��Ʈ���� �ʱ�ȭ�� �ð��� �ݴϴ�.
        yield return null;

        // --- ���� OnSceneLoaded�� �ִ� ���� �ڵ尡 �� ������ ���Խ��ϴ� ---
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = playerPositionBeforePortal;
        }

        if (Camera.main != null)
        {
            Camera.main.cullingMask = -1; // ī�޶� ���� ����
        }

        // ����� ���� ������ �ʱ�ȭ�մϴ�.
        sceneNameBeforePortal = null;
    }

    public void ProcessMinigameResult(int tapCount)
    {
        finalTapCount = tapCount;
        if (tapCount < 30) regenBuffValue = 0f;
        else if (tapCount < 50) regenBuffValue = 0.5f;
        else if (tapCount < 100) regenBuffValue = 1.0f;
        else regenBuffValue = 2.0f;
        Debug.Log($"�̴ϰ��� ����! ���� ��Ÿ: {finalTapCount}, ��� ����: {regenBuffValue}hp/sec");
    }
}