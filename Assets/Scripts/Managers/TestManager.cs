#if UNITY_EDITOR
using UnityEngine;

public class TestManager : MonoBehaviour
{
    private float time = 0;

    [SerializeField][Range(0f, 3f)] private float delay = 0.3f;

    [SerializeField][Range(0f, 45f)] float angleRange;

    private void Update()
    {
        #region 게임 테스트
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (GameManager.Instance.IsPaused) GameManager.Instance.Resume();
            else GameManager.Instance.Pause();
        }

        if (Input.GetKeyDown(KeyCode.R))
            GameManager.Instance.Replay();

        if (Input.GetKeyDown(KeyCode.Q))
            GameManager.Instance.Quit();
        #endregion

        #region 사운드 테스트
        if (Input.GetKeyDown(KeyCode.M))
            AudioManager.Instance.ToggleBGM();
        #endregion

        #region 소환 테스트
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

        if (Input.GetKey(KeyCode.W) && Time.time >= time)
        {
            UnitSystem unit = SpawnManager.Instance.Spawn();

            float angle = Random.Range(-angleRange, angleRange);
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
            unit.Shoot(dir * 25f);
            time = Time.time + delay;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int i = 1; i <= 10; i++)
            {
                UnitSystem unit = SpawnManager.Instance.Spawn(i);
                if (unit != null) unit.Shoot(Vector2.up * 100f);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
            SpawnManager.Instance.Spawn(1);

        if (Input.GetKeyDown(KeyCode.D))
            SpawnManager.Instance.DestroyAll();
        #endregion
    }
}
#endif
