#if UNITY_EDITOR
using System.Collections;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    public static TestManager Instance { get; private set; }

    [Header("Spawn Test")]
    [SerializeField][Range(0f, 45f)] float angleRange = 30f;
    [SerializeField][Range(0f, 20f)] float shotPower = 15f;

    [Header("Game Test")]
    [SerializeField][Min(1f)] private float regameTime = 3f;
    private Coroutine playRoutine;

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
        if (!SoundManager.Instance.IsBGMMuted()) SoundManager.Instance.ToggleBGM();
        if (!SoundManager.Instance.IsSFXMuted()) SoundManager.Instance.ToggleSFX();
        ActManager.Instance.SetTimeLimit(0.01f);
    }

    private void Update()
    {
        #region 게임 테스트
        if (Input.GetKeyDown(KeyCode.P))
            GameManager.Instance.Pause(!GameManager.Instance.IsPaused);

        if (Input.GetKeyDown(KeyCode.R))
            GameManager.Instance.Replay();

        if (Input.GetKeyDown(KeyCode.Q))
            GameManager.Instance.Quit();

        if (Input.GetKeyDown(KeyCode.G))
            GameManager.Instance.GameOver();

        if (GameManager.Instance.IsGameOver && playRoutine == null)
            if (EntityManager.Instance.GetCount(EntityManager.Instance.GetFinal()) <= 0)
                playRoutine = StartCoroutine(TestPlay());
        #endregion

        #region 사운드 테스트
        if (Input.GetKeyDown(KeyCode.M))
            SoundManager.Instance.ToggleBGM();
        if (Input.GetKeyDown(KeyCode.N))
            SoundManager.Instance.ToggleSFX();
        #endregion

        #region 엔티티 테스트
        for (int i = 1; i <= 10; i++)
        {
            KeyCode key = (i == 10) ? KeyCode.Alpha0 : (KeyCode)((int)KeyCode.Alpha0 + i);
            if (Input.GetKeyDown(key))
            {
                UnitSystem unit = EntityManager.Instance.Spawn(i);
                unit.Shoot(Vector2.up * shotPower);
                break;
            }
        }

        if (Input.GetKey(KeyCode.W))
        {
            UnitSystem unit = ActManager.Instance.GetReady();
            float angle = Random.Range(-angleRange, angleRange);
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
            if (unit != null)
            {
                unit.Shoot(dir * shotPower);
                EntityManager.Instance.Respawn();
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
            EntityManager.Instance.Spawn();

        if (Input.GetKeyDown(KeyCode.D))
        {
            GameManager.Instance.ResetScore();
            EntityManager.Instance.ResetCount();
            EntityManager.Instance.DespawnAll();
        }
        #endregion
    }

    private IEnumerator TestPlay()
    {
        yield return new WaitForSecondsRealtime(regameTime);
        GameManager.Instance.Replay();
        playRoutine = null;
    }
}
#endif
