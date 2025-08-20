using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobSpawner : MonoBehaviour
{
    [Header("스폰 설정")]
    [SerializeField] private GameObject monsterPrefab;
    [SerializeField] private int maxMonsters = 5;
    [SerializeField] private float spawnInterval = 8f;

    [Header("스폰 위치")]
    [SerializeField] private float spawnYPosition = 0f;
    [SerializeField] private float spawnXRangeMin = -10f;
    [SerializeField] private float spawnXRangeMax = 10f;

    private List<GameObject> spawnedMonsters = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 리스트에서 파괴된 몬스터들 제거
            spawnedMonsters.RemoveAll(monster => monster == null);

            // 스폰해야 할 몬스터 수 계산
            int monstersToSpawn = maxMonsters - spawnedMonsters.Count;

            // 계산된 수만큼 몬스터 스폰
            for (int i = 0; i < monstersToSpawn; i++)
            {
                SpawnSingleMonster();
            }
        }
    }

    void SpawnSingleMonster()
    {
        // 무작위 스폰 위치 결정
        float randomX = Random.Range(spawnXRangeMin, spawnXRangeMax);
        Vector2 spawnPosition = new Vector2(randomX, spawnYPosition);

        // 몬스터를 생성하고 리스트에 추가
        GameObject newMonster = Instantiate(monsterPrefab, spawnPosition, Quaternion.identity);
        spawnedMonsters.Add(newMonster);
    }

    // 기즈모로 스폰 범위 시각화
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 startPoint = new Vector3(spawnXRangeMin, spawnYPosition, 0);
        Vector3 endPoint = new Vector3(spawnXRangeMax, spawnYPosition, 0);
        Gizmos.DrawLine(startPoint, endPoint);
    }
}