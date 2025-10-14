// 파일 이름: StageClearManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // 씬을 관리하기 위해 꼭 필요한 네임스페이스입니다.
using UnityEngine.UI;              // UI.Text를 사용하기 위해 필요합니다.

public class StageClearManager : MonoBehaviour
{
    [Header("클리어 조건")]
    [Tooltip("이 수만큼 적을 잡으면 다음 씬으로 넘어갑니다.")]
    public int killsToClear = 5; // 목표 킬 수

    [Header("다음 씬 정보")]
    [Tooltip("클리어 시 이동할 씬의 이름을 정확하게 입력하세요.")]
    public string nextSceneName; // 이동할 씬의 이름

    [Header("UI 연결 (선택 사항)")]
    [Tooltip("현재 잡은 적의 수를 표시할 Text UI")]
    public Text killCountText;

    private int currentKills = 0; // 현재까지 잡은 적의 수

    /// <summary>
    /// 이 스크립트가 활성화될 때 딱 한 번 호출됩니다.
    /// </summary>
    void OnEnable()
    {
        // Enemy 스크립트의 OnEnemyKilled 이벤트에 "우리의 HandleEnemyKilled 함수"를 등록(구독)합니다.
        // 이제부터 적이 죽을 때마다 HandleEnemyKilled 함수가 자동으로 실행됩니다.
        Enemy.OnEnemyKilled += HandleEnemyKilled;
    }

    /// <summary>
    /// 이 스크립트가 비활성화될 때 호출됩니다. (오브젝트가 파괴될 때 등)
    /// </summary>
    void OnDisable()
    {
        // 등록했던 함수를 해제(구독 취소)합니다.
        // 이렇게 하지 않으면 씬이 바뀌거나 할 때 오류가 발생할 수 있습니다.
        Enemy.OnEnemyKilled -= HandleEnemyKilled;
    }

    /// <summary>
    /// 게임 시작 시 초기화를 담당합니다.
    /// </summary>
    void Start()
    {
        // 시작할 때 킬 카운트를 0으로 초기화하고 UI를 업데이트합니다.
        currentKills = 0;
        UpdateKillCountUI();
    }

    /// <summary>
    /// 적이 죽었다는 신호를 받았을 때 실행될 함수입니다.
    /// </summary>
    void HandleEnemyKilled()
    {
        currentKills++; // 킬 카운트를 1 증가시킵니다.
        UpdateKillCountUI(); // UI를 갱신합니다.

        // 만약 현재 킬 수가 목표 킬 수 이상이 되면
        if (currentKills >= killsToClear)
        {
            Debug.Log("스테이지 클리어! 다음 씬으로 이동합니다.");
            LoadNextScene(); // 다음 씬을 로드하는 함수를 호출합니다.
        }
    }

    /// <summary>
    /// 다음 씬을 로드하는 함수입니다.
    /// </summary>
    void LoadNextScene()
    {
        // nextSceneName 변수에 저장된 이름의 씬을 불러옵니다.
        // 씬 이름이 비어있으면 오류를 방지하기 위해 아무것도 하지 않습니다.
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("다음 씬 이름(Next Scene Name)이 지정되지 않았습니다!");
        }
    }

    /// <summary>
    /// 킬 카운트 UI를 업데이트하는 함수입니다.
    /// </summary>
    void UpdateKillCountUI()
    {
        if (killCountText != null)
        {
            killCountText.text = "Kills: " + currentKills + " / " + killsToClear;
        }
    }
}
