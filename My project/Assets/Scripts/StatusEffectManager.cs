using System.Collections;
using UnityEngine;

// ����� ������ �����ϴ� ������(Enum)�Դϴ�.
// ���⿡ 'ȭ��', '�ߵ�' �� �پ��� ������� �߰��� �� �ֽ��ϴ�.
public enum DebuffType
{
    Slow,    // ������ �����
    // Freeze,  // ������ ��
    // Poison,  // �ߵ� ��
}

// �� ������Ʈ�� �÷��̾�� �����Ǿ� ����� ���¸� �����մϴ�.
public class StatusEffectManager : MonoBehaviour
{
    // === �ܺ� ������Ʈ ���� ===
    [Header("���� ������Ʈ")]
    [Tooltip("�÷��̾��� �̵� �ӵ��� �����ϴ� PlayerController ��ũ��Ʈ�Դϴ�.")]
    public PlayerController playerController;

    // === ����� ���� ���� ���� ===
    // ���� ������ ������� ���� ������ ��Ÿ���ϴ�.
    private bool isSlowed = false;
    // ������ ������� ���� �̵� �ӵ� �����Դϴ�.
    private float originalSpeedMultiplier = 1f;

    // Start�� ���� ���� �� �� �� ȣ��˴ϴ�.
    void Start()
    {
        // PlayerController�� �Ҵ���� �ʾҴٸ�, ���� ���� ������Ʈ���� ã���ϴ�.
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        // PlayerController�� �ʼ����̹Ƿ�, ������ ��� ���ϴ�.
        if (playerController == null)
        {
            Debug.LogError("StatusEffectManager: PlayerController�� ã�� �� �����ϴ�! ������� ������ �� �����ϴ�.");
        }
    }

    /// <summary>
    /// ������ ������� Ư�� �ð� ���� �÷��̾�� �����ϴ� ���� �Լ��Դϴ�.
    /// </summary>
    /// <param name="type">������ ������� �����Դϴ� (��: DebuffType.Slow)</param>
    /// <param name="duration">������� ���ӵ� �ð� (��)</param>
    /// <param name="magnitude">������� ���� (�������� ���, �ӵ��� ���� ����)</param>
    public void ApplyDebuff(DebuffType type, float duration, float magnitude)
    {
        // ���� ���� ���� ������ ����� �ڷ�ƾ�� �ִٸ� �����Ͽ� ��ø�� �����մϴ�.
        // (����� ��ø ������ �����ϴٸ� ���⿡ �߰����� ������ �ʿ��մϴ�.)
        StopCoroutine(type.ToString() + "DebuffRoutine");

        // ���ο� ����� �ڷ�ƾ�� �����մϴ�.
        StartCoroutine(type.ToString() + "DebuffRoutine", new object[] { duration, magnitude });
    }

    /// <summary>
    /// [�ڷ�ƾ] ������ ������� ���� �� ������ �ð� �����ϴ� �Լ��Դϴ�.
    /// </summary>
    private IEnumerator SlowDebuffRoutine(float duration, float magnitude)
    {
        // �̹� ������ ���°� �ƴ϶��, ó�� ������� �����մϴ�.
        if (!isSlowed)
        {
            isSlowed = true;
            originalSpeedMultiplier = playerController.sprintMultiplier; // ���� �޸��� �ӵ� ������ �����մϴ�.

            // PlayerController�� �޸��� �ӵ� ������ ����� ������ �°� �����մϴ�.
            // ��: magnitude�� 0.5f�̸� ���� �ӵ��� �������� �������ϴ�.
            playerController.sprintMultiplier *= magnitude;

            Debug.Log($"[�����] �������� ����Ǿ����ϴ�. �ӵ� ����: {playerController.sprintMultiplier}");
        }
        else
        {
            // �̹� ������ ���¶��, �ð��� �ʱ�ȭ�մϴ�.
            // (���⼭�� �ܼ��ϰ� �ڷ�ƾ�� ������Ͽ� ���� �ð��� �����մϴ�.)
        }

        // ����� ���� �ð���ŭ ����մϴ�.
        yield return new WaitForSeconds(duration);

        // --- ����� ���� ���� ---
        if (isSlowed)
        {
            // �ӵ� ������ �����ߴ� ���� ������ �ǵ����ϴ�.
            playerController.sprintMultiplier = originalSpeedMultiplier;
            isSlowed = false;

            Debug.Log("[�����] �������� �����Ǿ����ϴ�. ���� �ӵ��� ����.");
        }
    }
}