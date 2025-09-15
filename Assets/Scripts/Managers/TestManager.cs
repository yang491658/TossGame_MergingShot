using UnityEngine;

public class TestManager : MonoBehaviour
{
    private float delay = 0.3f;
    private float nextTime = 0;

    private void Update()
    {
        if (Input.GetKey(KeyCode.W) && Time.time >= nextTime)
        {
            GameObject go = GameManager.Instance.Spawn(Random.Range(1, 4));
            go.GetComponent<UnitSystem>().Shoot(Vector2.up * 20f);
            nextTime = Time.time + delay;
        }

        if (Input.GetKeyDown(KeyCode.D))
            GameManager.Instance.DestroyAll();
    }
}
