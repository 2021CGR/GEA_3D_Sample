// ���� �̸�: Boss.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy ��ũ��Ʈ�� ��ӹ޴� ���� ���� ��ũ��Ʈ�Դϴ�.
/// 1. Enemy.cs�� ü��, UI, ���� �ý����� �״�� ����մϴ�.
/// 2. AttackPlayer()�� '3���� ���Ÿ� ����'�� �����ϵ��� �������մϴ�.
/// 3. Update()���� '���� ��'�� �ֱ������� �����մϴ�.
/// </summary>
public class Boss : Enemy
{
    [Header("�Ӹ� 1: ���� (��ȭ) ����")]
    [Tooltip("����(��ȭ) �Ӽ� ����ü ������")]
    public GameObject iceProjectilePrefab;
    [Tooltip("���� ����ü�� �߻�� ��ġ")]
    public Transform iceFirePoint;
    [Tooltip("���� ������ ��Ÿ�� (��)")]
    public float iceAttackCooldown = 5f;
    private float lastIceAttackTime; // ���� ���� ������ �߻� �ð�

    [Header("�Ӹ� 2: �� ����")]
    [Tooltip("�� �Ӽ� ����ü ������")]
    public GameObject poisonProjectilePrefab;
    [Tooltip("�� ����ü�� �߻�� ��ġ")]
    public Transform poisonFirePoint;
    [Tooltip("�� ������ ��Ÿ�� (��)")]
    public float poisonAttackCooldown = 8f;
    private float lastPoisonAttackTime; // �� ���� ������ �߻� �ð�

    [Header("�Ӹ� 3: ���� ����")]
    [Tooltip("���� �Ӽ� ����ü ������")]
    public GameObject stunProjectilePrefab;
    [Tooltip("���� ����ü�� �߻�� ��ġ")]
    public Transform stunFirePoint;
    [Tooltip("���� ������ ��Ÿ�� (��)")]
    public float stunAttackCooldown = 12f;
    private float lastStunAttackTime; // ���� ���� ������ �߻� �ð�

    [Header("���� ���� ���� ����")]
    [Tooltip("������ ��ȯ�� ���� �� ������ ��� (��: MeleeEnemy)")]
    public List<GameObject> minionPrefabs = new List<GameObject>();
    [Tooltip("���� ���� �� �ʸ��� ��ȯ���� ����")]
    public float minionSpawnInterval = 15f;
    [Tooltip("���� �ֺ� �󸶳� ���� ������ ��������")]
    public float minionSpawnRange = 10f;

    // ���� �� ������ ���� ���� Ÿ�̸�
    private float minionSpawnTimer = 0f;

    /// <summary>
    /// Awake�� Start���� ���� ȣ��˴ϴ�. ��Ÿ�� ������ �ʱ�ȭ�մϴ�.
    /// </summary>
    void Awake()
    {
        // ��Ÿ���� -cooldown���� �����Ͽ� ���� ���� �� ��� ������ �� �ֵ��� �մϴ�.
        lastIceAttackTime = -iceAttackCooldown;

        // (�� ������ ���ÿ� ������ �ʵ��� ��¦ �ð����� �ݴϴ�)
        lastPoisonAttackTime = -poisonAttackCooldown * 0.5f; // ���� ��Ÿ�� ������ �Ŀ�
        lastStunAttackTime = 0f; // ������ ��Ÿ�� �� ä��� ����
    }

    /// <summary>
    /// [������] void Update() -> protected override void Update()�� ����
    /// override: �θ�(Enemy.cs)�� 'virtual Update' �Լ��� ��ӹ޾� ������ �߰�(������)
    /// </summary>
    protected override void Update()
    {
        // �θ�(Enemy.cs)�� Update()�� ���� �����մϴ�.
        // (�̰��� �÷��̾� ����, �Ÿ� ���, ���� ����(Idle->Trace->Attack)�� ó���մϴ�)
        base.Update();

        // (���ϴ� Boss.cs�� Update ����)

        // 1. �÷��̾ ������ �ƹ��͵� �� �� (�θ� Update()�� player�� ã����)
        if (player == null) return;

        // 2. ���� �� ���� Ÿ�̸Ӹ� ������Ʈ�մϴ�.
        minionSpawnTimer += Time.deltaTime;

        // 3. ������ �ð��� �Ǹ�
        if (minionSpawnTimer >= minionSpawnInterval)
        {
            minionSpawnTimer = 0f; // Ÿ�̸� ����
            SpawnMinions();        // ���� ���� �Լ� ȣ��
        }
    }

    /// <summary>
    /// �θ� Enemy.cs�� "Attack" ������ �� �ڵ����� �� �Լ��� ȣ���մϴ�.
    /// ���� �� �Լ��� 3�� �Ӹ��� ��Ÿ���� ���� Ȯ���ϰ� ������ �����մϴ�.
    /// </summary>
    protected override void AttackPlayer()
    {
        // (Enemy.cs�� moveSpeed�� 0�̹Ƿ�) �������� �ʰ� �÷��̾ �Ĵٺ��⸸ �մϴ�.
        transform.LookAt(player.position);

        // --- 1. ���� �Ӹ� ��Ÿ�� Ȯ�� ---
        if (Time.time >= lastIceAttackTime + iceAttackCooldown)
        {
            FireProjectile(iceProjectilePrefab, iceFirePoint); // ���� �߻�
            lastIceAttackTime = Time.time; // ��Ÿ�� ����
        }

        // --- 2. �� �Ӹ� ��Ÿ�� Ȯ�� ---
        if (Time.time >= lastPoisonAttackTime + poisonAttackCooldown)
        {
            FireProjectile(poisonProjectilePrefab, poisonFirePoint); // �� �߻�
            lastPoisonAttackTime = Time.time; // ��Ÿ�� ����
        }

        // --- 3. ���� �Ӹ� ��Ÿ�� Ȯ�� ---
        if (Time.time >= lastStunAttackTime + stunAttackCooldown)
        {
            FireProjectile(stunProjectilePrefab, stunFirePoint); // ���� �߻�
            lastStunAttackTime = Time.time; // ��Ÿ�� ����
        }
    }

    /// <summary>
    /// [�� �Լ�] ����ü �߻� ������ ���� �Լ��� �и��߽��ϴ�.
    /// </summary>
    private void FireProjectile(GameObject prefab, Transform firePoint)
    {
        // �÷��̾�, ������, �߻� ������ ��� �����Ǿ� �־�� �մϴ�.
        if (prefab == null || firePoint == null || player == null)
        {
            Debug.LogWarning("[Boss] ����ü �߻� ������ �����Ǿ����ϴ�.");
            return;
        }

        // 1. ����ü�� �����մϴ�.
        GameObject proj = Instantiate(prefab, firePoint.position, Quaternion.identity);

        // 2. ����ü�� ������ �������ݴϴ�.
        EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
        if (ep != null)
        {
            Vector3 dir = (player.position - firePoint.position).normalized;
            ep.SetDirection(dir);
        }
    }


    /// <summary>
    /// ���� ���� �����ϴ� �Լ� (������ ����)
    /// </summary>
    private void SpawnMinions()
    {
        if (minionPrefabs == null || minionPrefabs.Count == 0)
        {
            Debug.LogWarning("[Boss] ������ ���� ���� �������� �ʾҽ��ϴ�.");
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
            Debug.Log("[Boss] ���� 1�� ����!");
        }
    }

    /// <summary>
    /// �����Ϳ��� ������ ���� ���� ������ ����� ������ ǥ���մϴ�. (������ ����)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, minionSpawnRange);
    }
}