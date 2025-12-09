using UnityEngine;

// 블록 타입 정의
public enum BlockType { Dirt, Grass, Water, Iron, Diamond, IronSword }

public class Block : MonoBehaviour
{
    [Header("블록 속성")]
    public BlockType type = BlockType.Dirt;
    public int maxHp = 3;         // 최대 체력
    public int dropCount = 1;     // 드랍 개수
    public bool mineable = true;  // 채광 가능 여부

    [HideInInspector] public int hp;

    [Header("드랍 설정")]
    public GameObject itemDropPrefab; // 드랍 아이템 프리팹(ItemDrop 스크립트 포함)

    void Awake()
    {
        hp = maxHp;

        // 콜라이더/태그 자동 설정(안전 장치)
        if (GetComponent<Collider>() == null) gameObject.AddComponent<BoxCollider>();
        if (string.IsNullOrEmpty(gameObject.tag) || gameObject.tag == "Untagged") gameObject.tag = "Block";
    }

    // 플레이어가 블록을 타격했을 때 호출
    public void Hit(int damage, Inventory inven)
    {
        if (!mineable) return;

        hp -= damage;
        if (hp <= 0)
        {
            SpawnItemDrop();     // 드랍 생성
            Destroy(gameObject); // 블록 제거
        }
    }

    // 드랍 생성 및 튕김(물리) 효과
    void SpawnItemDrop()
    {
        if (itemDropPrefab == null || dropCount <= 0) return;

        // 블록 중심에서 약간 위, 랜덤 방향 힘을 주어 튕기게 함
        Vector3 spawnPos = transform.position + new Vector3(0.5f, 0.5f, 0.5f);
        GameObject drop = Instantiate(itemDropPrefab, spawnPos, Quaternion.identity);
        drop.tag = "ItemDrop";

        // 드랍 스크립트에 타입/수량 설정
        ItemDrop itemScript = drop.GetComponent<ItemDrop>();
        if (itemScript != null)
        {
            itemScript.type = this.type;
            itemScript.count = this.dropCount;
        }

        // 물리(튀김/회전) 효과 적용
        Rigidbody rb = drop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // 위로 튕기고 수평 방향은 랜덤
            float upForce = Random.Range(2f, 3f);
            float horizontalForce = Random.Range(0.5f, 1.0f);
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

            rb.AddForce(Vector3.up * upForce + randomDir * horizontalForce, ForceMode.Impulse);

            // 랜덤 회전 추가(가벼움 표현)
            float randomTorque = Random.Range(-5f, 5f);
            rb.AddTorque(new Vector3(randomTorque, randomTorque, randomTorque), ForceMode.Impulse);
        }
    }
}
