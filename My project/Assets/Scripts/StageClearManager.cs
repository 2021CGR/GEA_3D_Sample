// ���� �̸�: StageClearManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageClearManager : MonoBehaviour
{
    [Header("Ŭ���� ����")]
    [Tooltip("�� ����ŭ ���� ������ Ŭ����˴ϴ�. (���� ���������� 1�� ����)")]
    public int killsToClear = 5;

    // �������������������� [�߰��� ����] ��������������������
    [Tooltip("�̰��� üũ�ϸ� ų ī��Ʈ�� 'Boss' �±׸� ���� ���� ī��Ʈ�մϴ�.")]
    public bool isBossStage = false;
    // ���������������������������������������������������

    [Header("���� �� ����")]
    public string nextSceneName;
    public string mainMenuSceneName;
    public string stage1SceneName;

    [Header("UI ����")]
    public Text killCountText;
    public GameObject clearPanel;

    private int currentKills = 0;

    void OnEnable()
    {
        // [������] GameObject�� �޴� �̺�Ʈ �ڵ鷯�� ����մϴ�.
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
    /// [������] ���� �׾��ٴ� ��ȣ�� �޾��� �� '����' �׾�����(enemyObject) Ȯ���մϴ�.
    /// </summary>
    void HandleEnemyKilled(GameObject enemyObject)
    {
        // 1. ���� �������� ������� Ȯ��
        if (isBossStage)
        {
            // ���� �����������, ���� ���� "Boss" �±׸� �������� Ȯ��
            if (enemyObject == null || !enemyObject.CompareTag("Boss"))
            {
                // "Boss" �±װ� ������(����̸�) ī��Ʈ���� �ʰ� �Լ� ����
                return;
            }
            // (�� �Ʒ� ������ ������ ���� �����)
        }

        // 2. �Ϲ� ���������ų�, ���� ������������ ������ �׾����� ī��Ʈ
        currentKills++;
        UpdateKillCountUI();

        // 3. Ŭ���� ���� Ȯ��
        if (currentKills >= killsToClear)
        {
            // (���� ����������� clearPanel�� null�� �ƴϾ�� ��)
            if (clearPanel != null && isBossStage)
            {
                ShowClearPanel();
            }
            // (�Ϲ� ����������� clearPanel�� null�̾�� ��)
            else if (clearPanel == null && !isBossStage)
            {
                IncrementWeaponCount();
                LoadNextScene();
            }
            // (�� �� ������ �߸��Ǿ��� ��츦 ����� �α�)
            else
            {
                Debug.LogWarning("StageClearManager�� isBossStage�� clearPanel ������ ���� �ʽ��ϴ�!");
            }
        }
    }

    // ... (ShowClearPanel, LoadNextScene, GoToMainMenu, RestartGame, QuitGame, UpdateKillCountUI, Increment/ResetWeaponCount �Լ��� ��� ������ �����մϴ�) ...

    void ShowClearPanel()
    {
        if (clearPanel == null) return;
        clearPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("�������� Ŭ����! Ŭ���� �г��� ���ϴ�.");
    }
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
        ResetWeaponCount();
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
    private void IncrementWeaponCount()
    {
        if (GameProgressManager.instance != null)
        {
            GameProgressManager.instance.unlockedWeaponCount++;
            Debug.Log($"[StageClearManager] ���� �ر�! ���� �������� ���� ����: {GameProgressManager.instance.unlockedWeaponCount}");
        }
    }
    private void ResetWeaponCount()
    {
        if (GameProgressManager.instance != null)
        {
            GameProgressManager.instance.unlockedWeaponCount = 1;
            Debug.Log("[StageClearManager] ���� ���� 1�� ����.");
        }
    }
}