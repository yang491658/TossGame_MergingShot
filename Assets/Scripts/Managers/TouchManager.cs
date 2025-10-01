using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance { get; private set; }

    private Camera cam => Camera.main;
    private LayerMask unitLayer => LayerMask.GetMask("Unit");
    private bool isDragging;

    private UnitSystem hovered;
    private UnitSystem selected;
    private Vector2 dragStart;

    [Header("Aim Dots")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private int dotCount = 12;
    [SerializeField] private float dotSpacing = 0.5f;
    private readonly List<Transform> dots = new List<Transform>();

    [Header("Aim Line & Ring")]
    [SerializeField] private LineRenderer line;
    [SerializeField] private LineRenderer ring;
    [SerializeField] private int ringSegments = 64;
    [SerializeField] private float ringRadius = 0.5f;
    private Vector3[] ringUnit;

    [Header("Launch")]
    [SerializeField] private float maxPower = 5f;
    [SerializeField] private float powerCoef = 3f;


#if UNITY_EDITOR
    private void OnValidate()
    {
        if (dotPrefab == null)
            dotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Dot.prefab");

        if (line == null) line = GameObject.Find("Line").GetComponent<LineRenderer>();
        if (ring == null) ring = GameObject.Find("Ring").GetComponent<LineRenderer>();
    }
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (dots.Count == 0)
        {
            for (int i = 0; i < dotCount; i++)
            {
                var dot = Instantiate(dotPrefab, transform);
                dot.SetActive(false);
                dots.Add(dot.transform);
            }
        }

        if (line != null)
        {
            line.gameObject.SetActive(false);
            line.positionCount = 0;
        }

        if (ring != null)
        {
            ring.gameObject.SetActive(false);
            ring.positionCount = 0;
            ringUnit = new Vector3[ringSegments + 1];
            for (int i = 0; i <= ringSegments; i++)
            {
                float t = (float)i / ringSegments * Mathf.PI * 2f;
                ringUnit[i] = new Vector3(Mathf.Cos(t), Mathf.Sin(t), 0f);
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance.IsPaused) return;

#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    #region 클릭
#if UNITY_EDITOR
    private void HandleMouse()
    {
        Hover(Input.mousePosition);

        if (Input.GetMouseButtonDown(0)) BeginDrag(Input.mousePosition);
        else if (Input.GetMouseButton(0)) DoDrag(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0)) EndDrag(Input.mousePosition);

        if (Input.GetMouseButtonDown(1)) Remove(Input.mousePosition);

        if (Input.GetMouseButton(2)) Move(Input.mousePosition);
    }
#endif

    private void HandleTouch()
    {
        if (Input.touchCount == 0) return;
        Touch t = Input.GetTouch(0);

        if (t.phase == TouchPhase.Began)
            BeginDrag(t.position, t.fingerId);
        else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
            DoDrag(t.position);
        else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            EndDrag(t.position);
    }

    private bool PointerOverUI(int _fingerID = -1)
        => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(_fingerID);

    private Vector2 ScreenToWorld(Vector2 _screenPos) => cam.ScreenToWorldPoint(_screenPos);

    private bool CanSelect(UnitSystem _unit)
    {
        var rb = _unit.GetRB();
        return rb != null && rb.linearVelocity.sqrMagnitude <= 0.01f;
    }
    #endregion

    #region 드래그
    private void BeginDrag(Vector2 _pos, int _fingerID = -1)
    {
        if (PointerOverUI(_fingerID)) return;

        Vector2 world = ScreenToWorld(_pos);
        Collider2D col = Physics2D.OverlapPoint(world, unitLayer);

        if (col != null && col.TryGetComponent(out UnitSystem unit) && CanSelect(unit))
        {
            selected = unit;
            dragStart = world;
            isDragging = true;
            ShowAim(true);
        }
        else
        {
            selected = null;
            isDragging = false;
            ShowAim(false);
        }
    }

    private void DoDrag(Vector2 _pos)
    {
        if (!isDragging || selected == null) return;
        UpdateAim(ScreenToWorld(_pos));
    }

    private void EndDrag(Vector2 _pos)
    {
        if (!isDragging || selected == null)
        {
            isDragging = false;
            ShowAim(false);
            return;
        }

        Vector2 endWorld = ScreenToWorld(_pos);
        Vector2 drag = endWorld - dragStart;
        Vector2 shotDir = -drag;

        float dist = Mathf.Min(shotDir.magnitude, maxPower);

        if (dist > Mathf.Epsilon)
        {
            Vector2 impulse = shotDir.normalized * dist * powerCoef;
            selected.Shoot(impulse);
            EntityManager.Instance.Respawn();
        }

        isDragging = false;
        ShowAim(false);
        selected = null;
    }
    #endregion

    #region 조준
    private void ShowAim(bool _on)
    {
        for (int i = 0; i < dots.Count; i++)
            dots[i].gameObject.SetActive(_on);

        if (line != null)
        {
            line.gameObject.SetActive(_on);
            if (!_on) line.positionCount = 0;
        }

        if (ring != null)
        {
            ring.gameObject.SetActive(_on);
            if (!_on) ring.positionCount = 0;
        }
    }

    private void UpdateAim(Vector3 _pos)
    {
        if (!isDragging || selected == null) return;

        var rb = selected.GetRB();
        Vector3 start = rb != null ? (Vector3)rb.worldCenterOfMass : selected.transform.position;

        Vector3 dirRaw = (start - _pos);
        float dist = Mathf.Min(dirRaw.magnitude, maxPower);
        Vector3 dir = dirRaw.normalized * dist;
        Vector3 ringCenter = start - dir;

        if (dist <= Mathf.Epsilon)
        {
            for (int i = 0; i < dots.Count; i++) dots[i].gameObject.SetActive(false);
            if (line != null) { line.gameObject.SetActive(false); line.positionCount = 0; }
            if (ring != null) { ring.gameObject.SetActive(false); ring.positionCount = 0; }
            return;
        }

        Vector3 step = dir.normalized * dotSpacing;
        int visible = Mathf.Min(Mathf.FloorToInt(dist / dotSpacing), dots.Count);

        Vector3 p = start + step;
        for (int i = 0; i < dots.Count; i++)
        {
            bool on = i < visible;
            dots[i].gameObject.SetActive(on);
            if (on)
            {
                dots[i].position = p;
                p += step;
            }
        }

        if (line != null)
        {
            line.gameObject.SetActive(true);
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, ringCenter + dir.normalized * ringRadius);
        }

        if (ring != null)
        {
            ring.gameObject.SetActive(true);
            ring.positionCount = ringSegments + 1;
            for (int i = 0; i <= ringSegments; i++)
                ring.SetPosition(i, ringCenter + ringUnit[i] * ringRadius);
        }
    }
    #endregion

