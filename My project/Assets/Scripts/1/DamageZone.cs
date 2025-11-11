// 파일 이름: DamageZone.cs
/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어에게 지속적인 데미지를 주는 영역(Zone)을 관리합니다.
/// 이 스크립트는 데미지를 줄 '땅' 오브젝트(예: 용암)에 부착해야 합니다.
/// </summary>
public class DamageZone : MonoBehaviour
{
    [Header("데미지 설정")]
    [Tooltip("몇 초마다 데미지를 줄지 간격 (초)")]
    public float damageInterval = 1f;
    [Tooltip("한 번에 입힐 데미지의 양")]
    public int damageAmount = 1;

    // 영역 내에 있는 플레이어와, 해당 플레이어에게 실행 중인 코루틴을 저장합니다.
    // (멀티플레이어 게임이 아니면 1명만 저장되겠지만, 확장성을 위해 Dictionary 사용)
    private Dictionary<PlayerController, Coroutine> playersInZone = new Dictionary<PlayerController, Coroutine>();

    /// <summary>
    /// 플레이어가 이 오브젝트의 '트리거' 영역에 들어왔을 때 1회 호출됩니다.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 들어온 오브젝트가 "Player" 태그를 가지고 있는지 확인합니다.
        if (other.CompareTag("Player"))
        {
            // PlayerController 컴포넌트를 가져옵니다.
            PlayerController pc = other.GetComponent<PlayerController>();

            // 플레이어가 존재하고, 아직 이 영역에 등록되지 않았다면
            if (pc != null && !playersInZone.ContainsKey(pc))
            {
                // 데미지를 주기 시작하는 코루틴을 실행하고, Dictionary에 저장합니다.
                Coroutine damageCoroutine = StartCoroutine(DamagePlayerOverTime(pc));
                playersInZone.Add(pc, damageCoroutine);
                Debug.Log("플레이어가 데미지 영역에 진입!");
            }
        }
    }

    /// <summary>
    /// 플레이어가 이 오브젝트의 '트리거' 영역에서 나갔을 때 1회 호출됩니다.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();

            // 플레이어가 존재하고, 이 영역에 등록되어 있다면
            if (pc != null && playersInZone.ContainsKey(pc))
            {
                // 저장해둔 데미지 코루틴을 즉시 중지시킵니다.
                StopCoroutine(playersInZone[pc]);
                // Dictionary에서 플레이어를 제거합니다.
                playersInZone.Remove(pc);
                Debug.Log("플레이어가 데미지 영역에서 이탈!");
            }
        }
    }

    /// <summary>
    /// [코루틴] 플레이어에게 설정된 간격(damageInterval)마다 데미지를 줍니다.
    /// </summary>
    private IEnumerator DamagePlayerOverTime(PlayerController pc)
    {
        // 이 코루틴은 OnTriggerExit에서 StopCoroutine()으로 중지되기 전까지 무한 반복합니다.
        while (true)
        {
            // (참고: 즉시 데미지를 주고 싶다면 이 줄을 맨 아래로 옮기면 됩니다)
            // 1. 설정된 간격만큼 대기합니다.
            yield return new WaitForSeconds(damageInterval);

            // 2. 플레이어에게 데미지를 줍니다.
            if (pc != null)
            {
                pc.TakeDamage(damageAmount);
            }
            else
            {
                // 혹시 플레이어가 죽거나 사라졌으면 코루틴 스스로 중지
                yield break;
            }
        }
    }
} */