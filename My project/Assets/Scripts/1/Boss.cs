// 파일 이름: Boss.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy 스크립트를 상속받는 보스 전용 스크립트입니다.
/// 1. Enemy.cs의 체력, UI, 감지 시스템을 그대로 사용합니다.
/// 2. AttackPlayer()를 '3가지 원거리 공격'을 관리하도록 재정의합니다.
/// 3. Update()에서 '부하 몹'을 주기적으로 스폰합니다.
/// </summary>
public class Boss : Enemy
{
    [Header("머리 1: 얼음 (둔화) 공격")]
    [Tooltip("얼음(둔화) 속성 투사체 프리팹")]
    public GameObject iceProjectilePrefab;
    [Tooltip("얼음 투사체가 발사될 위치")]
    public Transform iceFirePoint;
    [Tooltip("얼음 공격의 쿨타임 (초)")]
    public float iceAttackCooldown = 5f;
    private float lastIceAttackTime; // 얼음 공격 마지막 발사 시간

    [Header("머리 2: 독 공격")]
    [Tooltip("독 속성 투사체 프리팹")]
    public GameObject poisonProjectilePrefab;
    [Tooltip("독 투사체가 발사될 위치")]
    public Transform poisonFirePoint;
    [Tooltip("독 공격의 쿨타임 (초)")]
    public float poisonAttackCooldown = 8f;
    private float lastPoisonAttackTime; // 독 공격 마지막 발사 시간

    [Header("머리 3: 스턴 공격")]
    [Tooltip("스턴 속성 투사체 프리팹")]
    public GameObject stunProjectilePrefab;
    [Tooltip("스턴 투사체가 발사될 위치")]
    public Transform stunFirePoint;
    [Tooltip("스턴 공격의 쿨타임 (초)")]
    public float stunAttackCooldown = 12f;
    private float lastStunAttackTime; // 스턴 공격 마지막 발사 시간

    [Header("보스 부하 스폰 설정")]
    [Tooltip("보스가 소환할 부하 몹 프리팹 목록 (예: MeleeEnemy)")]
    public List<GameObject> minionPrefabs = new List<GameObject>();
    [Tooltip("부하 몹을 몇 초마다 소환할지 간격")]
    public float minionSpawnInterval = 15f;
    [Tooltip("보스 주변 얼마나 넓은 범위에 스폰할지")]
    public float minionSpawnRange = 10f;

    // 부하 몹 스폰을 위한 내부 타이머
    private float minionSpawnTimer = 0f;

    /// <summary>
    /// Awake는 Start보다 먼저 호출됩니다. 쿨타임 변수를 초기화합니다.
    /// </summary>
    void Awake()
    {
        // 쿨타임을 -cooldown으로 설정하여 게임 시작 시 즉시 공격할 수 있도록 합니다.
        lastIceAttackTime = -iceAttackCooldown;

        // (각 공격이 동시에 나가지 않도록 살짝 시간차를 줍니다)
        lastPoisonAttackTime = -poisonAttackCooldown * 0.5f; // 독은 쿨타임 절반쯤 후에
        lastStunAttackTime = 0f; // 스턴은 쿨타임 다 채우고 시작
    }

    /// <summary>
    /// [수정됨] void Update() -> protected override void Update()로 변경
    /// override: 부모(Enemy.cs)의 'virtual Update' 함수를 상속받아 내용을 추가(재정의)
    /// </summary>
    protected override void Update()
    {
        // 부모(Enemy.cs)의 Update()를 먼저 실행합니다.
        // (이것이 플레이어 감지, 거리 계산, 상태 변경(Idle->Trace->Attack)을 처리합니다)
        base.Update();

        // (이하는 Boss.cs의 Update 로직)

        // 1. 플레이어가 없으면 아무것도 안 함 (부모 Update()가 player를 찾아줌)
        if (player == null) return;

        // 2. 부하 몹 스폰 타이머를 업데이트합니다.
        minionSpawnTimer += Time.deltaTime;

        // 3. 스폰할 시간이 되면
        if (minionSpawnTimer >= minionSpawnInterval)
        {
            minionSpawnTimer = 0f; // 타이머 리셋
            SpawnMinions();        // 부하 스폰 함수 호출
        }
    }

    /// <summary>
    /// 부모 Enemy.cs가 "Attack" 상태일 때 자동으로 이 함수를 호출합니다.
    /// 이제 이 함수는 3개 머리의 쿨타임을 각각 확인하고 공격을 지시합니다.
    /// </summary>
    protected override void AttackPlayer()
    {
        // (Enemy.cs의 moveSpeed가 0이므로) 움직이지 않고 플레이어를 쳐다보기만 합니다.
        transform.LookAt(player.position);

        // --- 1. 얼음 머리 쿨타임 확인 ---
        if (Time.time >= lastIceAttackTime + iceAttackCooldown)
        {
            FireProjectile(iceProjectilePrefab, iceFirePoint); // 얼음 발사
            lastIceAttackTime = Time.time; // 쿨타임 리셋
        }

        // --- 2. 독 머리 쿨타임 확인 ---
        if (Time.time >= lastPoisonAttackTime + poisonAttackCooldown)
        {
            FireProjectile(poisonProjectilePrefab, poisonFirePoint); // 독 발사
            lastPoisonAttackTime = Time.time; // 쿨타임 리셋
        }

        // --- 3. 스턴 머리 쿨타임 확인 ---
        if (Time.time >= lastStunAttackTime + stunAttackCooldown)
        {
            FireProjectile(stunProjectilePrefab, stunFirePoint); // 스턴 발사
            lastStunAttackTime = Time.time; // 쿨타임 리셋
        }
    }

    /// <summary>
    /// [새 함수] 투사체 발사 로직을 공통 함수로 분리했습니다.
    /// </summary>
    private void FireProjectile(GameObject prefab, Transform firePoint)
    {
        // 플레이어, 프리팹, 발사 지점이 모두 설정되어 있어야 합니다.
        if (prefab == null || firePoint == null || player == null)
        {
            Debug.LogWarning("[Boss] 투사체 발사 설정이 누락되었습니다.");
            return;
        }

        // 1. 투사체를 생성합니다.
        GameObject proj = Instantiate(prefab, firePoint.position, Quaternion.identity);

        // 2. 투사체에 방향을 설정해줍니다.
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null)
        {
            Vector3 dir = (player.position - firePoint.position).normalized;
            ep.SetDirection(dir);
        }
    }


    /// <summary>
    /// 부하 몹을 스폰하는 함수 (기존과 동일)
    /// </summary>
    private void SpawnMinions()
    {
        if (minionPrefabs == null || minionPrefabs.Count == 0)
        {
            Debug.LogWarning("[Boss] 스폰할 부하 몹이 지정되지 않았습니다.");
            return;
        }

        Vector3 spawnPos = new Vector3(
            transform.position.x + Random.Range(-minionSpawnRange, minionSpawnRange),
            transform.position.y,
            transform.position.z + Random.Range(-minionSpawnRange, minionSpawnRange)
        );
        int randomIndex = Random.Range(0, minionPrefabs.Count);
        GameObject prefabToSpawn = minionPrefabs[randomIndex];
        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            Debug.Log("[Boss] 부하 1기 스폰!");
        }
    }

    /// <summary>
    /// 에디터에서 보스의 부하 스폰 범위를 노란색 원으로 표시합니다. (기존과 동일)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minionSpawnRange);
    }
}