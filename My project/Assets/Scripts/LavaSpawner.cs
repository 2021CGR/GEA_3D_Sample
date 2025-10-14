// ���� �̸�: LavaSpawner.cs
using UnityEngine;

public class LavaSpawner : MonoBehaviour
{
    // --- public ���� (�ν����� â���� ���� ������ �� �ֽ��ϴ�) ---

    [Header("���� ����")]
    [Tooltip("������ �Ҷ� �������� ���⿡ �������ּ���.")]
    public GameObject emberPrefab; // ������ �Ҷ��� ���� ������

    // [������] �Ҷ��� ������ ��ġ���� ��� �迭�Դϴ�.
    [Tooltip("�Ҷ��� ������ ��ġ(Transform)���� ���⿡ �������ּ���.")]
    public Transform[] spawnPoints;

    [Header("�ð� ����")]
    [Tooltip("�Ҷ��� �����Ǵ� �ּ� �ð� ����")]
    public float minSpawnDelay = 0.1f; // ���� �Ҷ��� �����Ǳ���� �ɸ��� �ּ� �ð�

    [Tooltip("�Ҷ��� �����Ǵ� �ִ� �ð� ����")]
    public float maxSpawnDelay = 0.5f; // ���� �Ҷ��� �����Ǳ���� �ɸ��� �ִ� �ð�

    // --- private ���� (��ũ��Ʈ ���ο����� ���˴ϴ�) ---
    private float spawnTimer; // ���� �������� ���� �ð��� �����ϴ� Ÿ�̸�

    /// <summary>
    /// ���� ������Ʈ�� ó�� Ȱ��ȭ�� �� �� �� ȣ��Ǵ� �Լ��Դϴ�.
    /// </summary>
    void Start()
    {
        // ù �Ҷ��� �ٷ� �����ǵ��� Ÿ�̸Ӹ� 0���� �����մϴ�.
        spawnTimer = 0f;
    }

    /// <summary>
    /// �� �����Ӹ��� ȣ��Ǵ� �Լ��Դϴ�.
    /// </summary>
    void Update()
    {
        // ���� Ÿ�̸Ӹ� ���ҽ�ŵ�ϴ�.
        spawnTimer -= Time.deltaTime;

        // Ÿ�̸Ӱ� 0 ���ϰ� �Ǹ� �Ҷ��� ������ �ð��� �� ���Դϴ�.
        if (spawnTimer <= 0)
        {
            // �Ҷ� ���� �Լ��� ȣ���մϴ�.
            SpawnEmber();

            // ���� �Ҷ��� ������ �ð��� �����ϰ� �ٽ� �����մϴ�.
            spawnTimer = Random.Range(minSpawnDelay, maxSpawnDelay);
        }
    }

    /// <summary>
    /// �Ҷ� �������� �����ϴ� �Լ��Դϴ�.
    /// </summary>
    void SpawnEmber()
    {
        // �������� �������� �ʾҰų�, ���� ��ġ�� �ϳ��� �������� �ʾҴٸ� ���� �޽����� ����ϰ� �Լ��� �����մϴ�.
        // �̷��� ���� ó���� ���ָ� �Ǽ��� �����ϰ� ������ ã�� �������ϴ�.
        if (emberPrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Ember Prefab �Ǵ� Spawn Points�� �������� �ʾҽ��ϴ�!");
            return;
        }

        // --- [������] ������ ���� ��ġ �� �ϳ��� �������� �����մϴ�. ---
        // 0���� spawnPoints �迭�� ���� - 1 ������ ������ ������ �����մϴ�.
        int randomIndex = Random.Range(0, spawnPoints.Length);

        // ������ ���� ���� �ε����� �ش��ϴ� Transform�� �����ɴϴ�.
        Transform selectedPoint = spawnPoints[randomIndex];

        // ���õ� Transform�� ��ġ(position)�� ���� ��ġ�� ����մϴ�.
        Vector3 spawnPosition = selectedPoint.position;

        // --- �Ҷ� �������� ������ ����(Instantiate)�մϴ�. ---
        Instantiate(emberPrefab, spawnPosition, Quaternion.identity);
    }

    /// <summary>
    /// Scene �信 ���� �������� �ð������� ǥ�����ִ� �Լ��Դϴ�. (���� ����� ����)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        // ������� ������ ��������� �����մϴ�.
        Gizmos.color = Color.yellow;

        // spawnPoints �迭�� ������� �ʴٸ�
        if (spawnPoints != null)
        {
            // �迭�� �ִ� ��� ��ġ(point)�� ���� �ݺ��մϴ�.
            foreach (Transform point in spawnPoints)
            {
                // point�� ������� �ʴٸ� (��Ȥ �迭�� �� ������ ���� ��츦 ���)
                if (point != null)
                {
                    // �ش� ��ġ�� �������� 0.5�� ���̾� ���Ǿ�(WireSphere)�� �׸��ϴ�.
                    // �̸� ���� �� �����Ϳ��� �Ҷ� ���� ��ġ�� ���� Ȯ���� �� �ֽ��ϴ�.
                    Gizmos.DrawWireSphere(point.position, 0.5f);
                }
            }
        }
    }
}