using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("설정")]
    public GameObject pauseMenuUI; // 일시 정지 창 (Panel)
    public string mainMenuSceneName = "TitleScene";

    // 크로스헤더 오브젝트를 저장할 변수
    private GameObject crosshairObject;

    public static bool GameIsPaused = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // [추가] 게임으로 돌아가면 크로스헤더 다시 켜기
        ToggleCrosshair(true);
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // [추가] 일시 정지 시 크로스헤더 끄기
        ToggleCrosshair(false);
    }

    /// <summary>
    /// 크로스헤더를 찾아서 켜거나 끄는 함수
    /// </summary>
    void ToggleCrosshair(bool isVisible)
    {
        // 1. 아직 크로스헤더를 찾지 못했다면 이름으로 찾는다.
        // (플레이어가 동적으로 생성되므로, Pause가 눌린 시점에 찾아야 함)
        if (crosshairObject == null)
        {
            crosshairObject = GameObject.Find("CrosshairUI");
        }

        // 2. 찾았다면 켜거나 끈다.
        if (crosshairObject != null)
        {
            crosshairObject.SetActive(isVisible);
        }
        else
        {
            // 혹시 이름을 못 찾았을 경우를 대비해 로그 출력
            Debug.LogWarning("크로스헤더 UI를 찾을 수 없습니다. 오브젝트 이름을 'CrosshairUI'로 확인해주세요.");
        }
    }

    public void LoadMenu()
    {
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}