using System.Collections;
using UnityEngine;

/// <summary>
/// 디버프 유형을 정의하는 열거형(Enum)입니다.
/// 여기에 원하는 모든 디버프를 추가하여 관리할 수 있습니다.
/// </summary>
public enum DebuffType
{
    None,    // 디버프 없음
    Slow,    // 느려짐 (달리기 불가 포함)
    Poison,  // 중독 (지속 피해)
    Stun     // 스턴 (행동 불가)
}

// 이 컴포넌트는 플레이어에게 부착되어 디버프 상태를 관리합니다.
public class StatusEffectManager : MonoBehaviour
{
    // === 외부 컴포넌트 참조 ===
    [Header("참조 컴포넌트")]
    [Tooltip("플레이어의 이동 속도를 제어하는 PlayerController 스크립트입니다.")]
    public PlayerController playerController;

    // === 디버프 상태 변수 ===
    // (이 변수들은 디버프가 중첩되거나, 해제될 때 상태를 관리하기 위해 필요합니다.)
    private bool isSlowed = false;
    private bool isPoisoned = false; // [추가]
    private bool isStunned = false;  // [추가]

    // (원본 스탯 저장을 위한 변수)
    private float originalSpeedMultiplier = 1f;
    private float originalSpeed = 5f;


    void Start()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (playerController == null)
        {
            Debug.LogError("StatusEffectManager: PlayerController를 찾을 수 없습니다! 디버프를 적용할 수 없습니다.");
        }
        else
        {
            // 스크립트 시작 시, 플레이어의 초기 속도/배율 값을 기본으로 저장해둡니다.
            originalSpeed = playerController.speed;
            originalSpeedMultiplier = playerController.sprintMultiplier;
        }
    }

    /// <summary>
    /// [핵심] 모든 적들이 이 함수를 호출하여 디버프를 요청합니다.
    /// </summary>
    /// <param name="type">적용할 디버프의 종류 (Enum)</param>
    /// <param name="duration">지속 시간 (초)</param>
    /// <param name="magnitude">강도 (느려짐 배율, 초당 중독 데미지 등)</param>
    public void ApplyDebuff(DebuffType type, float duration, float magnitude)
    {
        // 플레이어가 없거나, 스턴 상태에서는 다른 디버프가 걸리지 않도록 방지
        // (단, 스턴 갱신은 허용)
        if (playerController == null || (isStunned && type != DebuffType.Stun))
        {
            return;
        }

        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [수정된 부분] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // 이전에 실행 중이던 *같은 종류의* 디버프 코루틴을 중지합니다.
        // (이름을 Enum과 맞추는 것이 중요합니다: "Slow" -> "SlowDebuffRoutine")
        StopCoroutine(type.ToString() + "DebuffRoutine");

        // 요청받은 디버프 타입(type)에 따라 적절한 코루틴을 실행합니다.
        switch (type)
        {
            case DebuffType.Slow:
                StartCoroutine(SlowDebuffRoutine(duration, magnitude));
                break;

            case DebuffType.Poison:
                // magnitude를 '초당 데미지'로 해석하여 전달합니다.
                StartCoroutine(PoisonDebuffRoutine(duration, magnitude));
                break;

            case DebuffType.Stun:
                // magnitude는 스턴에서 사용하지 않지만, 일관성을 위해 전달합니다.
                StartCoroutine(StunDebuffRoutine(duration, magnitude));
                break;

            case DebuffType.None:
            default:
                // 아무것도 하지 않음
                break;
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }

    // --- 1. 느려짐(Slow) 코루틴 (이전 코드와 동일) ---
    private IEnumerator SlowDebuffRoutine(float duration, float magnitude)
    {
        if (!isSlowed)
        {
            isSlowed = true;
            // (1) 기본 속도 저장 및 감소
            originalSpeed = playerController.speed;
            playerController.speed *= magnitude;
            // (2) 달리기 배율 저장 및 비활성화
            originalSpeedMultiplier = playerController.sprintMultiplier;
            playerController.sprintMultiplier = 1f;

            Debug.Log($"[디버프] 느려짐 적용! (기본 속도: {playerController.speed}, 달리기 비활성화)");
        }
        else
        {
            Debug.Log("[디버프] 느려짐 갱신.");
        }

        yield return new WaitForSeconds(duration);

        if (isSlowed)
        {
            // (1) 속도 복구
            playerController.speed = originalSpeed;
            // (2) 달리기 배율 복구
            playerController.sprintMultiplier = originalSpeedMultiplier;
            isSlowed = false;
            Debug.Log("[디버프] 느려짐이 해제되었습니다. 원래 속도로 복귀.");
        }
    }

    // --- 2. 중독(Poison) 코루틴 [추가] ---
    /// <param name="duration">총 지속 시간</param>
    /// <param name="damagePerTick">1초마다 입힐 데미지 (magnitude가 이 값으로 전달됨)</param>
    private IEnumerator PoisonDebuffRoutine(float duration, float damagePerTick)
    {
        isPoisoned = true;
        float tickInterval = 1.0f; // 1초마다 데미지를 줍니다.
        float durationTimer = 0f;
        int damageAmount = Mathf.RoundToInt(damagePerTick); // TakeDamage는 int를 받으므로 변환

        Debug.Log($"[디버프] 중독 적용! (지속시간: {duration}, 초당 데미지: {damageAmount})");

        // 지속 시간(duration)이 다 될 때까지 반복
        while (durationTimer < duration)
        {
            // 1초 대기
            yield return new WaitForSeconds(tickInterval);

            if (playerController != null)
            {
                playerController.TakeDamage(damageAmount);
                Debug.Log($"[디버프] 중독 데미지 {damageAmount} 적용!");
            }

            durationTimer += tickInterval;
        }

        isPoisoned = false;
        Debug.Log("[디버프] 중독 해제.");
    }

    // --- 3. 스턴(Stun) 코루틴 [추가] ---
    /// <param name="duration">스턴 지속 시간</param>
    /// <param name="magnitude_unused">스턴은 강도가 필요 없지만, 파라미터는 받습니다.</param>
    private IEnumerator StunDebuffRoutine(float duration, float magnitude_unused)
    {
        // 스턴은 다른 디버프와 달리, 플레이어 컨트롤러의 기능을 직접 제어해야 합니다.
        if (playerController == null) yield break;

        isStunned = true;
        playerController.canMove = false; // PlayerController에 추가할 변수
        Debug.Log($"[디버프] 스턴 적용! (지속시간: {duration})");

        yield return new WaitForSeconds(duration);

        playerController.canMove = true;
        isStunned = false;
        Debug.Log("[디버프] 스턴 해제.");
    }
}