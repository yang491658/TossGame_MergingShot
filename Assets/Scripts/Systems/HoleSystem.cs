using System.Collections.Generic;
using UnityEngine;

public class HoleSystem : MonoBehaviour
{
    private float gravity = 100f;

    private readonly HashSet<UnitSystem> units = new HashSet<UnitSystem>();

    public void Register(UnitSystem _unit)
    {
        if (_unit != null) units.Add(_unit);
    }

    private void FixedUpdate()
    {
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

        units.RemoveWhere(x => x == null);
    }
}
