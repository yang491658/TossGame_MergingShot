using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D), typeof(Rigidbody2D))]
public class UnitSystem : MonoBehaviour
{
    private SpriteRenderer sr;
    private Collider2D col;
    private Rigidbody2D rb;

    [SerializeField] private UnitData data;

    public bool isFired { get; private set; }
    public bool isMerging { get; private set; }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        col.isTrigger = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Merge(collision);
    }

    public void Shoot(Vector2 _impulse, bool _isShot = true)
    {
        rb.AddForce(_impulse, ForceMode2D.Impulse);

        isFired = true;
        col.isTrigger = false;

        if (_isShot) SoundManager.Instance.Shoot();

        SpawnManager.Instance.AddCount(data);
    }

    private void Merge(Collision2D _collision)
    {
        var other = _collision.gameObject.GetComponent<UnitSystem>();

        if (other == null) return;
        if (!other.CompareTag(tag)) return;
        if (other.GetID() != GetID()) return;
        if (other.isMerging || isMerging) return;
        if (other.GetInstanceID() < GetInstanceID()) return;

        isMerging = true;
        other.isMerging = true;

        var otherRb = _collision.rigidbody;

        Vector2 pA = rb.position;
        Vector2 pB = otherRb.position;
        Vector2 pM = (pA + pB) / 2f;

        Vector2 vA = GetVelocity();
        Vector2 vB = other.GetVelocity();
        Vector2 vM = (vA + vB) / 2f;

        SpawnManager.Instance.DestroyUnit(other);
        SpawnManager.Instance.DestroyUnit(this);

        if (GetID() != SpawnManager.Instance.GetFinal())
        {
            UnitSystem us = SpawnManager.Instance.Spawn(GetID() + 1, pM);
            us.Shoot(vM, false);
        }

        GameManager.Instance.AddScore(GetScore());
        SoundManager.Instance.Merge(GetID());
    }

    #region SET
    public void SetData(UnitData _data)
    {
        data = _data;

        gameObject.name = data.unitName;

        if (data.unitImage != null)
            sr.sprite = data.unitImage;

        rb.mass = data.unitMass;
        transform.localScale = Vector3.one * data.unitScale;
    }
    #endregion

    #region GET
    public int GetID() => data.unitID;
    public int GetScore() => data.unitScore;
    public Vector2 GetVelocity() => rb.linearVelocity;
    #endregion
}
