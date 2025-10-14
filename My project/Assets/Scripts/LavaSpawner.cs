// 파일 이름: LavaSpawner.cs
using UnityEngine;

public class LavaSpawner : MonoBehaviour
{
    // --- public 변수 (인스펙터 창에서 값을 수정할 수 있습니다) ---

    [Header("스폰 설정")]
    [Tooltip("생성할 불똥 프리펩을 여기에 연결해주세요.")]
    public GameObject emberPrefab; // 생성할 불똥의 원본 프리팹

    // [변경점] 불똥이 생성될 위치들을 담는 배열입니다.
    [Tooltip("불똥이 생성될 위치(Transform)들을 여기에 연결해주세요.")]
    public Transform[] spawnPoints;

    [Header("시간 설정")]
    [Tooltip("불똥이 생성되는 최소 시간 간격")]
    public float minSpawnDelay = 0.1f; // 다음 불똥이 생성되기까지 걸리는 최소 시간

    [Tooltip("불똥이 생성되는 최대 시간 간격")]
    public float maxSpawnDelay = 0.5f; // 다음 불똥이 생성되기까지 걸리는 최대 시간

    // --- private 변수 (스크립트 내부에서만 사용됩니다) ---
    private float spawnTimer; // 다음 스폰까지 남은 시간을 추적하는 타이머

    /// <summary>
    /// 게임 오브젝트가 처음 활성화될 때 한 번 호출되는 함수입니다.
    /// </summary>
    void Start()
    {
        // 첫 불똥이 바로 생성되도록 타이머를 0으로 설정합니다.
        spawnTimer = 0f;
    }

    /// <summary>
    /// 매 프레임마다 호출되는 함수입니다.
    /// </summary>
    void Update()
    {
        // 스폰 타이머를 감소시킵니다.
        spawnTimer -= Time.deltaTime;

        // 타이머가 0 이하가 되면 불똥을 생성할 시간이 된 것입니다.
        if (spawnTimer <= 0)
        {
            // 불똥 생성 함수를 호출합니다.
            SpawnEmber();

            // 다음 불똥이 생성될 시간을 랜덤하게 다시 설정합니다.
            spawnTimer = Random.Range(minSpawnDelay, maxSpawnDelay);
        }
    }

    /// <summary>
    /// 불똥 프리팹을 생성하는 함수입니다.
    /// </summary>
    void SpawnEmber()
    {
        // 프리팹이 설정되지 않았거나, 스폰 위치가 하나도 지정되지 않았다면 오류 메시지를 출력하고 함수를 종료합니다.
        // 이렇게 예외 처리를 해주면 실수를 방지하고 원인을 찾기 쉬워집니다.
        if (emberPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Ember Prefab 또는 Spawn Points가 설정되지 않았습니다!");
            return;
        }

        // --- [변경점] 지정된 스폰 위치 중 하나를 무작위로 선택합니다. ---
        // 0부터 spawnPoints 배열의 길이 - 1 사이의 정수형 난수를 생성합니다.
        int randomIndex = Random.Range(0, spawnPoints.Length);

        // 위에서 뽑은 랜덤 인덱스에 해당하는 Transform을 가져옵니다.
        Transform selectedPoint = spawnPoints[randomIndex];

        // 선택된 Transform의 위치(position)를 생성 위치로 사용합니다.
        Vector3 spawnPosition = selectedPoint.position;

        // --- 불똥 프리팹을 실제로 생성(Instantiate)합니다. ---
        Instantiate(emberPrefab, spawnPosition, Quaternion.identity);
    }

    /// <summary>
    /// Scene 뷰에 스폰 지점들을 시각적으로 표시해주는 함수입니다. (게임 실행과 무관)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // 기즈모의 색상을 노란색으로 설정합니다.
        Gizmos.color = Color.yellow;

        // spawnPoints 배열이 비어있지 않다면
        if (spawnPoints != null)
        {
            // 배열에 있는 모든 위치(point)에 대해 반복합니다.
            foreach (Transform point in spawnPoints)
            {
                // point가 비어있지 않다면 (간혹 배열에 빈 슬롯이 있을 경우를 대비)
                if (point != null)
                {
                    // 해당 위치에 반지름이 0.5인 와이어 스피어(WireSphere)를 그립니다.
                    // 이를 통해 씬 에디터에서 불똥 생성 위치를 쉽게 확인할 수 있습니다.
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }
    }
}