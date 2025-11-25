using UnityEngine;

public enum BlockType { Dirt, Grass, Water, Iron, Diamond }

public class Block : MonoBehaviour
{
    [Header("블록 속성")]
    public BlockType type = BlockType.Dirt;
    public int maxHp = 3;
    public int dropCount = 1;
    public bool mineable = true; // 캘 수 있는가?

    [HideInInspector] public int hp;

    [Header("드롭 설정")]
    public GameObject itemDropPrefab;

    void Awake()
    {
        hp = maxHp;

        // 태그 및 콜라이더 자동 설정
        if (GetComponent<Collider>() == null) gameObject.AddComponent<BoxCollider>();
        if (string.IsNullOrEmpty(gameObject.tag) || gameObject.tag == "Untagged") gameObject.tag = "Block";
    }

    // 채광 시 호출되는 함수
    public void Hit(int damage, Inventory inven)
    {
        if (!mineable) return;

        hp -= damage;
        if (hp <= 0)
        {
            SpawnItemDrop();
            Destroy(gameObject);
        }
    }

    // 아이템 생성 및 튀어 오르는 효과
    void SpawnItemDrop()
    {
        if (itemDropPrefab == null || dropCount <= 0) return;

        // 블록 중심보다 약간 위에서 생성
        Vector3 spawnPos = transform.position + new Vector3(0.5f, 0.5f, 0.5f);
        GameObject drop = Instantiate(itemDropPrefab, spawnPos, Quaternion.identity);
        drop.tag = "ItemDrop";

        // 데이터 전달
        ItemDrop itemScript = drop.GetComponent<ItemDrop>();
        if (itemScript != null)
        {
            itemScript.type = this.type;
            itemScript.count = this.dropCount;
        }

        // 튀어 오르는 물리 효과 적용
        Rigidbody rb = drop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

            // 랜덤한 방향으로 살짝 튀게 함
            float upForce = Random.Range(2f, 3f);
            float horizontalForce = Random.Range(0.5f, 1.0f);
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

            rb.AddForce(Vector3.up * upForce + randomDir * horizontalForce, ForceMode.Impulse);

            // 랜덤 회전 추가
            float randomTorque = Random.Range(-5f, 5f);
            rb.AddTorque(new Vector3(randomTorque, randomTorque, randomTorque), ForceMode.Impulse);
        }
    }
}