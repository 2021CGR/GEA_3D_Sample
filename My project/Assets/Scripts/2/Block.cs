using UnityEngine;

// (BlockType enum은 class 바깥쪽에 있어야 합니다)
public enum BlockType
{
    Dirt,
    Grass,
    Water,
    Iron,
    Diamond
}

public class Block : MonoBehaviour
{
    [Header("Block Stat")]
    public BlockType type = BlockType.Dirt;
    public int maxHp = 3;
    [HideInInspector] public int hp;
    public int dropCount = 1;
    public bool mineable = true;

    [Header("Item Drop")]
    public GameObject itemDropPrefab;

    void Awake()
    {
        hp = maxHp;
        if (GetComponent<Collider>() == null)
            gameObject.AddComponent<BoxCollider>();
        if (string.IsNullOrEmpty(gameObject.tag) || gameObject.tag == "Untagged")
            gameObject.tag = "Block";
    }

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

                drop.tag = "ItemDrop";

                ItemDrop itemScript = drop.GetComponent<ItemDrop>();
                if (itemScript != null)
                {
                    itemScript.type = this.type;
                    itemScript.count = this.dropCount;
                }

                Rigidbody rb = drop.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // 튕기는 힘의 범위
                    float upForce = Random.Range(2f, 4f);
                    rb.AddForce(Vector3.up * upForce, ForceMode.Impulse);

                    // 사방으로 튀어 나가는 수평 힘
                    float horizontalForce = Random.Range(0.1f, 0.3f);
                    Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
                    rb.AddForce(randomDir * horizontalForce, ForceMode.Impulse);
                }
            }
            Destroy(gameObject);
        }
    }
}