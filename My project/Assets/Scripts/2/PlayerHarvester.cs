using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // UI 위 클릭 방지용

/// <summary>
/// 채광/건축/슬롯 선택을 담당하는 통합 컨트롤러.
/// - 카메라 중앙에서 레이캐스트하여 블록을 채광(Hit)
/// - 선택된 블록 프리팹을 월드에 설치(건축)
/// - 인벤토리 변경을 구독하여 슬롯 목록을 갱신하고 선택을 유지
/// - 무기/도구(검/도끼/곡괭이)에 따라 데미지를 가변 적용
/// </summary>
public class PlayerHarvester : MonoBehaviour
{
    // [데이터 구조] 블록 타입과 프리팹을 연결
    [System.Serializable]
    public struct BlockMapping
    {
        public BlockType type;    // 예: Dirt
        public GameObject prefab; // 예: DirtPrefab
    }

    #region [1. 변수 및 설정]

    [Header("UI 연결")]
    [Tooltip("슬롯 선택 시 빨간 테두리 표시를 위한 UI 스크립트")]
    public InventoryUI inventoryUI;

    [Header("채광(Mining) 설정")]
    [Tooltip("팔이 닿는 최대 거리")]
    public float rayDistance = 5f;

    [Tooltip("레이캐스트 충돌 레이어")]
    public LayerMask hitMask = ~0;

    [Tooltip("기본 채광 데미지 (맨손)")]
    public int baseDamage = 1;

    [Tooltip("철검(IronSword) 착용 시 데미지")]
    public int swordDamage = 5;
    [Tooltip("도끼(Axe) 착용 시 데미지")]
    public int axeDamage = 3;
    [Tooltip("곡괭이(Pickax) 착용 시 데미지")]
    public int pickaxDamage = 4;

    [Tooltip("공격/건축 쿨다운 (광클 방지)")]
    public float hitCooldown = 0.15f;

    [Header("건축(Building) 설정")]
    [Tooltip("설치 가능한 블록 프리팹 리스트")]
    public List<BlockMapping> blockDatabase;

    // 내부 동작 변수
    private float _nextHitTime;
    private Camera _cam;
    private Inventory inventory;
    private PlayerAnimation anim;

    // 현재 보유 중인 아이템 리스트 (1~9번 키 매핑용)
    private List<BlockType> availableBlocks = new List<BlockType>();

    // 현재 손에 들고 있는 아이템 (null이면 빈손)
    private BlockType? currentSelectedBlock = null;

    #endregion

    #region [2. 초기화 및 이벤트 연결]

    void Awake()
    {
        _cam = GetComponentInChildren<Camera>();
        anim = GetComponentInChildren<PlayerAnimation>();

        // 인벤토리 컴포넌트 가져오기 또는 추가
        inventory = GetComponent<Inventory>();
        if (inventory == null) inventory = gameObject.AddComponent<Inventory>();

        // [이벤트 구독] 인벤토리 변경 시 목록 갱신 함수 실행
        inventory.OnInventoryChanged += RefreshAvailableBlocks;

        // UI 자동 연결
        if (inventoryUI == null) inventoryUI = FindObjectOfType<InventoryUI>();
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제 (메모리 누수 방지)
        if (inventory != null) inventory.OnInventoryChanged -= RefreshAvailableBlocks;
    }

    #endregion

    #region [3. 입력 처리 및 업데이트]

    /// <summary>
    /// 입력 프레임 처리:
    /// - UI 모드이거나 포인터가 UI 위면 조작 중단
    /// - 좌클릭: 쿨다운 기준으로 채광 시도
    /// - 우클릭: 현재 선택 아이템으로 건축 시도
    /// </summary>
    void Update()
    {
        // UI 모드이거나 마우스가 UI 버튼 위에 있으면 작동 중지
        if (Cursor.lockState != CursorLockMode.Locked || EventSystem.current.IsPointerOverGameObject())
            return;

        HandleInput();

        // 좌클릭: 채광
        if (Input.GetMouseButton(0) && Time.time >= _nextHitTime)
        {
            _nextHitTime = Time.time + hitCooldown;
            TryMine();
        }

        // 우클릭: 건축
        if (Input.GetMouseButtonDown(1))
        {
            TryBuild();
        }
    }

