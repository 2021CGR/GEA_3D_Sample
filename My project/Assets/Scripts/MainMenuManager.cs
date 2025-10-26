// 파일 이름: MainMenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MainMenuManager : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    [Tooltip("Start 버튼을 눌렀을 때 이동할 게임 씬의 이름을 정확하게 입력하세요.")]
    public string gameSceneName;

    [Header("옵션 UI 패널")]
    [Tooltip("Option 버튼을 눌렀을 때 켜고 끌 UI 패널을 여기에 연결해주세요.")]
    public GameObject optionsPanel;

    [Header("오디오 설정")]
    [Tooltip("BGM, SFX 그룹이 있는 메인 오디오 믹서를 연결해주세요.")]
    public AudioMixer mainMixer;
    [Tooltip("BGM 볼륨 조절에 사용할 UI 슬라이더를 연결해주세요.")]
    public Slider bgmSlider;
    [Tooltip("SFX 볼륨 조절에 사용할 UI 슬라이더를 연결해주세요.")]
    public Slider sfxSlider;

    public const string BGM_VOLUME_KEY = "BGMVolume";
    public const string SFX_VOLUME_KEY = "SFXVolume";

    private const string BGM_PREF_KEY = "BGMVolPref";
    private const string SFX_PREF_KEY = "SFXVolPref";

    void Start()
    {
        // ... (기존 볼륨 로드 로직은 그대로) ...
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
    /// 'Start' 버튼을 클릭했을 때 호출될 함수입니다.
    /// </summary>
    public void StartGame()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [추가된 로직] ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // 게임을 새로 시작할 때, GameProgressManager를 찾아
        // 해금된 무기 개수를 '1' (초기값)로 리셋합니다.
        if (GameProgressManager.instance != null)
        {
            GameProgressManager.instance.unlockedWeaponCount = 1;
            Debug.Log("[MainMenu] 무기 개수 1로 리셋.");
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

        if (!string.IsNullOrEmpty(gameSceneName))
        {
            Debug.Log(gameSceneName + " 씬을 불러옵니다...");
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("게임 씬 이름이 지정되지 않았습니다!");
        }
    }

    // ... (OpenOptions, CloseOptions, QuitGame, 볼륨 조절 함수 등은 모두 기존과 동일) ...
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
        Debug.Log("게임을 종료합니다.");
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