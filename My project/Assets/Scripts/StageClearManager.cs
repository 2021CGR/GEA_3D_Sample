// 파일 이름: StageClearManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageClearManager : MonoBehaviour
{
    [Header("클리어 조건")]
    [Tooltip("이 수만큼 적을 잡으면 클리어됩니다. (보스 스테이지는 1로 설정)")]
    public int killsToClear = 5;

    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [추가된 변수] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
    [Tooltip("이것을 체크하면 킬 카운트가 'Boss' 태그를 가진 적만 카운트합니다.")]
    public bool isBossStage = false;
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    [Header("다음 씬 정보")]
    [Tooltip("일반 스테이지 클리어 시 이동할 씬의 이름")]
    public string nextSceneName;
    [Tooltip("메인 메뉴 씬의 이름 (클리어 패널 버튼용)")]
    public string mainMenuSceneName;
    [Tooltip("재시작 시 이동할 스테이지 1 씬의 이름")]
    public string stage1SceneName;

    [Header("UI 연결")]
    [Tooltip("현재 잡은 적의 수를 표시할 Text UI")]
    public Text killCountText;
    [Tooltip("클리어 시 띄울 UI 패널 (보스 스테이지 전용)")]
    public GameObject clearPanel;

    private int currentKills = 0;

    void OnEnable()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 부분] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // GameObject를 받는 이벤트 핸들러로 등록합니다.
        Enemy.OnEnemyKilled += HandleEnemyKilled;
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }

    void OnDisable()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 부분] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // 등록 해제도 동일하게 변경
        Enemy.OnEnemyKilled -= HandleEnemyKilled;
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }

    // Start() 함수는 기존과 동일합니다.
    void Start()
    {
        currentKills = 0;
        UpdateKillCountUI();
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// [수정됨] 적이 죽었다는 신호를 받았을 때 실행될 함수입니다.
    /// 이제 '누가' 죽었는지(enemyObject)를 매개변수로 받습니다.
    /// </summary>
    void HandleEnemyKilled(GameObject enemyObject)
    {
        // 1. (새 로직) 보스 스테이지인지 확인
        if (isBossStage)
        {
            // 보스 스테이지라면, 죽은 적이 "Boss" 태그를 가졌는지 확인
            if (enemyObject == null || !enemyObject.CompareTag("Boss"))
            {
                // "Boss" 태그가 없으면(잡몹이면) 카운트하지 않고 함수 종료
                return;
            }
            // (이 아래 로직은 보스일 때만 실행됨)
        }

        // 2. (기존 로직) 일반 스테이지거나, 보스 스테이지에서 보스가 죽었으면 카운트
        currentKills++;
        UpdateKillCountUI();

        // 3. (기존 로직) 클리어 조건 확인
        if (currentKills >= killsToClear)
        {
            if (clearPanel != null)
            {
                // 보스 스테이지 클리어: 패널만 띄움 (무기 개수 증가 X)
                ShowClearPanel();
            }
            else
            {
                // 일반 스테이지 클리어: 무기 개수를 1 증가시키고 다음 씬 로드
                IncrementWeaponCount();
                LoadNextScene();
            }
        }
    }

    // ... (ShowClearPanel, LoadNextScene, GoToMainMenu, RestartGame, QuitGame, UpdateKillCountUI, Increment/ResetWeaponCount 함수는 모두 기존과 동일합니다) ...

    void ShowClearPanel()
    {
        if (clearPanel == null) return;
        clearPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("스테이지 클리어! 클리어 패널을 띄웁니다.");
    }
    void LoadNextScene()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("다음 씬 이름(Next Scene Name)이 지정되지 않았습니다!");
        }
    }
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("메인 메뉴 씬 이름(Main Menu Scene Name)이 지정되지 않았습니다!");
        }
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        ResetWeaponCount();
        if (!string.IsNullOrEmpty(stage1SceneName))
        {
            SceneManager.LoadScene(stage1SceneName);
        }
        else
        {
            Debug.LogError("스테이지 1 씬 이름(Stage 1 Scene Name)이 지정되지 않았습니다!");
        }
    }
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("게임을 종료합니다.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    void UpdateKillCountUI()
    {
        if (killCountText != null)
        {
            killCountText.text = "Kills: " + currentKills + " / " + killsToClear;
        }
    }
    private void IncrementWeaponCount()
    {
        if (GameProgressManager.instance != null)
        {
            GameProgressManager.instance.unlockedWeaponCount++;
            Debug.Log($"[StageClearManager] 무기 해금! 다음 스테이지 무기 개수: {GameProgressManager.instance.unlockedWeaponCount}");
        }
    }
    private void ResetWeaponCount()
    {
        if (GameProgressManager.instance != null)
        {
            GameProgressManager.instance.unlockedWeaponCount = 1;
            Debug.Log("[StageClearManager] 무기 개수 1로 리셋.");
        }
    }
}