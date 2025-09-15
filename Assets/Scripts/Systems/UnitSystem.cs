using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D), typeof(Rigidbody2D))]
public class UnitSystem : MonoBehaviour
{
    private SpriteRenderer sr;
    private Rigidbody2D rb;
    private Color originalColor;

    [SerializeField] private UnitData data;

    private float mass;
    public bool fired { get; private set; }
    private bool merging;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        originalColor = sr.color;
    }

    private void Update()
    {
        SetMass();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        SetMass();
    }
#endif

    public void Shoot(Vector2 _impulse)
    {
        rb.linearVelocity = _impulse;
        fired = true;
        SetSelected(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var otherUS = collision.gameObject.GetComponent<UnitSystem>();
        if (otherUS == null) return;
        if (!collision.gameObject.CompareTag(gameObject.tag)) return;
        if (IsFinal() || otherUS.GetID() != GetID()) return;
        if (merging || otherUS.merging) return;
        if (GetInstanceID() > otherUS.GetInstanceID()) return;

        merging = true;
        otherUS.merging = true;

        Vector2 pA = rb != null ? rb.position : (Vector2)transform.position;

        var otherRb = collision.rigidbody;
        Vector2 pB = otherRb != null ? otherRb.position : (Vector2)otherUS.transform.position;

        Vector2 mid = (pA + pB) * 0.5f;

        Destroy(otherUS.gameObject);
        Destroy(gameObject);
        GameObject go = GameManager.Instance.Spawn(GetID() + 1, mid);
        var us = go.GetComponent<UnitSystem>();
        us.fired = true;
    }

    #region GET
    public int GetID() => data.unitID;
    public bool IsFinal() => GetID() == GameManager.Instance.GetFinal();
    #endregion

    #region SET
    public void SetData(UnitData _data)
    {
        data = _data;
        if (data == null) return;

        gameObject.name = data.unitName;

        if (sr != null && data.unitImage != null)
            sr.sprite = data.unitImage;

        mass = data.unitMass;
        SetMass();
    }

    private void SetMass()
    {
        if (rb != null) rb.mass = mass;
        transform.localScale = Vector3.one * mass;
    }

    public void SetSelected(bool _selected)
    {
        if (sr == null) return;
        sr.color = _selected ? Color.red : originalColor;
    }
    #endregion
}
