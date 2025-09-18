using UnityEngine;

public class HoleSystem : MonoBehaviour
{
    [SerializeField] private float gravity = 100f;

    private void FixedUpdate()
    {
        var units = SpawnManager.Instance.GetUnits();

        if (units.Count == 0) return;

        Vector2 center = transform.position;

        foreach (var unit in units)
        {
            if (unit == null || !unit.fired) continue;

            var rb = unit.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            Vector2 from = rb.worldCenterOfMass;
            Vector2 dir = center - from;
            float dist = dir.magnitude;

            float safeDist = Mathf.Max(dist, 0.001f);
            float invSq = 1f / (safeDist * safeDist);

            float force = gravity * invSq * rb.mass;
            rb.AddForce(dir.normalized * force, ForceMode2D.Force);
        }
    }
}
