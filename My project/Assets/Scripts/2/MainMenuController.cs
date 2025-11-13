using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

/// <summary>
/// 메인 메뉴(타이틀 화면)의 버튼 기능을 관리하는 스크립트입니다.
/// 시작 버튼: 게임 씬 로드
/// 종료 버튼: 게임 종료
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("설정")]
    // 이동할 게임 플레이 씬의 이름을 정확히 적어야 합니다.
    // (예: "SampleScene", "GameScene" 등)
    public string gameSceneName = "GameScene";

    void Start()
    {
        // [중요] 메인 메뉴에 들어오면 마우스 커서를 보이게 하고 잠금을 해제합니다.
        // (게임 플레이 중에는 커서가 잠겨있으므로, 메뉴로 돌아왔을 때 풀어줘야 함)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// '게임 시작' 버튼을 눌렀을 때 호출할 함수
    /// </summary>
    public void OnStartButtonClick()
    {
        // 지정된 이름의 씬을 로드합니다.
        // 주의: File -> Build Settings에 해당 씬이 등록되어 있어야 합니다.
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// '게임 종료' 버튼을 눌렀을 때 호출할 함수
    /// </summary>
    public void OnQuitButtonClick()
    {
        // 에디터에서는 게임 종료가 되지 않으므로 로그를 띄워 확인합니다.
#if UNITY_EDITOR
        Debug.Log("게임 종료 (에디터 상태라 종료되지 않습니다.)");
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 플레이 중지
#else
            // 실제 빌드된 게임을 종료합니다.
            Application.Quit(); 
#endif
    }
}