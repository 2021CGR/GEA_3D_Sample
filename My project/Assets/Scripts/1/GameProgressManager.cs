// 파일 이름: GameProgressManager.cs
using UnityEngine;

/// <summary>
/// 게임 전체의 진행 상황(예: 해금된 무기 수)을 저장하는 매니저입니다.
/// 이 스크립트는 씬이 바뀌어도 파괴되지 않고 유지됩니다. (Singleton 패턴)
/// </summary>
public class GameProgressManager : MonoBehaviour
{
    // 'instance'라는 static 변수를 만들어, 다른 모든 스크립트가
    // GameProgressManager.instance 로 이 스크립트에 쉽게 접근할 수 있게 합니다.
    public static GameProgressManager instance;

    [Header("게임 진행 상황")]
    [Tooltip("현재 플레이어가 해금한 무기의 총 개수입니다.")]
    public int unlockedWeaponCount = 1; // 1스테이지는 1개만 가지고 시작

    /// <summary>
    /// Awake는 Start보다 먼저 호출됩니다. 싱글톤 설정을 합니다.
    /// </summary>
    void Awake()
    {
        // 1. instance가 아직 설정되지 않았다면
        if (instance == null)
        {
            // 이 게임 오브젝트를 instance로 지정합니다.
            instance = this;
            // 씬이 전환될 때 이 게임 오브젝트를 파괴하지 말라고 명령합니다.
            DontDestroyOnLoad(gameObject);
        }
        // 2. 만약 instance가 이미 존재하는데 (예: 메인 메뉴로 돌아왔을 때)
        else if (instance != this)
        {
            // 새로 생성된 '이' 오브젝트는 중복이므로 파괴합니다.
            Destroy(gameObject);
        }
    }

    // (나중에 여기에 '체력 업그레이드 횟수', '보유 재화' 등을 추가할 수 있습니다.)
}