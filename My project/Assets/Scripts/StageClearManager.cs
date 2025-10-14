// ���� �̸�: StageClearManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // ���� �����ϱ� ���� �� �ʿ��� ���ӽ����̽��Դϴ�.
using UnityEngine.UI;              // UI.Text�� ����ϱ� ���� �ʿ��մϴ�.

public class StageClearManager : MonoBehaviour
{
    [Header("Ŭ���� ����")]
    [Tooltip("�� ����ŭ ���� ������ ���� ������ �Ѿ�ϴ�.")]
    public int killsToClear = 5; // ��ǥ ų ��

    [Header("���� �� ����")]
    [Tooltip("Ŭ���� �� �̵��� ���� �̸��� ��Ȯ�ϰ� �Է��ϼ���.")]
    public string nextSceneName; // �̵��� ���� �̸�

    [Header("UI ���� (���� ����)")]
    [Tooltip("���� ���� ���� ���� ǥ���� Text UI")]
    public Text killCountText;

    private int currentKills = 0; // ������� ���� ���� ��

    /// <summary>
    /// �� ��ũ��Ʈ�� Ȱ��ȭ�� �� �� �� �� ȣ��˴ϴ�.
    /// </summary>
    void OnEnable()
    {
        // Enemy ��ũ��Ʈ�� OnEnemyKilled �̺�Ʈ�� "�츮�� HandleEnemyKilled �Լ�"�� ���(����)�մϴ�.
        // �������� ���� ���� ������ HandleEnemyKilled �Լ��� �ڵ����� ����˴ϴ�.
        Enemy.OnEnemyKilled += HandleEnemyKilled;
    }

    /// <summary>
    /// �� ��ũ��Ʈ�� ��Ȱ��ȭ�� �� ȣ��˴ϴ�. (������Ʈ�� �ı��� �� ��)
    /// </summary>
    void OnDisable()
    {
        // ����ߴ� �Լ��� ����(���� ���)�մϴ�.
        // �̷��� ���� ������ ���� �ٲ�ų� �� �� ������ �߻��� �� �ֽ��ϴ�.
        Enemy.OnEnemyKilled -= HandleEnemyKilled;
    }

    /// <summary>
    /// ���� ���� �� �ʱ�ȭ�� ����մϴ�.
    /// </summary>
    void Start()
    {
        // ������ �� ų ī��Ʈ�� 0���� �ʱ�ȭ�ϰ� UI�� ������Ʈ�մϴ�.
        currentKills = 0;
        UpdateKillCountUI();
    }

    /// <summary>
    /// ���� �׾��ٴ� ��ȣ�� �޾��� �� ����� �Լ��Դϴ�.
    /// </summary>
    void HandleEnemyKilled()
    {
        currentKills++; // ų ī��Ʈ�� 1 ������ŵ�ϴ�.
        UpdateKillCountUI(); // UI�� �����մϴ�.

        // ���� ���� ų ���� ��ǥ ų �� �̻��� �Ǹ�
        if (currentKills >= killsToClear)
        {
            Debug.Log("�������� Ŭ����! ���� ������ �̵��մϴ�.");
            LoadNextScene(); // ���� ���� �ε��ϴ� �Լ��� ȣ���մϴ�.
        }
    }

    /// <summary>
    /// ���� ���� �ε��ϴ� �Լ��Դϴ�.
    /// </summary>
    void LoadNextScene()
    {
        // nextSceneName ������ ����� �̸��� ���� �ҷ��ɴϴ�.
        // �� �̸��� ��������� ������ �����ϱ� ���� �ƹ��͵� ���� �ʽ��ϴ�.
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("���� �� �̸�(Next Scene Name)�� �������� �ʾҽ��ϴ�!");
        }
    }

    /// <summary>
    /// ų ī��Ʈ UI�� ������Ʈ�ϴ� �Լ��Դϴ�.
    /// </summary>
    void UpdateKillCountUI()
    {
        if (killCountText != null)
        {
            killCountText.text = "Kills: " + currentKills + " / " + killsToClear;
        }
    }
}
