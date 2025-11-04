// 파일 이름: PauseManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환을 위해 필요

/// <summary>
/// ESC 키를 눌러 게임을 일시정지/재개하는 기능을 관리합니다.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("UI 연결")]
    [Tooltip("일시정지 시 띄울 UI 패널을 연결해주세요.")]
    public GameObject pausePanel; // 인스펙터에서 'PausePanel' UI를 연결

    [Header("씬 설정")]
    [Tooltip("돌아갈 메인 메뉴 씬의 이름을 정확히 입력하세요.")]
    public string mainMenuSceneName; // 예: "MainMenu"

    // 현재 게임이 일시정지 상태인지 추적하는 변수
    private bool isPaused = false;

    /// <summary>
    /// 게임 시작 시(씬 로드 시) 호출됩니다.
    /// </summary>
    void Start()
    {
        // 1. 시작할 때는 항상 일시정지 패널을 숨깁니다.
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // 2. 시작할 때는 항상 게임 시간이 정상적으로 흐르게 합니다.
        Time.timeScale = 1f;

        // 3. 시작할 때는 일시정지 상태가 아닙니다.
        isPaused = false;

        // 4. (참고) PlayerShooting.cs가 이미 커서를 잠그고 숨기고 있을 것입니다.
    }

    /// <summary>
    // 매 프레임마다 호출됩니다.
    /// </summary>
    void Update()
    {
        // 1. 'Escape' 키를 눌렀는지 감지합니다.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 2. 현재 일시정지 상태인지 확인합니다.
            if (isPaused)
            {
                // 이미 정지 상태 -> '계속하기(Resume)'를 호출
                ResumeGame();
            }
            else
            {
                // 정지 상태가 아님 -> '일시정지(Pause)'를 호출
                PauseGame();
            }
        }
    }

    /// <summary>
    /// 게임을 일시정지시키는 함수입니다. (ESC 키 또는 버튼 클릭으로 호출 가능)
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;

        // 1. 일시정지 패널을 켭니다.
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // 2. 게임의 시간을 0배속으로 만들어 '정지'시킵니다.
        // (Time.timeScale = 0f 이면 FixedUpdate는 멈추고, Update는 계속 실행됩니다)
        Time.timeScale = 0f;

        // 3. 마우스 커서를 보이게 하고, 잠금을 해제합니다 (버튼 클릭을 위해)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// 게임을 재개시키는 함수입니다. ('계속하기' 버튼이 호출할 함수)
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;

        // 1. 일시정지 패널을 끕니다.
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // 2. 게임의 시간을 다시 1배속(정상)으로 되돌립니다.
        Time.timeScale = 1f;

        // 3. 마우스 커서를 다시 잠그고 숨깁니다 (게임 플레이를 위해)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// '메인 메뉴로' 버튼이 호출할 함수입니다.
    /// </summary>
    public void GoToMainMenu()
    {
        // ★★★ (매우 중요) ★★★
        // 씬을 떠나기 전에 반드시 게임 시간을 1배속으로 되돌려야 합니다.
        // 그렇지 않으면 메인 메뉴 씬이 0배속(멈춘 상태)으로 로드됩니다.
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
}