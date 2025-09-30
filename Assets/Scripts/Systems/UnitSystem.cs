using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D), typeof(Rigidbody2D))]
public class UnitSystem : MonoBehaviour
{
    private SpriteRenderer sr;
    private Collider2D col;
    private Rigidbody2D rb;

    [SerializeField] private UnitData data;

    public bool isFired { get; private set; } = false;
    public bool isMerging { get; private set; } = false;
    public bool inHole { get; private set; } = false;

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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isFired) return;
        if (collision.CompareTag("Hole")) inHole = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isFired || isMerging || !inHole) return;
        //if (collision.CompareTag("Hole")) GameManager.Instance.GameOver();
    }

    public void Shoot(Vector2 _impulse, bool _isShot = true)
    {
        rb.AddForce(_impulse * rb.mass, ForceMode2D.Impulse);

        isFired = true;
        col.isTrigger = false;

        if (_isShot) SoundManager.Instance.Shoot();

        EntityManager.Instance.AddCount(data);
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

        EntityManager.Instance.DestroyUnit(other);
        EntityManager.Instance.DestroyUnit(this);

        int id = GetID();
        int score = GetScore();

        if (id != EntityManager.Instance.GetFinal())
        {
            UnitSystem us = EntityManager.Instance.Spawn(id + 1, pM);
            us.Shoot(vM, false);
        }

        GameManager.Instance.AddScore(score);
        SoundManager.Instance.Merge(id);
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
