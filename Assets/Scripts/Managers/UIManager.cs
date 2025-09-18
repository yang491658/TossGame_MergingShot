using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (scoreText == null)
            scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
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
    }

    private void OnDisable()
    {
        GameManager.Instance.OnScoreChanged -= UpdateScore;
    }

    private void Start()
    {
        UpdateScore(GameManager.Instance.GetTotalScore());
    }

    private void UpdateScore(int _score)
    {
        if (scoreText != null)
            scoreText.text = _score.ToString("0000");
    }
}
