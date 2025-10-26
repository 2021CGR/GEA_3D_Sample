// ���� �̸�: DamageZone.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �÷��̾�� �������� �������� �ִ� ����(Zone)�� �����մϴ�.
/// �� ��ũ��Ʈ�� �������� �� '��' ������Ʈ(��: ���)�� �����ؾ� �մϴ�.
/// </summary>
public class DamageZone : MonoBehaviour
{
    [Header("������ ����")]
    [Tooltip("�� �ʸ��� �������� ���� ���� (��)")]
    public float damageInterval = 1f;
    [Tooltip("�� ���� ���� �������� ��")]
    public int damageAmount = 1;

    // ���� ���� �ִ� �÷��̾��, �ش� �÷��̾�� ���� ���� �ڷ�ƾ�� �����մϴ�.
    // (��Ƽ�÷��̾� ������ �ƴϸ� 1�� ����ǰ�����, Ȯ�强�� ���� Dictionary ���)
    private Dictionary<PlayerController, Coroutine> playersInZone = new Dictionary<PlayerController, Coroutine>();

    /// <summary>
    /// �÷��̾ �� ������Ʈ�� 'Ʈ����' ������ ������ �� 1ȸ ȣ��˴ϴ�.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // ���� ������Ʈ�� "Player" �±׸� ������ �ִ��� Ȯ���մϴ�.
        if (other.CompareTag("Player"))
        {
            // PlayerController ������Ʈ�� �����ɴϴ�.
            PlayerController pc = other.GetComponent<PlayerController>();

            // �÷��̾ �����ϰ�, ���� �� ������ ��ϵ��� �ʾҴٸ�
            if (pc != null && !playersInZone.ContainsKey(pc))
            {
                // �������� �ֱ� �����ϴ� �ڷ�ƾ�� �����ϰ�, Dictionary�� �����մϴ�.
                Coroutine damageCoroutine = StartCoroutine(DamagePlayerOverTime(pc));
                playersInZone.Add(pc, damageCoroutine);
                Debug.Log("�÷��̾ ������ ������ ����!");
            }
        }
    }

    /// <summary>
    /// �÷��̾ �� ������Ʈ�� 'Ʈ����' �������� ������ �� 1ȸ ȣ��˴ϴ�.
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();

            // �÷��̾ �����ϰ�, �� ������ ��ϵǾ� �ִٸ�
            if (pc != null && playersInZone.ContainsKey(pc))
            {
                // �����ص� ������ �ڷ�ƾ�� ��� ������ŵ�ϴ�.
                StopCoroutine(playersInZone[pc]);
                // Dictionary���� �÷��̾ �����մϴ�.
                playersInZone.Remove(pc);
                Debug.Log("�÷��̾ ������ �������� ��Ż!");
            }
        }
    }

    /// <summary>
    /// [�ڷ�ƾ] �÷��̾�� ������ ����(damageInterval)���� �������� �ݴϴ�.
    /// </summary>
    private IEnumerator DamagePlayerOverTime(PlayerController pc)
    {
        // �� �ڷ�ƾ�� OnTriggerExit���� StopCoroutine()���� �����Ǳ� ������ ���� �ݺ��մϴ�.
        while (true)
        {
            // (����: ��� �������� �ְ� �ʹٸ� �� ���� �� �Ʒ��� �ű�� �˴ϴ�)
            // 1. ������ ���ݸ�ŭ ����մϴ�.
            yield return new WaitForSeconds(damageInterval);

            // 2. �÷��̾�� �������� �ݴϴ�.
            if (pc != null)
            {
                pc.TakeDamage(damageAmount);
            }
            else
            {
                // Ȥ�� �÷��̾ �װų� ��������� �ڷ�ƾ ������ ����
                yield break;
            }
        }
    }
}