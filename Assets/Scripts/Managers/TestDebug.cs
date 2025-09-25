#if UNITY_EDITOR
using UnityEngine;

public class TestDebug : MonoBehaviour
{
    private float time = 0;

    [SerializeField][Range(0f, 3f)] private float delay = 0.1f;

    [SerializeField][Range(0f, 45f)] float angleRange = 0f;

    private void Start()
    {
        SoundManager.Instance.ToggleBGM();
        SoundManager.Instance.ToggleSFX();
    }

    private void Update()
    {
        #region 게임 테스트
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (GameManager.Instance.IsPaused) GameManager.Instance.Pause(false);
            else GameManager.Instance.Pause(true);
        }

        if (Input.GetKeyDown(KeyCode.R))
            GameManager.Instance.Replay();

        if (Input.GetKeyDown(KeyCode.Q))
            GameManager.Instance.Quit();
        #endregion

        #region 사운드 테스트
        if (Input.GetKeyDown(KeyCode.M))
        {
            SoundManager.Instance.ToggleBGM();
            SoundManager.Instance.ToggleSFX();
        }
        #endregion

        #region 소환 테스트
        if (Input.GetKey(KeyCode.W) && Time.time >= time)
        {
            UnitSystem unit = SpawnManager.Instance.Spawn(1);

            float angle = Random.Range(-angleRange, angleRange);
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
            unit.Shoot(dir * 15f);
            time = Time.time + delay;
        }

        for (int i = 1; i <= 10; i++)
        {
            KeyCode key = (i == 10) ? KeyCode.Alpha0 : (KeyCode)((int)KeyCode.Alpha0 + i);
            if (Input.GetKeyDown(key))
            {
                UnitSystem unit = SpawnManager.Instance.Spawn(i, (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition));
                unit.Shoot(Vector2.zero);
                break;
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
            SpawnManager.Instance.DestroyAll();
        #endregion
    }
}
#endif
