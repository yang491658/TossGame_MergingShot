using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [Header("Pause UI")]
    [SerializeField] private Button menuBtn;
    [SerializeField] private Image menuUI;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (scoreText == null)
            scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();

        if (menuBtn == null)
            menuBtn = GameObject.Find("MenuBtn")?.GetComponent<Button>();
        if (menuUI == null)
            menuUI = GameObject.Find("MenuUI")?.GetComponent<Image>();
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
        GameManager.Instance.OnPauseChanged += OnPauseUI;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnScoreChanged -= UpdateScore;
        GameManager.Instance.OnPauseChanged -= OnPauseUI;
    }

    private void Start()
    {
        UpdateScore(GameManager.Instance.GetTotalScore());
        OnPauseUI(GameManager.Instance.IsPaused);
    }

    private void UpdateScore(int _score)
    {
        if (scoreText != null)
            scoreText.text = _score.ToString("0000");
    }

    private void OnPauseUI(bool _paused)
    {
        if (scoreText != null) scoreText.gameObject.SetActive(!_paused);

        if (menuBtn != null) menuBtn.gameObject.SetActive(!_paused);
        if (menuUI != null) menuUI.gameObject.SetActive(_paused);
    }
}
