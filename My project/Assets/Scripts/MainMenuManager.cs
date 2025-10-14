// ���� �̸�: MainMenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // ���� ��ȯ�ϴ� ����� ����ϱ� ���� �� �ʿ��մϴ�.

public class MainMenuManager : MonoBehaviour
{
    [Header("�̵��� �� �̸�")]
    [Tooltip("Start ��ư�� ������ �� �̵��� ���� ���� �̸��� ��Ȯ�ϰ� �Է��ϼ���.")]
    public string gameSceneName; // �ν����� â���� ���� ���� �̸��� ������ �� �ֵ��� public ������ ����ϴ�.

    [Header("�ɼ� UI �г�")]
    [Tooltip("Option ��ư�� ������ �� �Ѱ� �� UI �г��� ���⿡ �������ּ���.")]
    public GameObject optionsPanel; // �ɼ� â���� ����� UI �г��� �����ϱ� ���� ����

    /// <summary>
    /// 'Start' ��ư�� Ŭ������ �� ȣ��� �Լ��Դϴ�.
    /// </summary>
    public void StartGame()
    {
        // gameSceneName ������ ����� �̸��� ���� �ҷ��ɴϴ�.
        // �� �̸��� ����ְų� �߸��Ǹ� ������ �߻��� �� ������ �����ؾ� �մϴ�.
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            Debug.Log(gameSceneName + " ���� �ҷ��ɴϴ�...");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("���� �� �̸��� �������� �ʾҽ��ϴ�!");
        }
    }

    /// <summary>
    /// 'Option' ��ư�� Ŭ������ �� ȣ��� �Լ��Դϴ�.
    /// </summary>
    public void OpenOptions()
    {
        // �ɼ� �г��� ����Ǿ� �ִٸ�
        if (optionsPanel != null)
        {
            // �ɼ� �г��� Ȱ��ȭ�Ͽ� ȭ�鿡 �����ݴϴ�.
            optionsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// �ɼ� �г� �ȿ� �ִ� 'Close' ��ư�� Ŭ������ �� ȣ��� �Լ��Դϴ�.
    /// </summary>
    public void CloseOptions()
    {
        // �ɼ� �г��� ����Ǿ� �ִٸ�
        if (optionsPanel != null)
        {
            // �ɼ� �г��� ��Ȱ��ȭ�Ͽ� ȭ�鿡�� ����ϴ�.
            optionsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 'Quit' ��ư�� Ŭ������ �� ȣ��� �Լ��Դϴ�.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("������ �����մϴ�.");

        // ����: Application.Quit()�� ����Ƽ �����Ϳ����� �۵����� �ʰ�,
        // ���� ����� ����(PC, ����� ��)������ ���������� �۵��մϴ�.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}