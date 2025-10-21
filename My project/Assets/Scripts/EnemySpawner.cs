// ���� �̸�: EnemySpawner.cs
using System.Collections;
using System.Collections.Generic; // List�� ����ϱ� ���� �ʿ��մϴ�.
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // �������������������� [������ ����] ��������������������
    // public GameObject enemyPrefab; // (����) ���� ������

    [Header("���� ����")]
    [Tooltip("���⿡ ������ �� �����յ��� ��� �־��ּ���. (��: MeleeEnemy, RangedEnemy)")]
    public List<GameObject> enemyPrefabs = new List<GameObject>(); // (����) ���� �������� ���� ����Ʈ
    // ���������������������������������������������������

    [Tooltip("���� �� �ʸ��� �������� ������ �����մϴ�.")]
    public float spawnInterval = 3f; // �� ���� ����
    [Tooltip("�������� ��ġ(�߽�)�κ��� �󸶳� ������ ���� ������ �����ϰ� �������� �����մϴ�.")]
    public float spawnRange = 5f; // ���� �ݰ�

    private float timer = 0f; // ���� ���������� �ð��� ��� Ÿ�̸�

    /// <summary>
    /// �� �����Ӹ��� ȣ��˴ϴ�.
    /// </summary>
    void Update()
    {
        // Ÿ�̸ӿ� �ð��� ���մϴ�.
        timer += Time.deltaTime;

        // Ÿ�̸Ӱ� ������ ���� ����(spawnInterval)�� �Ѿ��
        if (timer >= spawnInterval)
        {
            // �������������������� [������ ����] ��������������������
            // enemyPrefabs ����Ʈ�� �������� �ϳ��� ����ִ��� Ȯ���մϴ�.
            if (enemyPrefabs != null && enemyPrefabs.Count > 0)
            {
                // 1. ������ ��ġ�� ���մϴ�. (������ ����)
                // x, z�� ����, y�� �������� y ��ġ ����
                Vector3 spawnPos = new Vector3(
                    transform.position.x + Random.Range(-spawnRange, spawnRange), // X�� ����
                     transform.position.y,                                          // Y�� ����
                     transform.position.z + Random.Range(-spawnRange, spawnRange) // Z�� ����
                     );

                // 2. ����Ʈ���� ������ �� �������� �����մϴ�.
                // 0���� (����Ʈ ũ�� - 1) ������ ������ ���ڸ� �̽��ϴ�.
                int randomIndex = Random.Range(0, enemyPrefabs.Count);

                // ����Ʈ���� �ش� ����(randomIndex)�� �������� �����ɴϴ�.
                GameObject prefabToSpawn = enemyPrefabs[randomIndex];

                // 3. ���õ� �������� �����մϴ�.
                // (Ȥ�� ����Ʈ�� �ش� ĭ�� ������� ��츦 ����� null üũ)
                if (prefabToSpawn != null)
                {
                    Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
                }

                // Ÿ�̸Ӹ� 0���� �����մϴ�.
                timer = 0f;
            }
            else
            {
                // ����Ʈ�� ��������� ������ �õ����� �ʰ�, Ÿ�̸Ӹ� �����մϴ�.
                // (������ ��� �߻��ϴ� ���� ����)
                timer = 0f;
                Debug.LogWarning("EnemySpawner�� enemyPrefabs ����Ʈ�� ����ֽ��ϴ�. ���� ������ �� �����ϴ�.");
            }
            // ���������������������������������������������������
        }
    }

    /// <summary>
    /// ����Ƽ �������� �� �信���� ���̸�, ���õǾ��� �� ���� ������ �ð������� ǥ���մϴ�.
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        // ���� ������ ������ �簢������ ǥ���մϴ�. (Y ũ��� 1�� ����)
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnRange * 2, 1, spawnRange * 2));
    }
}