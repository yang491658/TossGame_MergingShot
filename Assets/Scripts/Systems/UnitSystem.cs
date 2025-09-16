using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D), typeof(Rigidbody2D))]
public class UnitSystem : MonoBehaviour
{
    private SpriteRenderer sr;
    private Collider2D col;
    private Rigidbody2D rb;
    private Color originalColor;

    [SerializeField] private UnitData data;

    private float mass;
    public bool fired { get; private set; }
    private bool merging;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        originalColor = sr.color;
        col.isTrigger = true;
    }

    private void Update()
    {
        SetMass();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (col == null) col = GetComponent<Collider2D>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        SetMass();
    }
#endif

    public void Shoot(Vector2 _impulse)
    {
        rb.linearVelocity = _impulse;
        fired = true;
        col.isTrigger = false;

        SetSelected(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.gameObject.GetComponent<UnitSystem>();

        if (other == null) return;
        if (!collision.gameObject.CompareTag(gameObject.tag)) return;
        if (IsFinal() || other.GetID() != GetID()) return;
        if (merging || other.merging) return;
        if (GetInstanceID() > other.GetInstanceID()) return;

        merging = true;
        other.merging = true;

        var otherRb = collision.rigidbody;

        Vector2 pA = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 pB = otherRb != null ? otherRb.position : (Vector2)other.transform.position;
        Vector2 pM = (pA + pB) / 2f;

        GameManager.Instance.DestroyUnit(other);
        GameManager.Instance.DestroyUnit(this);

        UnitSystem us = GameManager.Instance.Spawn(GetID() + 1, pM);

        Vector2 vA = GetVelocity();
        Vector2 vB = other.GetVelocity();
        Vector2 vM = (vA + vB) / 2f;

        us.Shoot(vM);
    }

    #region GET
    public int GetID() => data.unitID;
    public bool IsFinal() => GetID() == GameManager.Instance.GetFinal();

    public int GetScore() => data.unitScore;

    public Vector2 GetVelocity() => rb != null ? rb.linearVelocity : Vector2.zero;
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