    /// <summary>
    /// 숫자키(1~9,0)로 슬롯 선택 처리
    /// </summary>
    void HandleInput()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) SelectSlot(i);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0)) SelectSlot(9);
    }

    /// <summary>
    /// 특정 슬롯 선택 및 UI 반영
    /// </summary>
    void SelectSlot(int index)
    {
        if (index >= availableBlocks.Count) return;

        currentSelectedBlock = availableBlocks[index];
        if (anim != null) anim.SetTool(currentSelectedBlock);

        // UI 갱신 요청
        if (inventoryUI != null) inventoryUI.SelectSlot(index);

        Debug.Log($"[아이템 선택] {currentSelectedBlock}");
    }

    /// <summary>
    /// 인벤토리 변경 시 보유 가능한 블록 목록을 정렬/재구성하고
    /// 이전에 들고 있던 아이템을 가능한 경우 동일 항목으로 재선택한다.
    /// </summary>
    void RefreshAvailableBlocks()
    {
        // 1. 현재 손에 들고 있는 아이템이 무엇인지 기억해둡니다. (예: IronSword)
        BlockType? memoryBlock = currentSelectedBlock;

        // 2. 보유 목록 초기화 및 재구성
        availableBlocks.Clear();
        foreach (var item in inventory.items)
        {
            if (item.Value > 0) availableBlocks.Add(item.Key);
        }

        // 3. [정렬] 아이템 순서를 이름순으로 정렬 (슬롯 위치가 뒤죽박죽 섞이는 것 방지)
        availableBlocks.Sort();

        // 4. [위치 추적] 아까 들고 있던 아이템이 여전히 내 가방에 있는가?
        if (memoryBlock.HasValue)
        {
            if (availableBlocks.Contains(memoryBlock.Value))
            {
                // 그 아이템이 몇 번째 칸으로 이사갔는지 찾습니다.
                int newIndex = availableBlocks.IndexOf(memoryBlock.Value);

                // 찾은 위치를 강제로 다시 선택합니다. (UI도 해당 위치로 이동)
                SelectSlot(newIndex);
            }
            else
            {
                // 아이템을 다 써서 없어졌다면 선택 해제
                currentSelectedBlock = null;
                if (inventoryUI != null) inventoryUI.SelectSlot(-1);
            }
        }
    }

    #endregion

    #region [4. 채광 및 건축 로직]

    /// <summary>
    /// 채광(블록 타격) 처리:
    /// - 카메라 중앙에서 레이캐스트
    /// - 선택된 무기/도구에 따라 데미지 계산 후 Block.Hit 호출
    /// </summary>
    void TryMine()
    {
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out var hit, rayDistance, hitMask))
        {
            var block = hit.collider.GetComponent<Block>();
            if (block != null)
            {
                // [데미지 로직] 도구/무기 선택에 따라 가변
                int currentDamage = baseDamage;
                if (currentSelectedBlock == BlockType.IronSword)
                {
                    currentDamage = swordDamage;
                    Debug.Log("⚔️ 철검 공격! (데미지: " + currentDamage + ")");
                }
                else if (currentSelectedBlock == BlockType.Axe)
                {
                    currentDamage = axeDamage;
                    Debug.Log("🪓 도끼 공격! (데미지: " + currentDamage + ")");
                }
                else if (currentSelectedBlock == BlockType.Pickax)
                {
                    currentDamage = pickaxDamage;
                    Debug.Log("⛏️ 곡괭이 공격! (데미지: " + currentDamage + ")");
                }

                if (anim != null) anim.TriggerAttack();
                block.Hit(currentDamage, inventory);
            }
        }
    }

    /// <summary>
    /// 건축(블록 설치):
    /// - 무기/도구면 설치 불가
    /// - 설치 지점은 히트 지점 + 법선 방향 보정 후 격자 반올림
    /// - 플레이어 위치/머리 위는 설치 금지(환불)
    /// </summary>
    void TryBuild()
    {
        if (currentSelectedBlock == null) return;

        // [예외] 도구/무기는 설치할 수 없음
        if (currentSelectedBlock == BlockType.IronSword
            || currentSelectedBlock == BlockType.Axe
            || currentSelectedBlock == BlockType.Pickax)
        {
            Debug.Log("🚫 무기는 설치할 수 없습니다.");
            return;
        }

        GameObject prefabToSpawn = GetPrefabByType(currentSelectedBlock.Value);
        if (prefabToSpawn == null) return;

        // 인벤토리에서 1개 소모 시도
        if (!inventory.Consume(currentSelectedBlock.Value, 1)) return;

        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out var hit, rayDistance, hitMask))
        {
            // 설치 위치 계산
            Vector3 placePoint = hit.point + (hit.normal * 0.5f);
            Vector3Int finalPos = Vector3Int.RoundToInt(placePoint);
            Vector3Int playerPos = Vector3Int.RoundToInt(transform.position);

            // 플레이어 위치 끼임 방지
            if (finalPos == playerPos || finalPos == playerPos + Vector3Int.up)
            {
                inventory.Add(currentSelectedBlock.Value, 1); // 환불
                Debug.Log("🚫 플레이어 위치에는 설치할 수 없습니다.");
                return;
            }

            Instantiate(prefabToSpawn, finalPos, Quaternion.identity);
            if (anim != null) anim.TriggerBuild();
        }
        else
        {
            inventory.Add(currentSelectedBlock.Value, 1); // 환불
        }
    }

    // 프리팹 검색 헬퍼 함수
    GameObject GetPrefabByType(BlockType type)
    {
        foreach (var mapping in blockDatabase)
        {
            if (mapping.type == type) return mapping.prefab;
        }
        return null;
    }

    #endregion
}
