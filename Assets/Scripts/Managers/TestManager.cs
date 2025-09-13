using UnityEngine;

public class TestManager : MonoBehaviour
{
    private float delay = 0.3f;
    private float nextTime = 0;

    private void Update()
    {
        if (Input.GetKey(KeyCode.W) && Time.time >= nextTime)
        {
            GameObject go = GameManager.Instance.Spawn(1);
            go.GetComponent<UnitSystem>().Shoot(Vector2.up * 20);
            nextTime = Time.time + delay;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            for (int i = 1; i < 11; i++)
            {
                GameObject go = GameManager.Instance.Spawn(i, new Vector2(-5 + i, -5));
                go.GetComponent<UnitSystem>().Shoot(Vector2.zero);
            }
        }

        if (Input.GetKeyDown(KeyCode.T))
            GameManager.Instance.Spawn();

        if (Input.GetKeyDown(KeyCode.D))
            GameManager.Instance.DestroyAll();
    }
}
