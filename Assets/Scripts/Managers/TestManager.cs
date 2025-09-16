#if UNITY_EDITOR
using UnityEngine;

public class TestManager : MonoBehaviour
{
    private float time = 0;
    private float delay = 0.1f;

    private void Update()
    {
        if (Input.GetKey(KeyCode.W) && Time.time >= time)
        {
            UnitSystem unit = GameManager.Instance.Spawn(1);
            if (unit != null)
            {
                Vector2 impulse = Vector2.up * 100f;
                unit.Shoot(impulse);
            }
            time = Time.time + delay;
        }

        if (Input.GetKeyDown(KeyCode.A))
            GameManager.Instance.Spawn(1);

        if (Input.GetKeyDown(KeyCode.D))
        {
            GameManager.Instance.DestroyAll();
        }
    }
}
#endif
