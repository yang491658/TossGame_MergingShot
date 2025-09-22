using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI menuScoreText;

    [Header("Pause UI")]
    [SerializeField] private Button menuBtn;
    [SerializeField] private Image menuUI;

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
        if (menuScoreText == null)
            menuScoreText = GameObject.Find("Box/ScoreText")?.GetComponent<TextMeshProUGUI>();

        if (menuBtn == null)
            menuBtn = GameObject.Find("MenuBtn")?.GetComponent<Button>();
        if (menuUI == null)
            menuUI = GameObject.Find("MenuUI")?.GetComponent<Image>();

        bgmIcons.Clear();
        LoadSprite(bgmIcons, "White Music");
        LoadSprite(bgmIcons, "White Music Off");
        sfxIcons.Clear();
        LoadSprite(sfxIcons, "White Sound On");
        LoadSprite(sfxIcons, "White Sound Icom");
        LoadSprite(sfxIcons, "White Sound Off 2");

        if (bgmIcon == null)
            bgmIcon = GameObject.Find("BGM/Btn/Icon")?.GetComponent<Image>();
        if (sfxIcon == null)
            sfxIcon = GameObject.Find("SFX/Btn/Icon")?.GetComponent<Image>();
        if (bgmSlider == null)
            bgmSlider = GameObject.Find("BGM/Slider")?.GetComponent<Slider>();
        if (sfxSlider == null)
            sfxSlider = GameObject.Find("SFX/Slider")?.GetComponent<Slider>();
    }

    private static void LoadSprite(List<Sprite> _list, string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return;
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Imports/Dark UI/Icons" });
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var obj in assets)
            {
                var s = obj as Sprite;
                if (s != null && s.name == spriteName)
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
    }

    private void OnEnable()
    {
        GameManager.Instance.OnScoreChanged += UpdateScore;
        GameManager.Instance.OnMenuOpened += OpenMenu;

        if (bgmSlider != null)
        {
            bgmSlider.value = AudioManager.Instance.GetBGMVolume();
            bgmSlider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.value = AudioManager.Instance.GetSFXVolume();
            sfxSlider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
        }

        AudioManager.Instance.OnVolumeChanged += ChangeVolume;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnScoreChanged -= UpdateScore;
        GameManager.Instance.OnMenuOpened -= OpenMenu;

        if (bgmSlider != null)
            bgmSlider.onValueChanged.RemoveListener(AudioManager.Instance.SetBGMVolume);
        if (sfxSlider != null)
            sfxSlider.onValueChanged.RemoveListener(AudioManager.Instance.SetSFXVolume);

        AudioManager.Instance.OnVolumeChanged -= ChangeVolume;
    }

    private void Start()
    {
        UpdateScore(GameManager.Instance.GetTotalScore());
        OpenMenu(GameManager.Instance.IsPaused);
        UpdateIcons();
    }

    private void UpdateScore(int _score)
    {
        if (scoreText != null)
            scoreText.text = _score.ToString("0000");
        if (menuScoreText != null)
            menuScoreText.text = "Score : " + _score.ToString("0000");
    }

    private void OpenMenu(bool _paused)
    {
        if (scoreText != null) scoreText.gameObject.SetActive(!_paused);

        if (menuBtn != null) menuBtn.gameObject.SetActive(!_paused);
        if (menuUI != null) menuUI.gameObject.SetActive(_paused);
    }

    private void ChangeVolume(Sound _sound, float _volume)
    {
        if (_sound == Sound.BGM)
        {
            if (bgmSlider != null && !Mathf.Approximately(bgmSlider.value, _volume))
                bgmSlider.value = _volume;
        }
        else 
        {
            if (sfxSlider != null && !Mathf.Approximately(sfxSlider.value, _volume))
                sfxSlider.value = _volume;
        }
        UpdateIcons();
    }

    private void UpdateIcons()
    {
        if (bgmIcon != null && bgmIcons.Count >= 2)
            bgmIcon.sprite = AudioManager.Instance.IsBGMMuted() ? bgmIcons[1] : bgmIcons[0];

        if (sfxIcon != null && sfxIcons.Count >= 3)
        {
            if (AudioManager.Instance.IsSFXMuted())
                sfxIcon.sprite = sfxIcons[2];
            else if (AudioManager.Instance.GetSFXVolume() <= 0.2f)
                sfxIcon.sprite = sfxIcons[1];
            else
                sfxIcon.sprite = sfxIcons[0];
        }
    }
}
