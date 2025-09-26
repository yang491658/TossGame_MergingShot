using UnityEngine;

public class HoleSystem : MonoBehaviour
{
    [Header("Gravity Info.")]
    [SerializeField] private float gravity = 300f;

    [Header("Rotation Info.")]
    [SerializeField] private float rotateSpeed;
    private HingeJoint2D hinge;

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

            float safeSqr = Mathf.Max(dir.sqrMagnitude, 0.05f * 0.05f);
            float invSqr = 1f / safeSqr;
            float invDist = 1f / Mathf.Sqrt(safeSqr);

            Vector2 forceVec = dir * invDist * (gravity * invSqr * rb.mass);
            rb.AddForce(forceVec, ForceMode2D.Force);
        }
    }

    #region SET
    private void SetMotor()
    {
        var motor = hinge.motor;
        motor.motorSpeed = rotateSpeed;
        hinge.motor = motor;
    }
    #endregion
}
