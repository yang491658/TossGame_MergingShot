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
    [SerializeField] private int testCount = 1;
    [SerializeField] private bool isAutoPlay = false;
    [SerializeField] private bool isAutoReplay = false;
    [SerializeField][Min(1f)] private float regameTime = 5f;
    private Coroutine playRoutine;

    [Header("ADs Test")]
    private bool onBanner = false;

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

        if (Input.GetKeyDown(KeyCode.A))
            isAutoReplay = !isAutoReplay;

        if (isAutoReplay && GameManager.Instance.IsGameOver && playRoutine == null)

            playRoutine = StartCoroutine(AutoReplay());
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
            UnitSystem unit = HandleManager.Instance.GetReady();
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

        #region 액트 테스트
        if (Input.GetKeyDown(KeyCode.T)) AutoPlay();
        #endregion

        #region 광고 테스트
        if (Input.GetKeyDown(KeyCode.C))
            ADManager.Instance?.ShowInterAD();
        if (Input.GetKeyDown(KeyCode.V))
            ADManager.Instance?.ShowReward();
        if (Input.GetKeyDown(KeyCode.B))
        {
            ADManager.Instance?.CreateBanner(!onBanner);
            onBanner = !onBanner;
        }
        #endregion
    }

    private void AutoPlay()
    {
        if (!isAutoPlay)
        {
            HandleManager.Instance.SetTimeLimit(0.01f);
            isAutoPlay = true;
        }
        else
        {
            HandleManager.Instance.SetTimeLimit(10f);
            isAutoPlay = false;
        }
    }

    private IEnumerator AutoReplay()
    {
        if (EntityManager.Instance.GetCount(EntityManager.Instance.GetFinal()) > 0)
            yield return null;

        yield return new WaitForSecondsRealtime(regameTime);
        if (GameManager.Instance.IsGameOver)
        {
            testCount++;
            GameManager.Instance.Replay();
        }
        playRoutine = null;
    }
}
#endif
