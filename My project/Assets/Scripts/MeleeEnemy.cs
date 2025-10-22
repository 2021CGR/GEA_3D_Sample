// ���� �̸�: MeleeEnemy.cs
using UnityEngine;

// 'Enemy' Ŭ������ ��� ����� �����޽��ϴ�.
public class MeleeEnemy : Enemy
{
    [Header("���� ���� ����")]
    [Tooltip("���� ������ �÷��̾�� ���� �������Դϴ�.")]
    public int meleeDamage = 1;

 
    [Header("����� ���� (������)")]
    [Tooltip("������ ������� ���� �ð� (��)�Դϴ�.")]
    public float slowDuration = 3f;
    [Tooltip("������ ������� ���� (�ӵ��� ���� ����, 1.0f�� ���� �ӵ�)�Դϴ�.")]
    public float slowMagnitude = 0.5f; // 0.5f�� �����ϸ� ���� �ӵ��� ����(50%)���� �������ϴ�.
   

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
                    // 1. PlayerController�� TakeDamage �Լ��� ȣ���Ͽ� �������� �ݴϴ�.
                    playerController.TakeDamage(meleeDamage);
                }

                
                // 2. �÷��̾� ������Ʈ���� StatusEffectManager�� ã�� ������� �����մϴ�.
                StatusEffectManager effectManager = player.GetComponent<StatusEffectManager>();
                if (effectManager != null)
                {
                    effectManager.ApplyDebuff(
                        DebuffType.Slow,     // ������ ����� ����
                        slowDuration,        // �ν����Ϳ��� ������ ���� �ð�
                        slowMagnitude        // �ν����Ϳ��� ������ ����
                    );
                }
              
            }
        }
    }
}