using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score")]
    [SerializeField] private int totalScore = 0;
    public event System.Action<int> OnChangeScore;

    public bool IsPaused { get; private set; } = false;
    public event System.Action<bool> OnOpenSetting;

    public bool IsGameOver { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += LoadGame;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= LoadGame;
    }

    private void LoadGame(Scene _scene, LoadSceneMode _mode)
    {
        Pause(false);
        StartCoroutine(LoadCoroutine());
    }

    private IEnumerator LoadCoroutine()
    {
        IsGameOver = false;
        ResetScore();

        SoundManager.Instance.PlayBGM("Default");

        UIManager.Instance.OpenResult(false);
        UIManager.Instance.OpenConfirm(false);
        UIManager.Instance.OpenSetting(false);

        EntityManager.Instance.ResetCount();
        EntityManager.Instance.SetEntity();
        EntityManager.Instance.Spawn(1);

        yield return null;
    }

    #region 점수
    public void AddScore(int _score)
    {
        totalScore += _score;
        OnChangeScore?.Invoke(totalScore);
    }

    public void ResetScore()
    {
        totalScore = 0;
        OnChangeScore?.Invoke(totalScore);
    }
    #endregion

    #region 진행
    public void Pause(bool _pause)
    {
        if (IsPaused == _pause) return;

        IsPaused = _pause;
        Time.timeScale = _pause ? 0f : 1f;

        OnOpenSetting?.Invoke(_pause);
    }

    public void Replay()
    {
        EntityManager.Instance.DespawnAll();
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

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        Pause(true);
        SoundManager.Instance.GameOver();
        UIManager.Instance.OpenResult(true);
    }
    #endregion

    #region GET
    public int GetTotalScore() => totalScore;
    #endregion
}
