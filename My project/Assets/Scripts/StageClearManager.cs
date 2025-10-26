// 파일 이름: StageClearManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageClearManager : MonoBehaviour
{
    [Header("클리어 조건")]
    [Tooltip("이 수만큼 적을 잡으면 클리어됩니다. (보스 스테이지는 1로 설정)")]
    public int killsToClear = 5;

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
        Enemy.OnEnemyKilled += HandleEnemyKilled;
    }
    void OnDisable()
    {
        Enemy.OnEnemyKilled -= HandleEnemyKilled;
    }
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
    /// 적이 죽었다는 신호를 받았을 때 실행될 함수입니다.
    /// </summary>
    void HandleEnemyKilled()
    {
        currentKills++;
        UpdateKillCountUI();

        if (currentKills >= killsToClear)
        {
            if (clearPanel != null)
            {
                // 보스 스테이지 클리어: 패널만 띄움 (무기 개수 증가 X)
                ShowClearPanel();
            }
            else
            {
                // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 로직] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
                // 일반 스테이지 클리어: 무기 개수를 1 증가시키고 다음 씬 로드
                IncrementWeaponCount();
                LoadNextScene();
                // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
            }
        }
    }

    void ShowClearPanel()
    {
        if (clearPanel == null) return;
        clearPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("스테이지 클리어! 클리어 패널을 띄웁니다.");
    }

    /// <summary>
    /// 다음 씬을 로드하는 함수입니다.
    /// </summary>
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
        // (메인 메뉴로 갈 때는 무기 개수를 리셋할 필요 없음. 
        //  어차피 메인 메뉴의 'StartGame' 버튼이 리셋을 담당함)
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

        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [추가된 로직] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // (보스 클리어 후) '게임 재시작'은 '새 게임'과 같으므로 무기 개수를 1로 리셋
        ResetWeaponCount();
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

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

    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [추가된 헬퍼 함수] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
    /// <summary>
    /// GameProgressManager를 찾아 무기 개수를 1 증가시킵니다.
    /// </summary>
    private void IncrementWeaponCount()
    {
        if (GameProgressManager.instance != null)
        {
            GameProgressManager.instance.unlockedWeaponCount++;
            Debug.Log($"[StageClearManager] 무기 해금! 다음 스테이지 무기 개수: {GameProgressManager.instance.unlockedWeaponCount}");
        }
    }

    /// <summary>
    /// GameProgressManager를 찾아 무기 개수를 1로 리셋합니다.
    /// </summary>
    private void ResetWeaponCount()
    {
        if (GameProgressManager.instance != null)
        {
            GameProgressManager.instance.unlockedWeaponCount = 1;
            Debug.Log("[StageClearManager] 무기 개수 1로 리셋.");
        }
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
}