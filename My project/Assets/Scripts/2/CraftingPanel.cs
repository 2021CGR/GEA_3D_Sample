 using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // 씬 전환용

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
        
        // [하드코딩 레시피 주입]
        InjectHardcodedRecipes();

        RefreshPlannedUI();
    }

    void InjectHardcodedRecipes()
    {
        if (recipeList == null) recipeList = new List<CraftingRecipe>();

        // 1. 곡괭이 (돌 2개 -> 곡괭이)
        AddRecipe(new Dictionary<BlockType, int> { { BlockType.Stone, 2 } }, BlockType.Pickax, "Pickax");

        // 2. 도끼 (철 2개 -> 도끼) - 사막맵용 (철이 나온다고 가정)
        AddRecipe(new Dictionary<BlockType, int> { { BlockType.Iron, 2 } }, BlockType.Axe, "Axe");

        // 3. 다이아 검 (다이아 2개 -> 다이아검) - 빙하맵용
        AddRecipe(new Dictionary<BlockType, int> { { BlockType.Diamond, 2 } }, BlockType.DiamondSword, "DiamondSword");
        
        // 4. 철검 (철 2개 -> 철검) - 기본
        AddRecipe(new Dictionary<BlockType, int> { { BlockType.Iron, 2 } }, BlockType.IronSword, "IronSword");
    }

    void AddRecipe(Dictionary<BlockType, int> inputs, BlockType output, string name)
    {
        // 중복 방지 (간단 체크)
        foreach (var r in recipeList)
        {
            if (r.name == name) return;
        }

        var recipe = ScriptableObject.CreateInstance<CraftingRecipe>();
        recipe.name = name;
        recipe.displayName = name;
        
        foreach (var kv in inputs)
        {
            recipe.inputs.Add(new CraftingRecipe.Ingredient { type = kv.Key, count = kv.Value });
        }
        recipe.outputs.Add(new CraftingRecipe.Product { type = output, count = 1 });

        recipeList.Add(recipe);
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

            // [게임 로직] 특정 아이템 제작 시 씬 전환 또는 게임 클리어
            CheckGameProgress(prod.type);
        }
        consumedPlanned.Clear();
        planned.Clear();
        RefreshPlannedUI();
        SetHint("조합 완료");
    }

    void CheckGameProgress(BlockType craftedItem)
    {
        // 씬 전환 전 강제 저장
        if (inventory != null) inventory.SyncToGlobal();

        if (NosieVoxelMap.Instance == null) return;
        var biome = NosieVoxelMap.Instance.currentBiome;

        // 1. 일반(Normal) 맵에서 곡괭이(Pickax) 제작 -> 사막 맵(Map2) 이동
        if (biome == NosieVoxelMap.MapBiome.Normal && craftedItem == BlockType.Pickax)
        {
            Debug.Log("🎉 곡괭이 제작 완료! 사막 맵으로 이동합니다.");
            SceneManager.LoadScene("Map2"); // 씬 이름 확인 필요
        }
        // 2. 사막(Desert) 맵에서 도끼(Axe) 제작 -> 빙하 맵(Map3) 이동
        else if (biome == NosieVoxelMap.MapBiome.Desert && craftedItem == BlockType.Axe)
        {
            Debug.Log("🎉 도끼 제작 완료! 빙하 맵으로 이동합니다.");
            SceneManager.LoadScene("Map3"); // 씬 이름 확인 필요
        }
        // 3. 빙하(Glacier) 맵에서 다이아검(DiamondSword) 제작 -> 게임 클리어
        else if (biome == NosieVoxelMap.MapBiome.Glacier && craftedItem == BlockType.DiamondSword)
        {
            Debug.Log("🏆 다이아 검 제작 완료! 게임 클리어!");
            SetHint("게임 클리어! 축하합니다!");
            // 게임 종료 또는 엔딩 크레딧
            // Application.Quit(); 
            // EditorApplication.isPlaying = false;
        }
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