#if UNITY_EDITOR
    private void Hover(Vector2 _pos)
    {
        if (PointerOverUI()) return;

        Vector2 world = ScreenToWorld(_pos);
        Collider2D col = Physics2D.OverlapPoint(world, unitLayer);

        if (col != null && col.TryGetComponent(out UnitSystem unit))
        {
            if (unit == hovered) return;
            ClearHover();

            hovered = unit;
            var sr = hovered.GetSR();
            if (sr != null) sr.color = Color.red;
        }
        else
        {
            ClearHover();
        }
    }

    private void ClearHover()
    {
    if (hovered != null)
    {
        var sr = hovered.GetSR();
        if (sr != null) sr.color = Color.white;
    }
    hovered = null;
    }

    private void Move(Vector2 _pos)
    {
        if (PointerOverUI()) return;

        Vector2 world = ScreenToWorld(_pos);
        Collider2D col = Physics2D.OverlapPoint(world, unitLayer);

        UnitSystem target = null;
        if (col != null && col.TryGetComponent(out UnitSystem unit))
            target = unit;

        if (target == null) return;

        var rb = target.GetRB();
        if (rb != null)
        {
            rb.position = world;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            target.transform.position = world;
        }
    }

    private void Remove(Vector2 _pos)
    {
        if (PointerOverUI()) return;

        Vector2 world = ScreenToWorld(_pos);
        Collider2D col = Physics2D.OverlapPoint(world, unitLayer);

        if (col != null && col.TryGetComponent(out UnitSystem unit))
            EntityManager.Instance.Despawn(unit);
    }
#endif
}
