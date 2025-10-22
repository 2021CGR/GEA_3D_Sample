using System.Collections;
using UnityEngine;

// 디버프 유형을 정의하는 열거형(Enum)입니다.
// 여기에 '화상', '중독' 등 다양한 디버프를 추가할 수 있습니다.
public enum DebuffType
{
    Slow,    // 느려짐 디버프
    // Freeze,  // 얼어붙음 등
    // Poison,  // 중독 등
}

// 이 컴포넌트는 플레이어에게 부착되어 디버프 상태를 관리합니다.
public class StatusEffectManager : MonoBehaviour
{
    // === 외부 컴포넌트 참조 ===
    [Header("참조 컴포넌트")]
    [Tooltip("플레이어의 이동 속도를 제어하는 PlayerController 스크립트입니다.")]
    public PlayerController playerController;

    // === 디버프 관련 내부 변수 ===
    // 현재 느려짐 디버프가 적용 중인지 나타냅니다.
    private bool isSlowed = false;
    // 느려짐 디버프의 원래 이동 속도 배율입니다.
    private float originalSpeedMultiplier = 1f;

    // Start는 게임 시작 시 한 번 호출됩니다.
    void Start()
    {
        // PlayerController가 할당되지 않았다면, 같은 게임 오브젝트에서 찾습니다.
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        // PlayerController가 필수적이므로, 없으면 경고를 띄웁니다.
        if (playerController == null)
        {
            Debug.LogError("StatusEffectManager: PlayerController를 찾을 수 없습니다! 디버프를 적용할 수 없습니다.");
        }
    }

    /// <summary>
    /// 지정된 디버프를 특정 시간 동안 플레이어에게 적용하는 공용 함수입니다.
    /// </summary>
    /// <param name="type">적용할 디버프의 종류입니다 (예: DebuffType.Slow)</param>
    /// <param name="duration">디버프가 지속될 시간 (초)</param>
    /// <param name="magnitude">디버프의 강도 (느려짐의 경우, 속도에 곱할 배율)</param>
    public void ApplyDebuff(DebuffType type, float duration, float magnitude)
    {
        // 현재 실행 중인 동일한 디버프 코루틴이 있다면 중지하여 중첩을 방지합니다.
        // (디버프 중첩 로직이 복잡하다면 여기에 추가적인 관리가 필요합니다.)
        StopCoroutine(type.ToString() + "DebuffRoutine");

        // 새로운 디버프 코루틴을 시작합니다.
        StartCoroutine(type.ToString() + "DebuffRoutine", new object[] { duration, magnitude });
    }

    /// <summary>
    /// [코루틴] 느려짐 디버프의 적용 및 해제를 시간 관리하는 함수입니다.
    /// </summary>
    private IEnumerator SlowDebuffRoutine(float duration, float magnitude)
    {
        // 이미 느려짐 상태가 아니라면, 처음 디버프를 적용합니다.
        if (!isSlowed)
        {
            isSlowed = true;
            originalSpeedMultiplier = playerController.sprintMultiplier; // 현재 달리기 속도 배율을 저장합니다.

            // PlayerController의 달리기 속도 배율을 디버프 강도에 맞게 조정합니다.
            // 예: magnitude가 0.5f이면 원래 속도의 절반으로 느려집니다.
            playerController.sprintMultiplier *= magnitude;

            Debug.Log($"[디버프] 느려짐이 적용되었습니다. 속도 배율: {playerController.sprintMultiplier}");
        }
        else
        {
            // 이미 느려짐 상태라면, 시간만 초기화합니다.
            // (여기서는 단순하게 코루틴을 재시작하여 지속 시간을 갱신합니다.)
        }

        // 디버프 지속 시간만큼 대기합니다.
        yield return new WaitForSeconds(duration);

        // --- 디버프 해제 로직 ---
        if (isSlowed)
        {
            // 속도 배율을 저장했던 원래 값으로 되돌립니다.
            playerController.sprintMultiplier = originalSpeedMultiplier;
            isSlowed = false;

            Debug.Log("[디버프] 느려짐이 해제되었습니다. 원래 속도로 복귀.");
        }
    }
}