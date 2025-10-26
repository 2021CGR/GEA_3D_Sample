using UnityEngine;

// 'Enemy' Ŭ������ ��� ����� �����޽��ϴ�.
public class MeleeEnemy : Enemy
{
    [Header("���� ���� ����")]
    [Tooltip("���� ������ �÷��̾�� ���� �������Դϴ�.")]
    public int meleeDamage = 1;


    // �������������������� [������ ����] ��������������������
    [Header("����� ���� (�ν����Ϳ��� ����)")]
    [Tooltip("������ ������� ������ �����մϴ�. (None���� �θ� ���� �� ��)")]
    public DebuffType debuffType = DebuffType.Slow; // �⺻���� Slow�� ����

    [Tooltip("������� ���� �ð� (��)�Դϴ�.")]
    public float debuffDuration = 3f;

    [Tooltip("������� ���� (��ȭ ���� 0.5, �ʴ� �� ������ 1 ��)")]
    public float debuffMagnitude = 0.5f;
    // ���������������������������������������������������


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

                // �������������������� [������ ����] ��������������������
                // ����� Ÿ���� None�� �ƴ� ���� ������� �����մϴ�.
                if (effectManager != null && debuffType != DebuffType.None)
                {
                    // �ν����Ϳ��� ������ �� 3���� �״�� �����մϴ�.
                    effectManager.ApplyDebuff(
                        debuffType,     // �ν����Ϳ��� ������ �����
                        debuffDuration, // �ν����Ϳ��� ������ ���� �ð�
                        debuffMagnitude // �ν����Ϳ��� ������ ����
                    );
                }
                // ���������������������������������������������������
            }
        }
    }
}