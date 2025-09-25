using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Menu UI")]
    [SerializeField] private Button settingBtn;
    [SerializeField] private Image settingUI;
    [SerializeField] private TextMeshProUGUI settingScoreText;

    [Header("Confirm UI")]
    [SerializeField] private Image confirmUI;
    [SerializeField] private TextMeshProUGUI confirmText;
    private System.Action confirmAction;

    [Header("Audio UI")]
    [SerializeField] private Image bgmIcon;
    [SerializeField] private Image sfxIcon;
    [SerializeField] private List<Sprite> bgmIcons = new List<Sprite>();
    [SerializeField] private List<Sprite> sfxIcons = new List<Sprite>();
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (scoreText == null)
            scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();

        if (settingBtn == null)
            settingBtn = GameObject.Find("SettingBtn")?.GetComponent<Button>();
        if (settingUI == null)
            settingUI = GameObject.Find("SettingUI")?.GetComponent<Image>();
        if (settingScoreText == null)
            settingScoreText = GameObject.Find("SettingUI/SettingBox/ScoreText")?.GetComponent<TextMeshProUGUI>();

        if (confirmUI == null)
            confirmUI = GameObject.Find("ConfirmUI")?.GetComponent<Image>();
        if (confirmText == null)
            confirmText = GameObject.Find("ConfirmUI/ConfirmBox/ConfirmText")?.GetComponent<TextMeshProUGUI>();

        bgmIcons.Clear();
        LoadSprite(bgmIcons, "White Music");
        LoadSprite(bgmIcons, "White Music Off");
        sfxIcons.Clear();
        LoadSprite(sfxIcons, "White Sound On");
        LoadSprite(sfxIcons, "White Sound Icom");
        LoadSprite(sfxIcons, "White Sound Off 2");

        if (bgmIcon == null)
            bgmIcon = GameObject.Find("BGM/BgmBtn/BgmIcon")?.GetComponent<Image>();
        if (sfxIcon == null)
            sfxIcon = GameObject.Find("SFX/SfxBtn/SfxIcon")?.GetComponent<Image>();
        if (bgmSlider == null)
            bgmSlider = GameObject.Find("BGM/BgmSlider")?.GetComponent<Slider>();
        if (sfxSlider == null)
            sfxSlider = GameObject.Find("SFX/SfxSlider")?.GetComponent<Slider>();
    }

    private static void LoadSprite(List<Sprite> _list, string _sprite)
    {
        if (string.IsNullOrEmpty(_sprite)) return;
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Imports/Dark UI/Icons" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var obj in assets)
            {
                var s = obj as Sprite;
                if (s != null && s.name == _sprite)
                {
                    _list.Add(s);
                    return;
                }
            }
        }
    }
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateScore(GameManager.Instance.GetTotalScore());
        OpenConfirm(false);
        OpenSetting(false);
        UpdateIcons();
    }

    private void OnEnable()
    {
        GameManager.Instance.OnChangeScore += UpdateScore;
        GameManager.Instance.OnOpenSetting += OpenSetting;

        bgmSlider.value = SoundManager.Instance.GetBGMVolume();
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.SetBGMVolume);
        sfxSlider.value = SoundManager.Instance.GetSFXVolume();
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.SetSFXVolume);

        SoundManager.Instance.OnChangeVolume += ChangeVolume;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnChangeScore -= UpdateScore;
        GameManager.Instance.OnOpenSetting -= OpenSetting;

        bgmSlider.onValueChanged.RemoveListener(SoundManager.Instance.SetBGMVolume);
        sfxSlider.onValueChanged.RemoveListener(SoundManager.Instance.SetSFXVolume);

        SoundManager.Instance.OnChangeVolume -= ChangeVolume;
    }

    private void UpdateScore(int _score)
    {
        scoreText.text = _score.ToString("0000");
        settingScoreText.text = "Score : " + _score.ToString("0000");
    }

    private void OpenSetting(bool _on)
    {
        scoreText.gameObject.SetActive(!_on);

        settingBtn.gameObject.SetActive(!_on);
        settingUI.gameObject.SetActive(_on);
    }

    private void OpenConfirm(bool _on, string _text = null, System.Action _onOkay = null)
    {
        scoreText.gameObject.SetActive(!_on);

        settingBtn.gameObject.SetActive(!_on);
        settingUI.gameObject.SetActive(!_on);

        confirmUI.gameObject.SetActive(_on);
        if (_on)
        {
            confirmText.text = $"{_text}하시겠습니까?";
            confirmAction = _onOkay;
        }
        else
            confirmAction = null;
    }

    private void ChangeVolume(Sound _sound, float _volume)
    {
        if (_sound == Sound.BGM)
        {
            if (!Mathf.Approximately(bgmSlider.value, _volume))
                bgmSlider.value = _volume;
        }
        else
        {
            if (!Mathf.Approximately(sfxSlider.value, _volume))
                sfxSlider.value = _volume;
        }
        UpdateIcons();
    }

    private void UpdateIcons()
    {
        if (bgmIcons.Count >= 2)
            bgmIcon.sprite = SoundManager.Instance.IsBGMMuted() ? bgmIcons[1] : bgmIcons[0];

        if (sfxIcons.Count >= 3)
        {
            if (SoundManager.Instance.IsSFXMuted())
                sfxIcon.sprite = sfxIcons[2];
            else if (SoundManager.Instance.GetSFXVolume() < 0.2f)
                sfxIcon.sprite = sfxIcons[1];
            else
                sfxIcon.sprite = sfxIcons[0];
        }
    }

    #region 버튼
    public void OnClikSetting() => OpenSetting(true);
    public void OnClikClose() => OpenSetting(false);
    public void OnClikBGM() => SoundManager.Instance.ToggleBGM();
    public void OnClikSFX() => SoundManager.Instance.ToggleSFX();
    public void OnClikReplay() => OpenConfirm(true, "다시", GameManager.Instance.Replay);
    public void OnClikQuit() => OpenConfirm(true, "종료", GameManager.Instance.Quit);
    public void OnClickOkay() => StartCoroutine(PlayClickThen(confirmAction));
    public void OnClickCancel() => OpenConfirm(false);
    #endregion
    
    private IEnumerator PlayClickThen(System.Action _action)
    {
        SoundManager.Instance.Button();
        float len = SoundManager.Instance.GetSFXLength("Button");
        if (len > 0f) yield return new WaitForSecondsRealtime(len);
        _action?.Invoke();
    }
}
