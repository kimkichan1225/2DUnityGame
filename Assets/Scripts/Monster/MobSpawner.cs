using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    public GameObject monsterPrefab; // 몬스터 프리팹
    public float spawnInterval = 3f; // 몬스터 생성 주기 (초)
    public int maxMonsters = 5;      // 최대 스폰 수
    public Transform[] spawnPoints;  // 스폰 위치 배열

    private int currentMonsterCount = 0;

    void Start()
    {
        InvokeRepeating(nameof(SpawnMonster), spawnInterval, spawnInterval);
    }

    void SpawnMonster()
    {
        if (currentMonsterCount >= maxMonsters) return;

        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);
        currentMonsterCount++;
    }

    public void OnMonsterDeath()
    {
        currentMonsterCount--;
    }
}
