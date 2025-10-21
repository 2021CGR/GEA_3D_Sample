// 파일 이름: MainMenuManager.cs
using UnityEngine;
using UnityEngine.SceneManagement; // 씬을 전환하는 기능을 사용하기 위해 꼭 필요합니다.
using UnityEngine.UI;              // Slider를 사용하기 위해 추가합니다.
using UnityEngine.Audio;         // AudioMixer를 사용하기 위해 추가합니다.

public class MainMenuManager : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    [Tooltip("Start 버튼을 눌렀을 때 이동할 게임 씬의 이름을 정확하게 입력하세요.")]
    public string gameSceneName; // 인스펙터 창에서 게임 씬의 이름을 설정할 수 있도록 public 변수로 만듭니다.

    [Header("옵션 UI 패널")]
    [Tooltip("Option 버튼을 눌렀을 때 켜고 끌 UI 패널을 여기에 연결해주세요.")]
    public GameObject optionsPanel; // 옵션 창으로 사용할 UI 패널을 연결하기 위한 변수

    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [추가된 변수] 볼륨 설정 ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
    [Header("오디오 설정")]
    [Tooltip("BGM, SFX 그룹이 있는 메인 오디오 믹서를 연결해주세요.")]
    public AudioMixer mainMixer;
    [Tooltip("BGM 볼륨 조절에 사용할 UI 슬라이더를 연결해주세요.")]
    public Slider bgmSlider;
    [Tooltip("SFX 볼륨 조절에 사용할 UI 슬라이더를 연결해주세요.")]
    public Slider sfxSlider;

    // AudioMixer에 노출시킨 파라미터의 이름 (문자열이므로 오타에 주의해야 합니다)
    public const string BGM_VOLUME_KEY = "BGMVolume"; // 예: "BGMVolume"
    public const string SFX_VOLUME_KEY = "SFXVolume"; // 예: "SFXVolume"

    // 볼륨 값을 저장할 때 사용할 PlayerPrefs 키 이름
    private const string BGM_PREF_KEY = "BGMVolPref";
    private const string SFX_PREF_KEY = "SFXVolPref";
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

    /// <summary>
    /// 스크립트가 처음 시작될 때 호출됩니다. (Awake 대신 Start를 사용)
    /// </summary>
    void Start()
    {
        // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [추가된 로직] 저장된 볼륨 불러오기 ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼
        // BGM 슬라이더와 믹서 설정
        if (bgmSlider != null)
        {
            // PlayerPrefs에서 BGM 볼륨 값을 불러옵니다. 저장된 값이 없으면 0.75f (75%)를 기본값으로 사용합니다.
            float savedBGMVol = PlayerPrefs.GetFloat(BGM_PREF_KEY, 0.75f);
            bgmSlider.value = savedBGMVol; // 슬라이더 값을 불러온 값으로 설정
            SetBGMVolume(savedBGMVol);     // 오디오 믹서에도 해당 값을 적용
        }

        // SFX 슬라이더와 믹서 설정
        if (sfxSlider != null)
        {
            // PlayerPrefs에서 SFX 볼륨 값을 불러옵니다. 저장된 값이 없으면 0.75f (75%)를 기본값으로 사용합니다.
            float savedSFXVol = PlayerPrefs.GetFloat(SFX_PREF_KEY, 0.75f);
            sfxSlider.value = savedSFXVol; // 슬라이더 값을 불러온 값으로 설정
            SetSFXVolume(savedSFXVol);     // 오디오 믹서에도 해당 값을 적용
        }
        // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
    }

    /// <summary>
    /// 'Start' 버튼을 클릭했을 때 호출될 함수입니다. (기존과 동일)
    /// </summary>
    public void StartGame()
    {
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

    /// <summary>
    /// 'Option' 버튼을 클릭했을 때 호출될 함수입니다. (기존과 동일)
    /// </summary>
    public void OpenOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 옵션 패널 안에 있는 'Close' 버튼을 클릭했을 때 호출될 함수입니다. (기존과 동일)
    /// </summary>
    public void CloseOptions()
    {
        if (optionsPanel != null)
        {
            optionsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 'Quit' 버튼을 클릭했을 때 호출될 함수입니다. (기존과 동일)
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼ [추가된 함수] 볼륨 조절 함수 ▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼▼

    /// <summary>
    /// BGM 슬라이더의 값이 변경될 때 호출될 함수입니다. (public이어야 함)
    /// </summary>
    public void OnBGMVolumeChanged()
    {
        if (bgmSlider == null) return;

        float volume = bgmSlider.value; // 슬라이더의 현재 값 (0.0001 ~ 1)
        SetBGMVolume(volume);

        // 변경된 값을 PlayerPrefs에 즉시 저장합니다.
        PlayerPrefs.SetFloat(BGM_PREF_KEY, volume);
    }

    /// <summary>
    /// SFX 슬라이더의 값이 변경될 때 호출될 함수입니다. (public이어야 함)
    /// </summary>
    public void OnSFXVolumeChanged()
    {
        if (sfxSlider == null) return;

        float volume = sfxSlider.value; // 슬라이더의 현재 값 (0.0001 ~ 1)
        SetSFXVolume(volume);

        // 변경된 값을 PlayerPrefs에 즉시 저장합니다.
        PlayerPrefs.SetFloat(SFX_PREF_KEY, volume);
    }

    /// <summary>
    /// 실제 AudioMixer의 BGM 볼륨을 조절하는 내부 함수
    /// </summary>
    private void SetBGMVolume(float volume)
    {
        if (mainMixer == null) return;

        // 슬라이더 값(0~1)을 데시벨(-80~0)로 변환합니다.
        // Log10(0)은 음의 무한대이므로, 볼륨이 0일 때는 -80dB(음소거)로 처리합니다.
        float db = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;

        // AudioMixer의 "BGMVolume" 파라미터 값을 변경합니다.
        mainMixer.SetFloat(BGM_VOLUME_KEY, db);
    }

    /// <summary>
    /// 실제 AudioMixer의 SFX 볼륨을 조절하는 내부 함수
    /// </summary>
    private void SetSFXVolume(float volume)
    {
        if (mainMixer == null) return;

        float db = (volume <= 0.0001f) ? -80f : Mathf.Log10(volume) * 20f;

        // AudioMixer의 "SFXVolume" 파라미터 값을 변경합니다.
        mainMixer.SetFloat(SFX_VOLUME_KEY, db);
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲
}