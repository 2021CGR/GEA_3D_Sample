using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 감지를 위해 필요
using System.Linq;

/// <summary>
/// 플레이어의 상호작용(채광, 건축)을 총괄하는 클래스입니다.
/// 기능:
/// 1. 좌클릭: 레이캐스트를 이용한 블록 파괴 (채광)
/// 2. 우클릭: 선택된 블록 설치 (건축)
/// 3. 숫자키(1~0): 인벤토리에 있는 블록 선택 및 UI 연동
/// </summary>
public class PlayerHarvester : MonoBehaviour
{
    // [데이터 구조] 블록 타입과 실제 설치될 프리팹을 1:1로 매칭
    [System.Serializable]
    public struct BlockMapping
    {
        public BlockType type;    // 논리적 타입 (예: Dirt)
        public GameObject prefab; // 물리적 프리팹 (예: DirtPrefab)
    }

    #region [변수 설정]

    [Header("UI 연결 (필수)")]
    [Tooltip("현재 선택된 슬롯을 화면에 표시하기 위한 UI 스크립트")]
    public InventoryUI inventoryUI;

    [Header("채광(Mining) 설정")]
    public float rayDistance = 5f;     // 상호작용 가능한 최대 거리
    public LayerMask hitMask = ~0;     // 레이캐스트가 충돌할 레이어 (모든 레이어)
    public int toolDamage = 1;         // 블록에 입히는 데미지
    public float hitCooldown = 0.15f;  // 채광/건축 간격 (공격 속도)

    [Header("건축(Building) 설정")]
    [Tooltip("인스펙터에서 모든 블록 타입과 프리팹을 등록해야 합니다.")]
    public List<BlockMapping> blockDatabase;

    // 내부 동작 변수
    private float _nextHitTime;       // 다음 동작 가능 시간 (쿨다운 체크용)
    private Camera _cam;              // 1인칭 카메라
    private Inventory inventory;      // 플레이어 인벤토리

    // 현재 인벤토리에 1개 이상 보유 중인 블록 타입 목록 (자동 갱신됨)
    private List<BlockType> availableBlocks = new List<BlockType>();

    // 현재 손에 들고 있는(선택된) 블록 타입 (null이면 선택 안 함)
    private BlockType? currentSelectedBlock = null;

    #endregion

    #region [초기화 및 이벤트 연결]

    void Awake()
    {
        // 컴포넌트 가져오기
        _cam = GetComponentInChildren<Camera>();

        // 인벤토리 컴포넌트가 없으면 가져오거나 새로 추가
        if (inventory == null) inventory = GetComponent<Inventory>();
        if (inventory == null) inventory = gameObject.AddComponent<Inventory>();

        // [중요] 인벤토리가 변할 때마다(획득/소모) '사용 가능 목록'을 갱신하도록 이벤트 등록
        inventory.OnInventoryChanged += RefreshAvailableBlocks;

        // UI 스크립트 자동 연결 시도 (안전장치)
        if (inventoryUI == null)
            inventoryUI = FindObjectOfType<InventoryUI>();
    }

    void OnDestroy()
    {
        // 오브젝트 파괴 시 이벤트 구독 해제 (메모리 누수 방지)
        if (inventory != null)
            inventory.OnInventoryChanged -= RefreshAvailableBlocks;
    }

    #endregion

    #region [업데이트 및 입력 처리]

    void Update()
    {
        // 1. 커서가 잠겨있지 않거나(메뉴 상태), 마우스가 UI 위에 있다면 상호작용 금지
        if (Cursor.lockState != CursorLockMode.Locked || EventSystem.current.IsPointerOverGameObject())
            return;

        // 2. 숫자 키 입력 감지 (아이템 선택)
        HandleInput();

        // 3. 좌클릭: 채광 (쿨다운 체크)
        if (Input.GetMouseButton(0) && Time.time >= _nextHitTime)
        {
            _nextHitTime = Time.time + hitCooldown;
            TryMine();
        }

        // 4. 우클릭: 건축 (설치는 보통 단발성 입력인 Down 사용)
        if (Input.GetMouseButtonDown(1))
        {
            TryBuild();
        }
    }

