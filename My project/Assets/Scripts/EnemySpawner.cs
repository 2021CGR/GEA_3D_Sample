// 파일 이름: EnemySpawner.cs
using System.Collections;
using System.Collections.Generic; // List를 사용하기 위해 필요합니다.
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 변수] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
    // public GameObject enemyPrefab; // (이전) 단일 프리팹

    [Header("스폰 설정")]
    [Tooltip("여기에 생성할 적 프리팹들을 모두 넣어주세요. (예: MeleeEnemy, RangedEnemy)")]
    public List<GameObject> enemyPrefabs = new List<GameObject>(); // (변경) 여러 프리팹을 담을 리스트
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    [Tooltip("적을 몇 초마다 생성할지 간격을 설정합니다.")]
    public float spawnInterval = 3f; // 적 생성 간격
    [Tooltip("스포너의 위치(중심)로부터 얼마나 떨어진 범위 내에서 랜덤하게 생성할지 설정합니다.")]
    public float spawnRange = 5f; // 생성 반경

    private float timer = 0f; // 다음 스폰까지의 시간을 재는 타이머

    /// <summary>
    /// 매 프레임마다 호출됩니다.
    /// </summary>
    void Update()
    {
        // 타이머에 시간을 더합니다.
        timer += Time.deltaTime;

        // 타이머가 설정된 생성 간격(spawnInterval)을 넘어서면
        if (timer >= spawnInterval)
        {
            // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 로직] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
            // enemyPrefabs 리스트에 프리팹이 하나라도 들어있는지 확인합니다.
            if (enemyPrefabs != null && enemyPrefabs.Count > 0)
            {
                // 1. 스폰할 위치를 정합니다. (기존과 동일)
                // x, z는 랜덤, y는 스포너의 y 위치 고정
                Vector3 spawnPos = new Vector3(
                    transform.position.x + Random.Range(-spawnRange, spawnRange), // X축 랜덤
                     transform.position.y,                                          // Y축 고정
                     transform.position.z + Random.Range(-spawnRange, spawnRange) // Z축 랜덤
                     );

                // 2. 리스트에서 랜덤한 적 프리팹을 선택합니다.
                // 0부터 (리스트 크기 - 1) 사이의 랜덤한 숫자를 뽑습니다.
                int randomIndex = Random.Range(0, enemyPrefabs.Count);

                // 리스트에서 해당 순번(randomIndex)의 프리팹을 가져옵니다.
                GameObject prefabToSpawn = enemyPrefabs[randomIndex];

                // 3. 선택된 프리팹을 생성합니다.
                // (혹시 리스트의 해당 칸이 비어있을 경우를 대비해 null 체크)
                if (prefabToSpawn != null)
                {
                    Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                }

                // 타이머를 0으로 리셋합니다.
                timer = 0f;
            }
            else
            {
                // 리스트가 비어있으면 스폰을 시도하지 않고, 타이머만 리셋합니다.
                // (오류가 계속 발생하는 것을 방지)
                timer = 0f;
                Debug.LogWarning("EnemySpawner에 enemyPrefabs 리스트가 비어있습니다. 적을 스폰할 수 없습니다.");
            }
            // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
        }
    }

    /// <summary>
    /// 유니티 에디터의 씬 뷰에서만 보이며, 선택되었을 때 스폰 범위를 시각적으로 표시합니다.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // 스폰 범위를 빨간색 사각형으로 표시합니다. (Y 크기는 1로 고정)
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnRange * 2, 1, spawnRange * 2));
    }
}