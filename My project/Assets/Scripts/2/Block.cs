using UnityEngine;

// 블록 타입 정의
public enum BlockType { Dirt, Grass, Water, Iron, Diamond, IronSword, Axe, Pickax, Sand, Cactus, Snow, Ice, Stone, DiamondSword }

/// <summary>
/// 채광 가능한 간단한 블록 엔티티.
/// - hp가 0 이하가 되면 자신을 파괴하고 인벤토리에 드랍 개수만큼 추가한다.
/// - 안전장치: 콜라이더/태그가 없으면 자동으로 설정한다.
/// </summary>
public class Block : MonoBehaviour
{
    [Header("블록 속성")]
    public BlockType type = BlockType.Dirt;
    public int maxHp = 3;         // 최대 체력
    public int dropCount = 1;     // 드랍 개수
    public bool mineable = true;  // 채광 가능 여부

    [HideInInspector] public int hp;

    // 드랍 관련 설정 제거

    /// <summary>
    /// 초기 hp 설정 및 안전장치 구성
    /// </summary>
    void Awake()
    {
        hp = maxHp;

        // 콜라이더/태그 자동 설정(안전 장치)
        if (GetComponent<Collider>() == null) gameObject.AddComponent<BoxCollider>();
        if (string.IsNullOrEmpty(gameObject.tag) || gameObject.tag == "Untagged") gameObject.tag = "Block";
    }

    /// <summary>
    /// 플레이어가 블록을 타격했을 때 호출되는 함수
    /// </summary>
    public void Hit(int damage, Inventory inven)
    {
        if (!mineable) return;

        hp -= damage;
        if (hp <= 0)
        {
            if (inven != null)
            {
                inven.Add(this.type, this.dropCount);
            }
            Destroy(gameObject); // 블록 제거
        }
    }

    // (드랍 생성 로직 제거)
}
