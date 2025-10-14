// 파일 이름: MainMenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // 씬을 전환하는 기능을 사용하기 위해 꼭 필요합니다.

public class MainMenuManager : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    [Tooltip("Start 버튼을 눌렀을 때 이동할 게임 씬의 이름을 정확하게 입력하세요.")]
    public string gameSceneName; // 인스펙터 창에서 게임 씬의 이름을 설정할 수 있도록 public 변수로 만듭니다.

    [Header("옵션 UI 패널")]
    [Tooltip("Option 버튼을 눌렀을 때 켜고 끌 UI 패널을 여기에 연결해주세요.")]
    public GameObject optionsPanel; // 옵션 창으로 사용할 UI 패널을 연결하기 위한 변수

    /// <summary>
    /// 'Start' 버튼을 클릭했을 때 호출될 함수입니다.
    /// </summary>
    public void StartGame()
    {
        // gameSceneName 변수에 저장된 이름의 씬을 불러옵니다.
        // 씬 이름이 비어있거나 잘못되면 오류가 발생할 수 있으니 주의해야 합니다.
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            Debug.Log(gameSceneName + " 씬을 불러옵니다...");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("게임 씬 이름이 지정되지 않았습니다!");
        }
    }

    /// <summary>
    /// 'Option' 버튼을 클릭했을 때 호출될 함수입니다.
    /// </summary>
    public void OpenOptions()
    {
        // 옵션 패널이 연결되어 있다면
        if (optionsPanel != null)
        {
            // 옵션 패널을 활성화하여 화면에 보여줍니다.
            optionsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 옵션 패널 안에 있는 'Close' 버튼을 클릭했을 때 호출될 함수입니다.
    /// </summary>
    public void CloseOptions()
    {
        // 옵션 패널이 연결되어 있다면
        if (optionsPanel != null)
        {
            // 옵션 패널을 비활성화하여 화면에서 숨깁니다.
            optionsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 'Quit' 버튼을 클릭했을 때 호출될 함수입니다.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");

        // 참고: Application.Quit()은 유니티 에디터에서는 작동하지 않고,
        // 실제 빌드된 게임(PC, 모바일 등)에서만 정상적으로 작동합니다.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}