using UnityEngine;

// [중요!]
// public enum BlockType... 이 선언문은
// public class Block... 보다 *바깥쪽*에 있어야 합니다.
// (보통 파일의 맨 위에 둡니다)
public enum BlockType
{
    Dirt,    // 흙
    Grass,   // 잔디
    Water,   // 물
    Iron,    // 철
    Diamond  // 다이아몬드
}

/// <summary>
/// 맵을 구성하는 기본 단위 블록입니다.
/// </summary>
public class Block : MonoBehaviour
{
    [Header("Block Stat")]
    // 이 줄에서 오류가 났다는 것은, 이 코드보다 '위'에
    // enum BlockType 정의가 없다는 의미입니다.
    public BlockType type = BlockType.Dirt;
    public int maxHp = 3;
    [HideInInspector] public int hp;
    public int dropCount = 1;
    public bool mineable = true;

    [Header("Item Drop")]
    [Tooltip("파괴되었을 때 스폰할 아이템 드롭 프리팹")]
    public GameObject itemDropPrefab;

    void Awake()
    {
        hp = maxHp;

        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();

        if (string.IsNullOrEmpty(gameObject.tag) || gameObject.tag == "Untagged")
            gameObject.tag = "Block";
    }

    /// <summary>
    /// [수정됨] 피격 시 아이템 드롭 프리팹을 스폰하고 "ItemDrop" 태그를 설정합니다.
    /// </summary>
    public void Hit(int damage, Inventory inven)
    {
        if (!mineable) return;

        hp -= damage;

        if (hp <= 0)
        {
            if (itemDropPrefab != null && dropCount > 0)
            {
                Vector3 spawnPos = transform.position + new Vector3(0.5f, 0.5f, 0.5f);
                GameObject drop = Instantiate(itemDropPrefab, spawnPos, Quaternion.identity);

                // [추가] 생성된 아이템 드롭 오브젝트의 태그를 "ItemDrop"으로 설정합니다.
                drop.tag = "ItemDrop"; // (ItemPickupRadius.cs가 감지)

                ItemDrop itemScript = drop.GetComponent<ItemDrop>();
                if (itemScript != null)
                {
                    itemScript.type = this.type;
                    itemScript.count = this.dropCount;
                }

                Rigidbody rb = drop.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    float randomForce = Random.Range(2f, 4f);
                    rb.AddForce(Vector3.up * randomForce, ForceMode.Impulse);
                    rb.AddTorque(Random.insideUnitSphere * randomForce, ForceMode.Impulse);
                }
            }
            // 블록 파괴
            Destroy(gameObject);
        }
    }
}