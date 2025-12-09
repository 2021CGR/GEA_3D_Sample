using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

/// <summary>
/// 메인 메뉴(타이틀 화면)의 버튼 동작을 관리합니다.
/// 시작 버튼: 게임 씬 로드
/// 종료 버튼: 애플리케이션 종료
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("설정")]
    // 로드할 게임 씬 이름을 지정합니다.
    // 예: "SampleScene", "GameScene" 등
    public string gameSceneName = "Map1";

    void Start()
    {
        // [주의] 메뉴 화면에서는 커서를 보이게 하고 잠금 해제합니다.
        // (게임 중에는 커서가 잠기므로, 메뉴에서는 해제해야 조작이 가능합니다)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// '게임 시작' 버튼을 눌렀을 때 호출되는 함수
    /// </summary>
    public void OnStartButtonClick()
    {
        // 설정된 씬 이름을 로드합니다.
        // 주의: File -> Build Settings에 해당 씬이 포함되어 있어야 합니다.
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// '게임 종료' 버튼을 눌렀을 때 호출되는 함수
    /// </summary>
    public void OnQuitButtonClick()
    {
        // 에디터에서는 즉시 종료가 되지 않으므로 로그로 확인합니다.
#if UNITY_EDITOR
        Debug.Log("에디터에서 종료 요청(플레이 모드 종료)");
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 플레이 종료
#else
            // 빌드된 애플리케이션을 종료합니다.
            Application.Quit(); 
#endif
    }
}
