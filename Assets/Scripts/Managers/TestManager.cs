#if UNITY_EDITOR
using UnityEngine;

public class TestManager : MonoBehaviour
{
    private float time = 0;

    [Header("Spawn Test")]
    [SerializeField][Range(0.05f, 3f)] private float delay = 0.05f;
    [SerializeField][Range(0f, 45f)] float angleRange = 30f;
    [SerializeField][Range(0f, 15f)] float shotPower = 15f;
    private bool autoFire = false;

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

        #endregion

        #region 사운드 테스트
        if (Input.GetKeyDown(KeyCode.M))
        {
            SoundManager.Instance.ToggleBGM();
            SoundManager.Instance.ToggleSFX();
        }
        #endregion

        #region 엔티티 테스트
        if (Input.GetKeyDown(KeyCode.W))
        {
            autoFire = !autoFire;
            time = Time.time;
        }

        if (autoFire && !GameManager.Instance.IsGameOver && Time.time >= time)
        {
            UnitSystem unit = EntityManager.Instance.Spawn(1);
            float angle = Random.Range(-angleRange, angleRange);
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
            unit.Shoot(dir * shotPower);
            time = Time.time + delay;
        }

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

        if (Input.GetKeyDown(KeyCode.S))
            EntityManager.Instance.Spawn(1);

        if (Input.GetKeyDown(KeyCode.D))
            EntityManager.Instance.DestroyAll();
        #endregion
    }
}
#endif
