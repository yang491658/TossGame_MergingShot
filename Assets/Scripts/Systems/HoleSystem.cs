using UnityEngine;
using System.Collections.Generic;

public class HoleSystem : MonoBehaviour
{
    [Header("Gravity")]
    [SerializeField] private float gravity = 300f;

    [Header("Stabilize / Sleep")]
    [SerializeField] private float sleepRadius = 0.6f;
    [SerializeField] private float sleepSpeed = 0.05f;
    [SerializeField] private int sleepFrame = 8;

    [Header("Stabilize / Damping")]
    [SerializeField] private float dampRadius = 1.2f;
    [SerializeField] private float radDamping = 8f;
    [SerializeField] private float tanDamping = 2f;
    [SerializeField] private float maxForce = 30f;

    [Header("Stabilize / BounceKill")]
    [SerializeField] private float bounceKillRadius = 0.55f;
    [SerializeField] private float bounceKillSpeed = 0.06f;

    [Header("Rotation Info.")]
    [SerializeField] private float rotateSpeed = 30f;
    private HingeJoint2D hinge;

    private readonly Dictionary<Rigidbody2D, int> sleepCount = new Dictionary<Rigidbody2D, int>(64);

    private void Awake()
    {
        hinge = GetComponentInChildren<HingeJoint2D>();
        hinge.useMotor = true;
    }

    private void Update()
    {
        SetMotor();
    }

    private void FixedUpdate()
    {
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        var units = EntityManager.Instance.GetUnits();
        if (units == null || units.Count == 0) return;

        Vector2 center = transform.position;

        for (int i = 0; i < units.Count; i++)
        {
            var unit = units[i];
            if (unit == null || !unit.isFired) continue;

            var rb = unit.GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            Vector2 p = rb.worldCenterOfMass;
            Vector2 d = center - p;
            float dist = d.magnitude;

            bool canSleep = dist < sleepRadius && rb.linearVelocity.magnitude < sleepSpeed;
            int c = 0;
            sleepCount.TryGetValue(rb, out c);
            c = canSleep ? c + 1 : 0;
            sleepCount[rb] = c;

            if (c >= sleepFrame)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.Sleep();
                continue;
            }

            Vector2 n = dist > 1e-4f ? d / dist : Vector2.zero;
            float safeDist = Mathf.Max(dist, sleepRadius);
            float baseAcc = gravity / (safeDist * safeDist);
            Vector2 gravityForce = n * (baseAcc * rb.mass);

            float t = 0f;
            if (dist <= dampRadius)
                t = 1f - Mathf.Clamp01((dist - sleepRadius) / Mathf.Max(1e-4f, dampRadius - sleepRadius));

            Vector2 v = rb.linearVelocity;
            float vRad = Vector2.Dot(v, n);
            Vector2 vTan = v - n * vRad;

            if (dist < bounceKillRadius && vRad > 0f && vRad < bounceKillSpeed)
                rb.linearVelocity = v - n * vRad;

            Vector2 radialDamp = -n * vRad * (radDamping * t) * rb.mass;
            Vector2 tangentialDamp = -vTan * (tanDamping * t) * rb.mass;

            Vector2 total = gravityForce + radialDamp + tangentialDamp;
            float mag = total.magnitude;
            if (mag > maxForce) total *= (maxForce / mag);

            rb.AddForce(total, ForceMode2D.Force);

            if (rb.linearVelocity.sqrMagnitude < 1e-4f) rb.linearVelocity = Vector2.zero;
            if (Mathf.Abs(rb.angularVelocity) < 1e-2f) rb.angularVelocity = 0f;
        }
    }

    private void SetMotor()
    {
        if (hinge == null) return;
        var motor = hinge.motor;
        motor.motorSpeed = rotateSpeed;
        hinge.motor = motor;
    }
}