    /// <summary>
    /// 키보드 숫자키(1~9, 0) 입력을 처리하여 슬롯을 선택합니다.
    /// </summary>
    void HandleInput()
    {
        // 키보드 1번 ~ 9번 (KeyCode.Alpha1 ~ Alpha9)
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i); // i는 0부터 시작 (0번 인덱스 = 1번 키)
            }
        }

        // 키보드 0번 (보통 10번째 슬롯으로 사용)
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SelectSlot(9);
        }
    }

    /// <summary>
    /// 특정 인덱스의 아이템을 선택하고 UI를 업데이트합니다.
    /// </summary>
    void SelectSlot(int index)
    {
        // 내가 가진 블록 종류보다 더 높은 번호를 누르면 무시
        if (index >= availableBlocks.Count) return;

        // 현재 선택된 블록 업데이트
        currentSelectedBlock = availableBlocks[index];

        // UI 스크립트에게 "N번째 슬롯 강조해줘"라고 요청
        if (inventoryUI != null)
        {
            inventoryUI.SelectSlot(index);
        }
    }

    /// <summary>
    /// 인벤토리 내용이 변경될 때 호출됩니다.
    /// 보유량이 0인 아이템은 목록에서 제외하고, 선택 중이던 아이템이 사라지면 선택을 해제합니다.
    /// </summary>
    void RefreshAvailableBlocks()
    {
        availableBlocks.Clear();

        // 인벤토리 전체를 순회하며 개수가 1개 이상인 것만 리스트에 추가
        foreach (var item in inventory.items)
        {
            if (item.Value > 0)
            {
                availableBlocks.Add(item.Key);
            }
        }

        // 예외 처리: 현재 손에 들고 있던 블록을 다 써버렸는지 확인
        if (currentSelectedBlock.HasValue)
        {
            if (!inventory.items.ContainsKey(currentSelectedBlock.Value) || inventory.items[currentSelectedBlock.Value] <= 0)
            {
                // 선택 해제
                currentSelectedBlock = null;
                // UI 강조 끄기 (-1 전달)
                if (inventoryUI != null) inventoryUI.SelectSlot(-1);
            }
        }
    }

    #endregion

    #region [채광 및 건축 로직]

    /// <summary>
    /// [좌클릭] 블록 파괴 로직
    /// </summary>
    void TryMine()
    {
        // 화면 중앙(0.5, 0.5)에서 레이 발사
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out var hit, rayDistance, hitMask))
        {
            // 충돌한 물체에서 Block 컴포넌트 확인
            var block = hit.collider.GetComponent<Block>();

            // 블록이 맞다면 데미지 주기 (채광)
            if (block != null)
            {
                block.Hit(toolDamage, inventory);
            }
        }
    }

    /// <summary>
    /// [우클릭] 블록 설치 로직
    /// </summary>
    void TryBuild()
    {
        // 1. 선택된 블록이 없으면 취소
        if (currentSelectedBlock == null) return;

        // 2. 설치할 프리팹 데이터 가져오기
        GameObject prefabToSpawn = GetPrefabByType(currentSelectedBlock.Value);
        if (prefabToSpawn == null) return;

        // 3. [중요] 인벤토리에서 아이템 1개 소모 시도 (실패 시 함수 종료)
        // 선 소모 후 실패 시 환불하는 방식이 더 안전함
        if (!inventory.Consume(currentSelectedBlock.Value, 1)) return;

        // 4. 설치 위치 계산
        Ray ray = _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out var hit, rayDistance, hitMask))
        {
            // 클릭한 지점(point) + 표면 법선(normal)의 절반 = 설치될 블록의 중심 좌표
            Vector3 placePoint = hit.point + (hit.normal * 0.5f);

            // 좌표를 정수(Grid) 단위로 반올림하여 딱 맞게 정렬
            Vector3Int finalPos = Vector3Int.RoundToInt(placePoint);
            Vector3Int playerPos = Vector3Int.RoundToInt(transform.position);

            // 5. 플레이어 끼임 방지 (플레이어 발 밑 or 머리 위치에는 설치 불가)
            if (finalPos == playerPos || finalPos == playerPos + Vector3Int.up)
            {
                // 설치 실패: 소모했던 아이템 환불
                inventory.Add(currentSelectedBlock.Value, 1);
                return;
            }

            // 6. 블록 생성
            Instantiate(prefabToSpawn, finalPos, Quaternion.identity);
        }
        else
        {
            // 허공을 클릭했거나 사거리가 안 닿음: 아이템 환불
            inventory.Add(currentSelectedBlock.Value, 1);
        }
    }

    /// <summary>
    /// 블록 타입(Enum)에 해당하는 프리팹(GameObject)을 리스트에서 찾아 반환합니다.
    /// </summary>
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