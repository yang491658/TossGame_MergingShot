using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public struct PlanetSlot
{
    public GameObject go;
    public Image image;
    public TextMeshProUGUI tmp;

    public PlanetSlot(GameObject obj)
    {
        go = obj;
        image = obj.GetComponentInChildren<Image>();
        tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
    }
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("InGame UI")]
    [SerializeField] private GameObject inGameUI;
    [SerializeField] private Image nextImage;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button settingBtn;

    [Header("Setting UI")]
    [SerializeField] private GameObject settingUI;
    [SerializeField] private TextMeshProUGUI settingScoreText;
    public event System.Action<bool> OnOpenSetting;

    [Header("Confirm UI")]
    [SerializeField] private GameObject confirmUI;
    [SerializeField] private TextMeshProUGUI confirmText;
    private System.Action confirmAction;

    [Header("Sound UI")]
    [SerializeField] private Image bgmIcon;
    [SerializeField] private Image sfxIcon;
    [SerializeField] private List<Sprite> bgmIcons = new List<Sprite>();
    [SerializeField] private List<Sprite> sfxIcons = new List<Sprite>();
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Result UI")]
    [SerializeField] private GameObject resultUI;
    [SerializeField] private TextMeshProUGUI resultScoreText;
    [SerializeField] private List<PlanetSlot> planets = new List<PlanetSlot>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (inGameUI == null)
            inGameUI = GameObject.Find("InGameUI");
        if (scoreText == null)
            scoreText = GameObject.Find("InGameUI/Score/ScoreText").GetComponent<TextMeshProUGUI>();
        if (settingBtn == null)
            settingBtn = GameObject.Find("InGameUI/SettingBtn").GetComponent<Button>();
        if (nextImage == null)
            nextImage = GameObject.Find("InGameUI/Next/NextImage").GetComponent<Image>();

        if (settingUI == null)
            settingUI = GameObject.Find("SettingUI");
        if (settingScoreText == null)
            settingScoreText = GameObject.Find("SettingUI/Box/Score/ScoreText").GetComponent<TextMeshProUGUI>();

        if (confirmUI == null)
            confirmUI = GameObject.Find("ConfirmUI");
        if (confirmText == null)
            confirmText = GameObject.Find("ConfirmUI/Box/ConfirmText").GetComponent<TextMeshProUGUI>();

        bgmIcons.Clear();
        LoadSprite(bgmIcons, "White Music");
        LoadSprite(bgmIcons, "White Music Off");
        sfxIcons.Clear();
        LoadSprite(sfxIcons, "White Sound On");
        LoadSprite(sfxIcons, "White Sound Icon");
        LoadSprite(sfxIcons, "White Sound Off 2");

        if (bgmIcon == null)
            bgmIcon = GameObject.Find("BGM/BgmBtn/BgmIcon").GetComponent<Image>();
        if (sfxIcon == null)
            sfxIcon = GameObject.Find("SFX/SfxBtn/SfxIcon").GetComponent<Image>();
        if (bgmSlider == null)
            bgmSlider = GameObject.Find("BGM/BgmSlider").GetComponent<Slider>();
        if (sfxSlider == null)
            sfxSlider = GameObject.Find("SFX/SfxSlider").GetComponent<Slider>();

        if (resultUI == null)
            resultUI = GameObject.Find("ResultUI");
        if (resultScoreText == null)
            resultScoreText = GameObject.Find("ResultUI/Score/ScoreText").GetComponent<TextMeshProUGUI>();
        if (planets == null || planets.Count == 0)
            foreach (Transform child in GameObject.Find("ResultUI/Planets").transform)
                planets.Add(new PlanetSlot(child.gameObject));

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
        UpdateSlider(SoundType.BGM, SoundManager.Instance.GetBGMVolume());
        UpdateSlider(SoundType.SFX, SoundManager.Instance.GetSFXVolume());
        UpdateIcon();
        UpdateNext(EntityManager.Instance.GetNextSR());
    }

    private void OnEnable()
    {
        GameManager.Instance.OnChangeScore += UpdateScore;

        SoundManager.Instance.OnChangeVolume += UpdateSlider;
        bgmSlider.value = SoundManager.Instance.GetBGMVolume();
        bgmSlider.onValueChanged.AddListener(SoundManager.Instance.SetBGMVolume);
        sfxSlider.value = SoundManager.Instance.GetSFXVolume();
        sfxSlider.onValueChanged.AddListener(SoundManager.Instance.SetSFXVolume);

        EntityManager.Instance.OnChangeNext += UpdateNext;

        OnOpenSetting += GameManager.Instance.Pause;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnChangeScore -= UpdateScore;

        SoundManager.Instance.OnChangeVolume -= UpdateSlider;
        bgmSlider.onValueChanged.RemoveListener(SoundManager.Instance.SetBGMVolume);
        sfxSlider.onValueChanged.RemoveListener(SoundManager.Instance.SetSFXVolume);

        EntityManager.Instance.OnChangeNext -= UpdateNext;

        OnOpenSetting -= GameManager.Instance.Pause;
    }

    #region OPEN
    public void OpenSetting(bool _on)
    {
        if (settingUI == null) return;

        OnOpenSetting?.Invoke(_on);

        inGameUI.SetActive(!_on);
        settingUI.SetActive(_on);
    }

    public void OpenConfirm(bool _on, string _text = null, System.Action _action = null)
    {
        if (confirmUI == null) return;

        OnOpenSetting?.Invoke(_on);

        confirmUI.SetActive(_on);
        confirmText.text = $"{_text}하시겠습니까?";
        confirmAction = _action;

        if (!_on) confirmAction = null;
    }

    public void OpenResult(bool _on)
    {
        if (resultUI == null) return;

        OnOpenSetting?.Invoke(_on);

        resultUI.SetActive(_on);
        for (int i = 0; i < planets.Count; i++)
        {
            var u = EntityManager.Instance.GetDatas()[i];

            planets[i].go.name = u.unitName;
            planets[i].image.sprite = u.unitImage;
            planets[i].tmp.text = EntityManager.Instance.GetCount(u.unitID).ToString("×00");
        }

    }
    #endregion

    #region UPDATE
    public void UpdateScore(int _score)
    {
        scoreText.text = _score.ToString("0000");
        settingScoreText.text = _score.ToString("0000");
        resultScoreText.text = _score.ToString("0000");
    }

    private void UpdateSlider(SoundType _type, float _volume)
    {
        switch (_type)
        {
            case SoundType.BGM:
                if (!Mathf.Approximately(bgmSlider.value, _volume))
                    bgmSlider.value = _volume;
                break;

            case SoundType.SFX:
                if (!Mathf.Approximately(sfxSlider.value, _volume))
                    sfxSlider.value = _volume;
                break;

            default:
                return;
        }
        UpdateIcon();
    }

    private void UpdateIcon()
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

    private void UpdateNext(Sprite _sprite) => nextImage.sprite = _sprite;
    #endregion

    #region 버튼
    public void OnClickSetting() => OpenSetting(true);
    public void OnClickClose() => OpenSetting(false);
    public void OnClickBGM() => SoundManager.Instance.ToggleBGM();
    public void OnClickSFX() => SoundManager.Instance.ToggleSFX();
    public void OnClickReplay() => OpenConfirm(true, "다시", GameManager.Instance.Replay);
    public void OnClickQuit() => OpenConfirm(true, "종료", GameManager.Instance.Quit);
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
