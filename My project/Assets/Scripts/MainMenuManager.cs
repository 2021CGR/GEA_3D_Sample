// ���� �̸�: MainMenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenuManager : MonoBehaviour
{
    [Header("�̵��� �� �̸�")]
    [Tooltip("Start ��ư�� ������ �� �̵��� ���� ���� �̸��� ��Ȯ�ϰ� �Է��ϼ���.")]
    public string gameSceneName;

    [Header("�ɼ� UI �г�")]
    [Tooltip("Option ��ư�� ������ �� �Ѱ� �� UI �г��� ���⿡ �������ּ���.")]
    public GameObject optionsPanel;

    [Header("����� ����")]
    [Tooltip("BGM, SFX �׷��� �ִ� ���� ����� �ͼ��� �������ּ���.")]
    public AudioMixer mainMixer;
    [Tooltip("BGM ���� ������ ����� UI �����̴��� �������ּ���.")]
    public Slider bgmSlider;
    [Tooltip("SFX ���� ������ ����� UI �����̴��� �������ּ���.")]
    public Slider sfxSlider;

    public const string BGM_VOLUME_KEY = "BGMVolume";
    public const string SFX_VOLUME_KEY = "SFXVolume";

    private const string BGM_PREF_KEY = "BGMVolPref";
    private const string SFX_PREF_KEY = "SFXVolPref";

    void Start()
    {
        // ... (���� ���� �ε� ������ �״��) ...
        if (bgmSlider != null)
        {
            float savedBGMVol = PlayerPrefs.GetFloat(BGM_PREF_KEY, 0.75f);
            bgmSlider.value = savedBGMVol;
            SetBGMVolume(savedBGMVol);
        }
        if (sfxSlider != null)
        {
            float savedSFXVol = PlayerPrefs.GetFloat(SFX_PREF_KEY, 0.75f);
            sfxSlider.value = savedSFXVol;
            SetSFXVolume(savedSFXVol);
        }
    }

    /// <summary>
    /// 'Start' ��ư�� Ŭ������ �� ȣ��� �Լ��Դϴ�.
    /// </summary>
    public void StartGame()
    {
        // �������������������� [�߰��� ����] ��������������������
        // ������ ���� ������ ��, GameProgressManager�� ã��
        // �رݵ� ���� ������ '1' (�ʱⰪ)�� �����մϴ�.
        if (GameProgressManager.instance != null)
        {
            GameProgressManager.instance.unlockedWeaponCount = 1;
            Debug.Log("[MainMenu] ���� ���� 1�� ����.");
        }
        // ���������������������������������������������������

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

    // ... (OpenOptions, CloseOptions, QuitGame, ���� ���� �Լ� ���� ��� ������ ����) ...
    public void OpenOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }
    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
    }
    public void QuitGame()
    {
        Debug.Log("������ �����մϴ�.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    public void OnBGMVolumeChanged()
    {
        if (bgmSlider == null) return;
        float volume = bgmSlider.value;
        SetBGMVolume(volume);
        PlayerPrefs.SetFloat(BGM_PREF_KEY, volume);
    }
    public void OnSFXVolumeChanged()
    {
        if (sfxSlider == null) return;
        float volume = sfxSlider.value;
        SetSFXVolume(volume);
        PlayerPrefs.SetFloat(SFX_PREF_KEY, volume);
    }
    private void SetBGMVolume(float volume)
    {
        if (mainMixer == null) return;
        float db = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;
        mainMixer.SetFloat(BGM_VOLUME_KEY, db);
    }
    private void SetSFXVolume(float volume)
    {
        if (mainMixer == null) return;
        float db = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;
        mainMixer.SetFloat(SFX_VOLUME_KEY, db);
    }
}