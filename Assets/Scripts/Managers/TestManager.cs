using UnityEngine;

public class TestManager : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameObject go = GameManager.Instance.SpawnById(1);
            go.GetComponent<UnitSystem>().Shoot(Vector2.up * 5);
        }

        if (Input.GetKeyDown(KeyCode.T))
            GameManager.Instance.SpawnRandom();

        if (Input.GetKeyDown(KeyCode.D))
            GameManager.Instance.DestroyAll();
    }
}
