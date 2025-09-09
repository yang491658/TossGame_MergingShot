using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HoleSystem : MonoBehaviour
{
    private Collider2D col;

    private LayerMask unitLayer => LayerMask.GetMask("Unit");
    [Space]
    [SerializeField] private float pullPower = 15f;
    [SerializeField] private float powerLimit = 50f;
    [SerializeField]
    private AnimationCurve falloff =
        new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

    private readonly HashSet<Rigidbody2D> targets = new HashSet<Rigidbody2D>();

    private void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if ((unitLayer.value & (1 << other.gameObject.layer)) == 0) return;
        if (other.attachedRigidbody != null)
            targets.Add(other.attachedRigidbody);
    }

    private void FixedUpdate()
    {
        if (targets.Count == 0) return;

        Vector2 center = col.bounds.center;
        float radius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.y);

        List<Rigidbody2D> toRemove = null;

        foreach (var rb in targets)
        {
            if (rb == null)
            {
                if (toRemove == null) toRemove = new List<Rigidbody2D>();
                toRemove.Add(rb);
                continue;
            }

            var unit = rb.GetComponent<UnitSystem>();
            if (unit == null || !unit.fired) continue;

            Vector2 dir = center - rb.position;
            float dist = dir.magnitude;
            if (dist > powerLimit) continue;

            float t = Mathf.Clamp01(dist / radius);
            float force = pullPower * falloff.Evaluate(1f - t);
            rb.AddForce(dir.normalized * force, ForceMode2D.Force);
        }

        if (toRemove != null)
        {
            for (int i = 0; i < toRemove.Count; i++)
                targets.Remove(toRemove[i]);
        }
    }
}
