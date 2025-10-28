// ���� �̸�: PauseManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // �� ��ȯ�� ���� �ʿ�

/// <summary>
/// ESC Ű�� ���� ������ �Ͻ�����/�簳�ϴ� ����� �����մϴ�.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("UI ����")]
    [Tooltip("�Ͻ����� �� ��� UI �г��� �������ּ���.")]
    public GameObject pausePanel; // �ν����Ϳ��� 'PausePanel' UI�� ����

    [Header("�� ����")]
    [Tooltip("���ư� ���� �޴� ���� �̸��� ��Ȯ�� �Է��ϼ���.")]
    public string mainMenuSceneName; // ��: "MainMenu"

    // ���� ������ �Ͻ����� �������� �����ϴ� ����
    private bool isPaused = false;

    /// <summary>
    /// ���� ���� ��(�� �ε� ��) ȣ��˴ϴ�.
    /// </summary>
    void Start()
    {
        // 1. ������ ���� �׻� �Ͻ����� �г��� ����ϴ�.
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // 2. ������ ���� �׻� ���� �ð��� ���������� �帣�� �մϴ�.
        Time.timeScale = 1f;

        // 3. ������ ���� �Ͻ����� ���°� �ƴմϴ�.
        isPaused = false;

        // 4. (����) PlayerShooting.cs�� �̹� Ŀ���� ��װ� ����� ���� ���Դϴ�.
    }

    /// <summary>
    // �� �����Ӹ��� ȣ��˴ϴ�.
    /// </summary>
    void Update()
    {
        // 1. 'Escape' Ű�� �������� �����մϴ�.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 2. ���� �Ͻ����� �������� Ȯ���մϴ�.
            if (isPaused)
            {
                // �̹� ���� ���� -> '����ϱ�(Resume)'�� ȣ��
                ResumeGame();
            }
            else
            {
                // ���� ���°� �ƴ� -> '�Ͻ�����(Pause)'�� ȣ��
                PauseGame();
            }
        }
    }

    /// <summary>
    /// ������ �Ͻ�������Ű�� �Լ��Դϴ�. (ESC Ű �Ǵ� ��ư Ŭ������ ȣ�� ����)
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;

        // 1. �Ͻ����� �г��� �մϴ�.
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // 2. ������ �ð��� 0������� ����� '����'��ŵ�ϴ�.
        // (Time.timeScale = 0f �̸� FixedUpdate�� ���߰�, Update�� ��� ����˴ϴ�)
        Time.timeScale = 0f;

        // 3. ���콺 Ŀ���� ���̰� �ϰ�, ����� �����մϴ� (��ư Ŭ���� ����)
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// ������ �簳��Ű�� �Լ��Դϴ�. ('����ϱ�' ��ư�� ȣ���� �Լ�)
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;

        // 1. �Ͻ����� �г��� ���ϴ�.
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // 2. ������ �ð��� �ٽ� 1���(����)���� �ǵ����ϴ�.
        Time.timeScale = 1f;

        // 3. ���콺 Ŀ���� �ٽ� ��װ� ����ϴ� (���� �÷��̸� ����)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// '���� �޴���' ��ư�� ȣ���� �Լ��Դϴ�.
    /// </summary>
    public void GoToMainMenu()
    {
        // �ڡڡ� (�ſ� �߿�) �ڡڡ�
        // ���� ������ ���� �ݵ�� ���� �ð��� 1������� �ǵ����� �մϴ�.
        // �׷��� ������ ���� �޴� ���� 0���(���� ����)���� �ε�˴ϴ�.
        Time.timeScale = 1f;

        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("���� �޴� �� �̸�(Main Menu Scene Name)�� �������� �ʾҽ��ϴ�!");
        }
    }
}