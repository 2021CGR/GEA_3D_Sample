 using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 간단한 제작 패널 제어:
/// - 패널 열기/닫기 및 커서/카메라 제어는 PlayerController가 담당하고,
///   이 클래스는 UI 표시/재료 관리/제작 실행을 담당한다.
/// - AddPlanned: 인벤토리에서 재료를 즉시 소모하여 계획에 추가
/// - ClearPlanned: 방금 소모한 재료를 모두 환불
/// - DoCraft: 레시피 일치 시 결과만 지급(재료는 이미 소모됨)
/// </summary>
public class CraftingPanel : MonoBehaviour
{
    public static CraftingPanel Instance;

    public Inventory inventory;
    public List<CraftingRecipe> recipeList;
    public GameObject root;
    public Text plannedText;
    public Button craftButton;
    public Button clearButton;
    public Text hintText;

    readonly Dictionary<BlockType, int> planned = new Dictionary<BlockType, int>();
    readonly Dictionary<BlockType, int> consumedPlanned = new Dictionary<BlockType, int>();
    bool isOpen;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SetOpen(false);
        if (craftButton != null) craftButton.onClick.AddListener(DoCraft);
        if (clearButton != null) clearButton.onClick.AddListener(ClearPlanned);
        RefreshPlannedUI();
    }

    // 입력 처리는 PlayerController에서 전담합니다.

    /// <summary>
    /// 패널 열기/닫기 및 표시 안전장치 수행
    /// </summary>
    public void SetOpen(bool open)
    {
        isOpen = open;
        Debug.Log($"[제작 패널] 열기 상태 변경: {open}");
        if (root != null)
        {
            Debug.Log($"[제작 패널] 루트 활성(변경 전): {root.activeSelf}");
            root.SetActive(open);
            Debug.Log($"[제작 패널] 루트 활성(변경 후): {root.activeSelf}");
            if (open)
            {
                EnsureEventSystem();
                EnsureVisible();
            }
        }
        else
        {
            Debug.LogWarning("[제작 패널] root가 비어 있습니다.");
        }
        if (!open) ClearPlanned();
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public void Toggle()
    {
        SetOpen(!isOpen);
    }

    /// <summary>
    /// 재료 추가:
    /// - 인벤토리 잔량 확인 후 즉시 소모
    /// - 계획(planned)과 환불 추적(consumedPlanned)에 누적
    /// - Shift=5, Ctrl=10은 UIItem에서 처리
    /// </summary>
    public void AddPlanned(BlockType type, int count = 1)
    {
        if (inventory == null)
        {
            SetHint("인벤토리가 없습니다.");
            return;
        }
        int available = inventory.GetCount(type);
        if (available <= 0)
        {
            SetHint("재료 부족");
            return;
        }
        int toTake = Mathf.Min(count, available);
        if (toTake <= 0)
        {
            SetHint("재료 부족");
            return;
        }
        inventory.Consume(type, toTake);
        if (!planned.ContainsKey(type)) planned[type] = 0;
        planned[type] += toTake;
        if (!consumedPlanned.ContainsKey(type)) consumedPlanned[type] = 0;
        consumedPlanned[type] += toTake;
        RefreshPlannedUI();
        SetHint($"{type} x{toTake} 추가 완료");
    }

    /// <summary>
    /// 계획 초기화:
    /// - 지금까지 소모한 재료(consumedPlanned)를 모두 환불
    /// - 텍스트/상태 갱신
    /// </summary>
    public void ClearPlanned()
    {
        if (inventory != null)
        {
            foreach (var kv in consumedPlanned)
            {
                if (kv.Value > 0) inventory.Add(kv.Key, kv.Value);
            }
        }
        consumedPlanned.Clear();
        planned.Clear();
        RefreshPlannedUI();
        SetHint("초기화 완료(환불 처리)");
    }

    void RefreshPlannedUI()
    {
        if (plannedText == null) return;
        if (planned.Count == 0)
        {
            plannedText.text = "우클릭으로 재료를 추가하세요.";
            return;
        }
        var sb = new StringBuilder();
        foreach (var kv in planned)
        {
            sb.AppendLine($"{kv.Key} x{kv.Value}");
        }
        plannedText.text = sb.ToString();
    }

    void SetHint(string msg)
    {
        if (hintText != null) hintText.text = msg;
    }

    /// <summary>
    /// 제작 실행:
    /// - 레시피 일치 시 결과 아이템 지급
    /// - 계획/소모 기록 초기화
    /// </summary>
    void DoCraft()
    {
        CraftingRecipe match = null;
        foreach (var r in recipeList)
        {
            if (RecipeMatchesPlanned(r))
            {
                match = r;
                break;
            }
        }
        if (match == null)
        {
            SetHint("레시피 불일치");
            return;
        }
        foreach (var prod in match.outputs)
        {
            inventory.Add(prod.type, prod.count);
        }
        consumedPlanned.Clear();
        planned.Clear();
        RefreshPlannedUI();
        SetHint("조합 완료");
    }

    /// <summary>
    /// 계획된 재료가 레시피 요구량을 충족하는지 검사
    /// </summary>
    bool RecipeMatchesPlanned(CraftingRecipe r)
    {
        foreach (var ing in r.inputs)
        {
            int have = 0;
            planned.TryGetValue(ing.type, out have);
            if (have < ing.count) return false;
        }
        return true;
    }

    /// <summary>
    /// EventSystem이 없으면 자동 생성
    /// </summary>
    void EnsureEventSystem()
    {
        if (EventSystem.current == null)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.LogWarning("[제작 패널] EventSystem이 없어 자동 생성했습니다.");
        }
    }

    /// <summary>
    /// Canvas/레이캐스터/카메라 등 UI 표시 조건을 보장
    /// </summary>
    void EnsureVisible()
    {
        var canvas = root.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[제작 패널] Canvas를 찾지 못했습니다. 루트가 Canvas 하위에 있는지 확인하세요.");
            return;
        }

        canvas.enabled = true;
        var raycaster = canvas.GetComponent<GraphicRaycaster>() ?? canvas.gameObject.AddComponent<GraphicRaycaster>();
        raycaster.enabled = true;

        var group = root.GetComponent<CanvasGroup>();
        if (group == null) group = root.AddComponent<CanvasGroup>();
        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;

        var rt = root.GetComponent<RectTransform>();
        if (rt != null)
        {
            if (rt.localScale.x == 0f || rt.localScale.y == 0f)
            {
                rt.localScale = Vector3.one;
                Debug.LogWarning("[제작 패널] 루트 스케일이 0이라 1로 복구했습니다.");
            }
            root.transform.SetAsLastSibling();
        }

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
            Debug.LogWarning("[제작 패널] Screen Space - Camera인데 카메라가 없어 Camera.main을 지정했습니다.");
        }

        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            Debug.LogWarning("[제작 패널] Canvas가 World Space입니다. 위치/크기/카메라 마스크를 확인하세요.");
        }
    }
}
