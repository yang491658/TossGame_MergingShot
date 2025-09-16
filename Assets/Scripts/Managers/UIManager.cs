using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Awake()
    {
        if (scoreText == null)
            scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
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
        UpdateScore(GameManager.Instance.GetScore());
    }

    private void UpdateScore(int _score)
    {
        if (scoreText != null)
            scoreText.text = _score.ToString("00000");
    }
}
