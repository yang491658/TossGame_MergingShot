#if UNITY_EDITOR
using UnityEngine;

public class TestManager : MonoBehaviour
{
    private float time = 0;
    private float delay = 0.3f;

    private void Update()
    {
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
            UnitSystem unit = SpawnManager.Instance.Spawn(1);
            float angle = Random.Range(-45f, 45f);
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
            unit.Shoot(dir * 15f);
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
    }
}
#endif
