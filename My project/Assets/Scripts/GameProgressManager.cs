// ���� �̸�: GameProgressManager.cs
using UnityEngine;

/// <summary>
/// ���� ��ü�� ���� ��Ȳ(��: �رݵ� ���� ��)�� �����ϴ� �Ŵ����Դϴ�.
/// �� ��ũ��Ʈ�� ���� �ٲ� �ı����� �ʰ� �����˴ϴ�. (Singleton ����)
/// </summary>
public class GameProgressManager : MonoBehaviour
{
    // 'instance'��� static ������ �����, �ٸ� ��� ��ũ��Ʈ��
    // GameProgressManager.instance �� �� ��ũ��Ʈ�� ���� ������ �� �ְ� �մϴ�.
    public static GameProgressManager instance;

    [Header("���� ���� ��Ȳ")]
    [Tooltip("���� �÷��̾ �ر��� ������ �� �����Դϴ�.")]
    public int unlockedWeaponCount = 1; // 1���������� 1���� ������ ����

    /// <summary>
    /// Awake�� Start���� ���� ȣ��˴ϴ�. �̱��� ������ �մϴ�.
    /// </summary>
    void Awake()
    {
        // 1. instance�� ���� �������� �ʾҴٸ�
        if (instance == null)
        {
            // �� ���� ������Ʈ�� instance�� �����մϴ�.
            instance = this;
            // ���� ��ȯ�� �� �� ���� ������Ʈ�� �ı����� ����� ����մϴ�.
            DontDestroyOnLoad(gameObject);
        }
        // 2. ���� instance�� �̹� �����ϴµ� (��: ���� �޴��� ���ƿ��� ��)
        else if (instance != this)
        {
            // ���� ������ '��' ������Ʈ�� �ߺ��̹Ƿ� �ı��մϴ�.
            Destroy(gameObject);
        }
    }

    // (���߿� ���⿡ 'ü�� ���׷��̵� Ƚ��', '���� ��ȭ' ���� �߰��� �� �ֽ��ϴ�.)
}