// ���� �̸�: MainMenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // ���� ��ȯ�ϴ� ����� ����ϱ� ���� �� �ʿ��մϴ�.
using UnityEngine.UI;              // Slider�� ����ϱ� ���� �߰��մϴ�.
using UnityEngine.Audio;         // AudioMixer�� ����ϱ� ���� �߰��մϴ�.

public class MainMenuManager : MonoBehaviour
{
    [Header("�̵��� �� �̸�")]
    [Tooltip("Start ��ư�� ������ �� �̵��� ���� ���� �̸��� ��Ȯ�ϰ� �Է��ϼ���.")]
    public string gameSceneName; // �ν����� â���� ���� ���� �̸��� ������ �� �ֵ��� public ������ ����ϴ�.

    [Header("�ɼ� UI �г�")]
    [Tooltip("Option ��ư�� ������ �� �Ѱ� �� UI �г��� ���⿡ �������ּ���.")]
    public GameObject optionsPanel; // �ɼ� â���� ����� UI �г��� �����ϱ� ���� ����

    // �������������������� [�߰��� ����] ���� ���� ��������������������
    [Header("����� ����")]
    [Tooltip("BGM, SFX �׷��� �ִ� ���� ����� �ͼ��� �������ּ���.")]
    public AudioMixer mainMixer;
    [Tooltip("BGM ���� ������ ����� UI �����̴��� �������ּ���.")]
    public Slider bgmSlider;
    [Tooltip("SFX ���� ������ ����� UI �����̴��� �������ּ���.")]
    public Slider sfxSlider;

    // AudioMixer�� �����Ų �Ķ������ �̸� (���ڿ��̹Ƿ� ��Ÿ�� �����ؾ� �մϴ�)
    public const string BGM_VOLUME_KEY = "BGMVolume"; // ��: "BGMVolume"
    public const string SFX_VOLUME_KEY = "SFXVolume"; // ��: "SFXVolume"

    // ���� ���� ������ �� ����� PlayerPrefs Ű �̸�
    private const string BGM_PREF_KEY = "BGMVolPref";
    private const string SFX_PREF_KEY = "SFXVolPref";
    // ���������������������������������������������������

    /// <summary>
    /// ��ũ��Ʈ�� ó�� ���۵� �� ȣ��˴ϴ�. (Awake ��� Start�� ���)
    /// </summary>
    void Start()
    {
        // �������������������� [�߰��� ����] ����� ���� �ҷ����� ��������������������
        // BGM �����̴��� �ͼ� ����
        if (bgmSlider != null)
        {
            // PlayerPrefs���� BGM ���� ���� �ҷ��ɴϴ�. ����� ���� ������ 0.75f (75%)�� �⺻������ ����մϴ�.
            float savedBGMVol = PlayerPrefs.GetFloat(BGM_PREF_KEY, 0.75f);
            bgmSlider.value = savedBGMVol; // �����̴� ���� �ҷ��� ������ ����
            SetBGMVolume(savedBGMVol);     // ����� �ͼ����� �ش� ���� ����
        }

        // SFX �����̴��� �ͼ� ����
        if (sfxSlider != null)
        {
            // PlayerPrefs���� SFX ���� ���� �ҷ��ɴϴ�. ����� ���� ������ 0.75f (75%)�� �⺻������ ����մϴ�.
            float savedSFXVol = PlayerPrefs.GetFloat(SFX_PREF_KEY, 0.75f);
            sfxSlider.value = savedSFXVol; // �����̴� ���� �ҷ��� ������ ����
            SetSFXVolume(savedSFXVol);     // ����� �ͼ����� �ش� ���� ����
        }
        // ���������������������������������������������������
    }

    /// <summary>
    /// 'Start' ��ư�� Ŭ������ �� ȣ��� �Լ��Դϴ�. (������ ����)
    /// </summary>
    public void StartGame()
    {
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
    /// 'Option' ��ư�� Ŭ������ �� ȣ��� �Լ��Դϴ�. (������ ����)
    /// </summary>
    public void OpenOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// �ɼ� �г� �ȿ� �ִ� 'Close' ��ư�� Ŭ������ �� ȣ��� �Լ��Դϴ�. (������ ����)
    /// </summary>
    public void CloseOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 'Quit' ��ư�� Ŭ������ �� ȣ��� �Լ��Դϴ�. (������ ����)
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("������ �����մϴ�.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // �������������������� [�߰��� �Լ�] ���� ���� �Լ� ��������������������

    /// <summary>
    /// BGM �����̴��� ���� ����� �� ȣ��� �Լ��Դϴ�. (public�̾�� ��)
    /// </summary>
    public void OnBGMVolumeChanged()
    {
        if (bgmSlider == null) return;

        float volume = bgmSlider.value; // �����̴��� ���� �� (0.0001 ~ 1)
        SetBGMVolume(volume);

        // ����� ���� PlayerPrefs�� ��� �����մϴ�.
        PlayerPrefs.SetFloat(BGM_PREF_KEY, volume);
    }

    /// <summary>
    /// SFX �����̴��� ���� ����� �� ȣ��� �Լ��Դϴ�. (public�̾�� ��)
    /// </summary>
    public void OnSFXVolumeChanged()
    {
        if (sfxSlider == null) return;

        float volume = sfxSlider.value; // �����̴��� ���� �� (0.0001 ~ 1)
        SetSFXVolume(volume);

        // ����� ���� PlayerPrefs�� ��� �����մϴ�.
        PlayerPrefs.SetFloat(SFX_PREF_KEY, volume);
    }

    /// <summary>
    /// ���� AudioMixer�� BGM ������ �����ϴ� ���� �Լ�
    /// </summary>
    private void SetBGMVolume(float volume)
    {
        if (mainMixer == null) return;

        // �����̴� ��(0~1)�� ���ú�(-80~0)�� ��ȯ�մϴ�.
        // Log10(0)�� ���� ���Ѵ��̹Ƿ�, ������ 0�� ���� -80dB(���Ұ�)�� ó���մϴ�.
        float db = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;

        // AudioMixer�� "BGMVolume" �Ķ���� ���� �����մϴ�.
        mainMixer.SetFloat(BGM_VOLUME_KEY, db);
    }

    /// <summary>
    /// ���� AudioMixer�� SFX ������ �����ϴ� ���� �Լ�
    /// </summary>
    private void SetSFXVolume(float volume)
    {
        if (mainMixer == null) return;

        float db = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;

        // AudioMixer�� "SFXVolume" �Ķ���� ���� �����մϴ�.
        mainMixer.SetFloat(SFX_VOLUME_KEY, db);
    }
    // ���������������������������������������������������
}