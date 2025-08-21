using System.Collections.Generic;
using UnityEngine;

public class DontDestroyOnLoadManager : MonoBehaviour
{
    // 모든 싱글톤 인스턴스를 문자열 키로 식별하여 보관하는 Dictionary
    private static Dictionary<string, GameObject> instances = new Dictionary<string, GameObject>();

    // 각 게임 오브젝트를 식별하기 위한 고유 ID
    // Unity 인스펙터 창에서 각 오브젝트에 맞게 설정해야 합니다.
    public string instanceId;

    void Awake()
    {
        // instanceId가 비어있으면 에러를 출력하고 이 오브젝트를 파괴합니다.
        if (string.IsNullOrEmpty(instanceId))
        {
            Debug.LogError("DontDestroyOnLoadManager 스크립트가 적용된 " + gameObject.name + " 오브젝트의 instanceId가 비어있습니다.");
            Destroy(gameObject);
            return;
        }

        // 만약 같은 ID를 가진 다른 오브젝트가 이미 존재한다면, 이 새로운 오브젝트를 파괴합니다.
        if (instances.ContainsKey(instanceId))
        {
            Destroy(gameObject);
        }
        else
        {
            // 그렇지 않다면, 이 오브젝트를 Dictionary에 추가하고 씬 전환 시 파괴되지 않도록 설정합니다.
            instances.Add(instanceId, gameObject);
            DontDestroyOnLoad(gameObject);
        }
    }
}
