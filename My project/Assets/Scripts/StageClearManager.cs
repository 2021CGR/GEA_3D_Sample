// ���� �̸�: StageClearManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageClearManager : MonoBehaviour
{
    [Header("Ŭ���� ����")]
    [Tooltip("�� ����ŭ ���� ������ Ŭ����˴ϴ�. (���� ���������� 1�� ����)")]
    public int killsToClear = 5;

    [Header("���� �� ����")]
    [Tooltip("�Ϲ� �������� Ŭ���� �� �̵��� ���� �̸�")]
    public string nextSceneName;
    [Tooltip("���� �޴� ���� �̸� (Ŭ���� �г� ��ư��)")]
    public string mainMenuSceneName;
    [Tooltip("����� �� �̵��� �������� 1 ���� �̸�")]
    public string stage1SceneName;

    [Header("UI ����")]
    [Tooltip("���� ���� ���� ���� ǥ���� Text UI")]
    public Text killCountText;
    [Tooltip("Ŭ���� �� ��� UI �г� (���� �������� ����)")]
    public GameObject clearPanel;

    private int currentKills = 0;

    void OnEnable()
    {
        Enemy.OnEnemyKilled += HandleEnemyKilled;
    }
    void OnDisable()
    {
        Enemy.OnEnemyKilled -= HandleEnemyKilled;
    }
    void Start()
    {
        currentKills = 0;
        UpdateKillCountUI();
        if (clearPanel != null)
        {
            clearPanel.SetActive(false);
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// ���� �׾��ٴ� ��ȣ�� �޾��� �� ����� �Լ��Դϴ�.
    /// </summary>
    void HandleEnemyKilled()
    {
        currentKills++;
        UpdateKillCountUI();

        if (currentKills >= killsToClear)
        {
            if (clearPanel != null)
            {
                // ���� �������� Ŭ����: �гθ� ��� (���� ���� ���� X)
                ShowClearPanel();
            }
            else
            {
                // �������������������� [������ ����] ��������������������
                // �Ϲ� �������� Ŭ����: ���� ������ 1 ������Ű�� ���� �� �ε�
                IncrementWeaponCount();
                LoadNextScene();
                // ���������������������������������������������������
            }
        }
    }

    void ShowClearPanel()
    {
        if (clearPanel == null) return;
        clearPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("�������� Ŭ����! Ŭ���� �г��� ���ϴ�.");
    }

    /// <summary>
    /// ���� ���� �ε��ϴ� �Լ��Դϴ�.
    /// </summary>
    void LoadNextScene()
    {
        Time.timeScale = 1f;
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("���� �� �̸�(Next Scene Name)�� �������� �ʾҽ��ϴ�!");
        }
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        // (���� �޴��� �� ���� ���� ������ ������ �ʿ� ����. 
        //  ������ ���� �޴��� 'StartGame' ��ư�� ������ �����)
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError("���� �޴� �� �̸�(Main Menu Scene Name)�� �������� �ʾҽ��ϴ�!");
        }
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // �������������������� [�߰��� ����] ��������������������
        // (���� Ŭ���� ��) '���� �����'�� '�� ����'�� �����Ƿ� ���� ������ 1�� ����
        ResetWeaponCount();
        // ���������������������������������������������������

        if (!string.IsNullOrEmpty(stage1SceneName))
        {
            SceneManager.LoadScene(stage1SceneName);
        }
        else
        {
            Debug.LogError("�������� 1 �� �̸�(Stage 1 Scene Name)�� �������� �ʾҽ��ϴ�!");
        }
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("������ �����մϴ�.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void UpdateKillCountUI()
    {
        if (killCountText != null)
        {
            killCountText.text = "Kills: " + currentKills + " / " + killsToClear;
        }
    }

    // �������������������� [�߰��� ���� �Լ�] ��������������������
    /// <summary>
    /// GameProgressManager�� ã�� ���� ������ 1 ������ŵ�ϴ�.
    /// </summary>
    private void IncrementWeaponCount()
    {
        if (GameProgressManager.instance != null)
        {
            GameProgressManager.instance.unlockedWeaponCount++;
            Debug.Log($"[StageClearManager] ���� �ر�! ���� �������� ���� ����: {GameProgressManager.instance.unlockedWeaponCount}");
        }
    }

    /// <summary>
    /// GameProgressManager�� ã�� ���� ������ 1�� �����մϴ�.
    /// </summary>
    private void ResetWeaponCount()
    {
        if (GameProgressManager.instance != null)
        {
            GameProgressManager.instance.unlockedWeaponCount = 1;
            Debug.Log("[StageClearManager] ���� ���� 1�� ����.");
        }
    }
    // ���������������������������������������������������
}