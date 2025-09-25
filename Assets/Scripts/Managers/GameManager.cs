using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score Info.")]
    [SerializeField] private int totalScore = 0;
    public event System.Action<int> OnScoreChanged;

    public bool IsPaused { get; private set; } = false;
    public event System.Action<bool> OnSettingOpened;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        SpawnManager.Instance.Spawn(1);
    }

    #region 점수
    public void AddScore(int _score)
    {
        totalScore += _score;
        OnScoreChanged?.Invoke(totalScore);
    }

    public void ResetScore()
    {
        totalScore = 0;
        OnScoreChanged?.Invoke(totalScore);
    }
    #endregion

    #region 진행
    public void Pause()
    {
        if (IsPaused) return;

        IsPaused = true;
        Time.timeScale = 0f;
        OnSettingOpened?.Invoke(true);
    }

    public void Resume()
    {
        if (!IsPaused) return;

        IsPaused = false;
        Time.timeScale = 1f;
        OnSettingOpened?.Invoke(false);
    }

    public void TogglePause()
    {
        if (IsPaused) Resume();
        else Pause();
    }

    public void Replay()
    {
        Resume();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
    #endregion

    #region GET
    public int GetTotalScore() => totalScore;
    #endregion
}
