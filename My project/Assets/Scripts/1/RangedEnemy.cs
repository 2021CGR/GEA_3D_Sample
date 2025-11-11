// 파일 이름: RangedEnemy.cs
/*using UnityEngine;

// 'Enemy' 클래스의 모든 기능을 물려받습니다.
public class RangedEnemy : Enemy
{
    [Header("원거리 공격 설정")]
    [Tooltip("발사할 총알 프리팹을 연결해주세요.")]
    public GameObject projectilePrefab; // EnemyProjectile.cs를 가진 프리팹

    [Tooltip("총알이 발사될 위치입니다.")]
    public Transform firePoint;

    // override 키워드로 부모의 AttackPlayer() 내용을 원거리 공격으로 채웁니다.
    protected override void AttackPlayer()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            transform.LookAt(player.position); // 발사 직전 플레이어 조준

            if (projectilePrefab != null && firePoint != null)
            {
                // 총알을 생성하고, EnemyProjectile 스크립트에 방향을 알려줍니다.
                GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
                EnemyProjectile ep = proj.GetComponent<EnemyProjectile>();
                if (ep != null)
                {
                    Vector3 dir = (player.position - firePoint.position).normalized;
                    ep.SetDirection(dir);
                }
            }
        }
    }
}*/