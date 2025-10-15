// ���� �̸�: MeleeEnemy.cs
using UnityEngine;

// 'Enemy' Ŭ������ ��� ����� �����޽��ϴ�.
public class MeleeEnemy : Enemy
{
    [Header("���� ���� ����")]
    [Tooltip("���� ������ �÷��̾�� ���� �������Դϴ�.")]
    public int meleeDamage = 1;

    // �θ��� AttackPlayer()�� ���� ���� ������� �������մϴ�.
    protected override void AttackPlayer()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            transform.LookAt(player.position);

            // �÷��̾ ������ ���� ���� ���� �ִ��� �ٽ� Ȯ���մϴ�.
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist <= attackRange)
            {
                // player �������� PlayerController ��ũ��Ʈ�� ã���ϴ�.
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController != null)
                {
                    // PlayerController�� TakeDamage �Լ��� ȣ���Ͽ� �������� �ݴϴ�.
                    playerController.TakeDamage(meleeDamage);
                }
            }
        }
    }
}