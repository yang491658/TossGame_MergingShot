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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Merge(collision);
    }


    public void Shoot(Vector2 _impulse, bool _isShot = true)
    {
        rb.AddForce(_impulse, ForceMode2D.Impulse);

        fired = true;
        col.isTrigger = false;

        if (_isShot) SoundManager.Instance.Shoot();
    }

    private void Merge(Collision2D _collision)
    {
        var other = _collision.gameObject.GetComponent<UnitSystem>();

        if (other == null) return;
        if (!other.CompareTag(tag)) return;
        if (other.GetID() != GetID()) return;
        if (other.merging || merging) return;
        if (other.GetInstanceID() < GetInstanceID()) return;

        merging = true;
        other.merging = true;

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

        rb.mass = 1f;
        transform.localScale = Vector3.one * data.unitScale;
    }
    #endregion

    #region GET
    public int GetID() => data.unitID;
    public int GetScore() => data.unitScore;
    public Vector2 GetVelocity() => rb.linearVelocity;
    #endregion
}
