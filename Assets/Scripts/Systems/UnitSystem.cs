using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D), typeof(Rigidbody2D))]
public class UnitSystem : MonoBehaviour
{
    private SpriteRenderer sr;
    private Collider2D col;
    private Rigidbody2D rb;

    [SerializeField] private UnitData data;

    public bool fired { get; private set; }
    public bool merging { get; private set; }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        col.isTrigger = true;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (col == null) col = GetComponent<Collider2D>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }
#endif

    public void Shoot(Vector2 _impulse)
    {
        Debug.Log(gameObject.name + " น฿ป็ : " + _impulse.magnitude);

        rb.AddForce(_impulse, ForceMode2D.Impulse);

        fired = true;
        col.isTrigger = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.gameObject.GetComponent<UnitSystem>();

        if (other == null) return;
        if (!collision.gameObject.CompareTag(gameObject.tag)) return;
        if (other.GetID() != GetID()) return;
        if (other.merging || merging) return;
        if (other.GetInstanceID() < GetInstanceID()) return;

        merging = true;
        other.merging = true;

        var otherRb = collision.rigidbody;

        Vector2 pA = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 pB = otherRb != null ? otherRb.position : (Vector2)other.transform.position;
        Vector2 pM = (pA + pB) / 2f;

        Vector2 vA = GetVelocity();
        Vector2 vB = other.GetVelocity();
        Vector2 vM = (vA + vB) / 2f;

        SpawnManager.Instance.DestroyUnit(other);
        SpawnManager.Instance.DestroyUnit(this);

        if (!IsFinal())
        {
            UnitSystem us = SpawnManager.Instance.Spawn(GetID() + 1, pM);
            us.Shoot(vM);
        }

        GameManager.Instance.AddScore(GetScore());
    }

    #region SET
    public void SetData(UnitData _data)
    {
        data = _data;
        if (data == null) return;

        gameObject.name = data.unitName;

        if (sr != null && data.unitImage != null)
            sr.sprite = data.unitImage;

        if (rb != null) rb.mass = 1;
        transform.localScale = Vector3.one * data.unitScale;
    }
    #endregion

    #region GET
    public int GetID() => data.unitID;
    public bool IsFinal() => GetID() == SpawnManager.Instance.GetFinal();

    public int GetScore() => data.unitScore;

    public Vector2 GetVelocity() => rb != null ? rb.linearVelocity : Vector2.zero;
    #endregion
}
