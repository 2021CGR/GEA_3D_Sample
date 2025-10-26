using System.Collections;
using UnityEngine;

/// <summary>
/// ����� ������ �����ϴ� ������(Enum)�Դϴ�.
/// ���⿡ ���ϴ� ��� ������� �߰��Ͽ� ������ �� �ֽ��ϴ�.
/// </summary>
public enum DebuffType
{
    None,    // ����� ����
    Slow,    // ������ (�޸��� �Ұ� ����)
    Poison,  // �ߵ� (���� ����)
    Stun     // ���� (�ൿ �Ұ�)
}

// �� ������Ʈ�� �÷��̾�� �����Ǿ� ����� ���¸� �����մϴ�.
public class StatusEffectManager : MonoBehaviour
{
    // === �ܺ� ������Ʈ ���� ===
    [Header("���� ������Ʈ")]
    [Tooltip("�÷��̾��� �̵� �ӵ��� �����ϴ� PlayerController ��ũ��Ʈ�Դϴ�.")]
    public PlayerController playerController;

    // === ����� ���� ���� ===
    // (�� �������� ������� ��ø�ǰų�, ������ �� ���¸� �����ϱ� ���� �ʿ��մϴ�.)
    private bool isSlowed = false;
    private bool isPoisoned = false; // [�߰�]
    private bool isStunned = false;  // [�߰�]

    // (���� ���� ������ ���� ����)
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
            Debug.LogError("StatusEffectManager: PlayerController�� ã�� �� �����ϴ�! ������� ������ �� �����ϴ�.");
        }
        else
        {
            // ��ũ��Ʈ ���� ��, �÷��̾��� �ʱ� �ӵ�/���� ���� �⺻���� �����صӴϴ�.
            originalSpeed = playerController.speed;
            originalSpeedMultiplier = playerController.sprintMultiplier;
        }
    }

    /// <summary>
    /// [�ٽ�] ��� ������ �� �Լ��� ȣ���Ͽ� ������� ��û�մϴ�.
    /// </summary>
    /// <param name="type">������ ������� ���� (Enum)</param>
    /// <param name="duration">���� �ð� (��)</param>
    /// <param name="magnitude">���� (������ ����, �ʴ� �ߵ� ������ ��)</param>
    public void ApplyDebuff(DebuffType type, float duration, float magnitude)
    {
        // �÷��̾ ���ų�, ���� ���¿����� �ٸ� ������� �ɸ��� �ʵ��� ����
        // (��, ���� ������ ���)
        if (playerController == null || (isStunned && type != DebuffType.Stun))
        {
            return;
        }

        // �������������������� [������ �κ�] ��������������������
        // ������ ���� ���̴� *���� ������* ����� �ڷ�ƾ�� �����մϴ�.
        // (�̸��� Enum�� ���ߴ� ���� �߿��մϴ�: "Slow" -> "SlowDebuffRoutine")
        StopCoroutine(type.ToString() + "DebuffRoutine");

        // ��û���� ����� Ÿ��(type)�� ���� ������ �ڷ�ƾ�� �����մϴ�.
        switch (type)
        {
            case DebuffType.Slow:
                StartCoroutine(SlowDebuffRoutine(duration, magnitude));
                break;

            case DebuffType.Poison:
                // magnitude�� '�ʴ� ������'�� �ؼ��Ͽ� �����մϴ�.
                StartCoroutine(PoisonDebuffRoutine(duration, magnitude));
                break;

            case DebuffType.Stun:
                // magnitude�� ���Ͽ��� ������� ������, �ϰ����� ���� �����մϴ�.
                StartCoroutine(StunDebuffRoutine(duration, magnitude));
                break;

            case DebuffType.None:
            default:
                // �ƹ��͵� ���� ����
                break;
        }
        // ���������������������������������������������������
    }

    // --- 1. ������(Slow) �ڷ�ƾ (���� �ڵ�� ����) ---
    private IEnumerator SlowDebuffRoutine(float duration, float magnitude)
    {
        if (!isSlowed)
        {
            isSlowed = true;
            // (1) �⺻ �ӵ� ���� �� ����
            originalSpeed = playerController.speed;
            playerController.speed *= magnitude;
            // (2) �޸��� ���� ���� �� ��Ȱ��ȭ
            originalSpeedMultiplier = playerController.sprintMultiplier;
            playerController.sprintMultiplier = 1f;

            Debug.Log($"[�����] ������ ����! (�⺻ �ӵ�: {playerController.speed}, �޸��� ��Ȱ��ȭ)");
        }
        else
        {
            Debug.Log("[�����] ������ ����.");
        }

        yield return new WaitForSeconds(duration);

        if (isSlowed)
        {
            // (1) �ӵ� ����
            playerController.speed = originalSpeed;
            // (2) �޸��� ���� ����
            playerController.sprintMultiplier = originalSpeedMultiplier;
            isSlowed = false;
            Debug.Log("[�����] �������� �����Ǿ����ϴ�. ���� �ӵ��� ����.");
        }
    }

    // --- 2. �ߵ�(Poison) �ڷ�ƾ [�߰�] ---
    /// <param name="duration">�� ���� �ð�</param>
    /// <param name="damagePerTick">1�ʸ��� ���� ������ (magnitude�� �� ������ ���޵�)</param>
    private IEnumerator PoisonDebuffRoutine(float duration, float damagePerTick)
    {
        isPoisoned = true;
        float tickInterval = 1.0f; // 1�ʸ��� �������� �ݴϴ�.
        float durationTimer = 0f;
        int damageAmount = Mathf.RoundToInt(damagePerTick); // TakeDamage�� int�� �����Ƿ� ��ȯ

        Debug.Log($"[�����] �ߵ� ����! (���ӽð�: {duration}, �ʴ� ������: {damageAmount})");

        // ���� �ð�(duration)�� �� �� ������ �ݺ�
        while (durationTimer < duration)
        {
            // 1�� ���
            yield return new WaitForSeconds(tickInterval);

            if (playerController != null)
            {
                playerController.TakeDamage(damageAmount);
                Debug.Log($"[�����] �ߵ� ������ {damageAmount} ����!");
            }

            durationTimer += tickInterval;
        }

        isPoisoned = false;
        Debug.Log("[�����] �ߵ� ����.");
    }

    // --- 3. ����(Stun) �ڷ�ƾ [�߰�] ---
    /// <param name="duration">���� ���� �ð�</param>
    /// <param name="magnitude_unused">������ ������ �ʿ� ������, �Ķ���ʹ� �޽��ϴ�.</param>
    private IEnumerator StunDebuffRoutine(float duration, float magnitude_unused)
    {
        // ������ �ٸ� ������� �޸�, �÷��̾� ��Ʈ�ѷ��� ����� ���� �����ؾ� �մϴ�.
        if (playerController == null) yield break;

        isStunned = true;
        playerController.canMove = false; // PlayerController�� �߰��� ����
        Debug.Log($"[�����] ���� ����! (���ӽð�: {duration})");

        yield return new WaitForSeconds(duration);

        playerController.canMove = true;
        isStunned = false;
        Debug.Log("[�����] ���� ����.");
    }
}