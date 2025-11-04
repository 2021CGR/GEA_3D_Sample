using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // NavMeshAgent를 사용하기 위해 AI 네임스페이스 추가

// 이 스크립트를 붙이면 NavMeshAgent 컴포넌트가 자동으로 추가됨
[RequireComponent(typeof(NavMeshAgent))]
public class MonsterAI : MonoBehaviour
{
    [Header("AI Settings")]
    [Tooltip("몬스터가 배회할 최대 반경 (맵 크기에 맞춰 조절)")]
    public float wanderRadius = 15f;

    [Tooltip("목적지 도착 후 대기할 최소 시간")]
    public float minWaitTime = 2f;

    [Tooltip("목적지 도착 후 대기할 최대 시간")]
    public float maxWaitTime = 5f;

    // NavMeshAgent 컴포넌트를 저장할 변수
    private NavMeshAgent agent;

    // 몬스터의 초기 스폰 위치
    private Vector3 startPosition;

    void Start()
    {
        // 컴포넌트를 가져와서 agent 변수에 할당
        agent = GetComponent<NavMeshAgent>();

        // 몬스터의 초기 위치를 저장 (이 위치를 중심으로 배회)
        startPosition = transform.position;

        // AI 로직이 프레임마다 실행되지 않도록 Coroutine으로 실행
        StartCoroutine(WanderRoutine());
    }

    /// <summary>
    /// 몬스터가 무작위로 배회하도록 하는 코루틴
    /// </summary>
    private IEnumerator WanderRoutine()
    {
        // 게임이 실행되는 동안 무한 반복
        while (true)
        {
            // 1. 새로운 목적지 탐색
            Vector3 randomPos = GetRandomNavMeshPoint(startPosition, wanderRadius);

            // 2. NavMeshAgent에 목적지 설정 (길찾기 시작)
            agent.SetDestination(randomPos);

            // 3. 목적지에 거의 도착할 때까지 대기
            //    (agent.pathPending: 경로 계산 중인지 확인)
            //    (agent.remainingDistance: 남은 거리가 0.1f보다 클 때까지)
            while (agent.pathPending || agent.remainingDistance > 0.1f)
            {
                // 1프레임 대기
                yield return null;
            }

            // 4. 목적지 도착 후, 랜덤 시간 동안 대기
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// 지정된 중심(center)과 반경(radius) 내에서 NavMesh 위의 랜덤한 지점을 반환합니다.
    /// </summary>
    private Vector3 GetRandomNavMeshPoint(Vector3 center, float radius)
    {
        // 1. 반경 내에서 랜덤한 방향과 거리를 정함
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += center; // 중심점으로부터의 상대 위치 계산

        // 2. NavMesh.SamplePosition을 사용해 가장 가까운 NavMesh 위의 유효한 지점을 찾음
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, radius, NavMesh.AllAreas))
        {
            // 유효한 지점을 찾았으면 그 위치를 반환
            return hit.position;
        }

        // 못 찾았으면 그냥 현재 위치(혹은 중심 위치)를 반환
        return center;
    }
}