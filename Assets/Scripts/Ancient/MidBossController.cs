// 파일명: MidBossController.cs (수정됨)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MidBossController : MonoBehaviour
{
    // --- 기존 변수들 ---
    // static 리스트는 게임 세션 동안 유지됩니다.
    public static List<string> completedEventIDs = new List<string>();
    [Tooltip("이 보스 이벤트의 고유 ID")]
    public string eventID = "DefeatedMidBoss"; // Inspector에서 각 보스마다 다른 ID를 설정할 수 있습니다.

    [Header("이벤트 연동")]
    [Tooltip("처치 후 활성화시킬 석상/포탈 오브젝트")]
    public StatueInteraction targetPortal;
    [Tooltip("보스 처치 후 나타날 대화 내용")]
    [TextArea(3, 10)]
    public string[] deathDialogue;

    [Header("사망 후 이벤트 추가")]
    [Tooltip("처치 후 비활성화할 벽 게임 오브젝트")]
    [SerializeField] private GameObject wallToDisable;
    [Tooltip("처치 후 활성화할 화살표 (부모) 게임 오브젝트")]
    [SerializeField] private GameObject arrowsToEnable;

    // --- ▼▼▼▼▼ 수정된 부분 (Start 함수 추가/수정) ▼▼▼▼▼ ---
    void Start()
    {
        // 이 보스의 eventID가 이미 완료 목록(completedEventIDs)에 있는지 확인합니다.
        if (completedEventIDs.Contains(eventID))
        {
            // 이미 완료된 보스라면,
            Debug.Log($"보스 이벤트 '{eventID}'는 이미 완료되었습니다. 오브젝트를 비활성화합니다.");

            // 관련된 벽과 화살표도 즉시 처리합니다 (선택사항, 이미 비활성화/활성화 되어있을 수 있음)
            if (wallToDisable != null) wallToDisable.SetActive(false);
            if (arrowsToEnable != null) arrowsToEnable.SetActive(true);

            // 보스 게임 오브젝트 자체를 비활성화합니다.
            gameObject.SetActive(false);
        }
    }
    // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---

    // 이 함수는 MonsterHealth의 DieRoutine에서 호출됩니다.
    public void TriggerDeathEvent()
    {
        // 이벤트가 이미 완료되었다면 실행하지 않음
        if (completedEventIDs.Contains(eventID)) return;
        // 완료 목록에 이 보스의 ID를 추가하여 '기억'시킵니다.
        completedEventIDs.Add(eventID);

        // --- 기존 코드 (포탈 활성화) ---
        if (targetPortal != null)
        {
            Debug.Log("중간 보스 처치! 포탈을 활성화합니다.");
            targetPortal.UnlockPortal();
        }
        else
        {
            Debug.LogWarning("MidBossController에 활성화할 포탈(targetPortal)이 연결되지 않았습니다!");
        }
        // ---

        // --- 기존 코드 (벽 비활성화 & 화살표 활성화) ---
        if (wallToDisable != null)
        {
            wallToDisable.SetActive(false);
            Debug.Log(wallToDisable.name + " 벽이 비활성화되었습니다.");
        }
        else
        {
            Debug.LogWarning("MidBossController에 비활성화할 벽(wallToDisable)이 연결되지 않았습니다!");
        }
        if (arrowsToEnable != null)
        {
            arrowsToEnable.SetActive(true);
            Debug.Log(arrowsToEnable.name + " 화살표가 활성화되었습니다.");
        }
        else
        {
            Debug.LogWarning("MidBossController에 활성화할 화살표(arrowsToEnable)가 연결되지 않았습니다!");
        }
        // ---

        // --- 기존 코드 (사망 대화 출력) ---
        if (DialogueController.Instance != null && deathDialogue.Length > 0)
        {
            DialogueController.Instance.StartDialogue(deathDialogue, null);
        }
        // ---
    }
}
