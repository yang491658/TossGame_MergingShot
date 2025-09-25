using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ShotManager : MonoBehaviour
{
    public static ShotManager Instance { get; private set; }

    private Camera cam => Camera.main;

    private LayerMask unitLayer => LayerMask.GetMask("Unit");
    private bool isDragging;

    [Header("Aim Dots")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private int dotCount = 12;
    [SerializeField] private float dotSpacing = 0.5f;
    private readonly List<Transform> dots = new List<Transform>();

    [Header("Aim Ring & Line")]
    [SerializeField] private LineRenderer line;
    [SerializeField] private LineRenderer ring;
    [SerializeField] private int ringSegments = 64;
    [SerializeField] private float ringRadius = 0.5f;

    [Header("Shot Info.")]
    [SerializeField] private float maxPower = 5f;
    [SerializeField] private float powerCoef = 3f;

    private UnitSystem selectedUnit;
    private Vector2 dragStart;

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

        if (dots.Count == 0)
        {
            for (int i = 0; i < dotCount; i++)
            {
                var dot = Instantiate(dotPrefab, transform);
                dot.SetActive(false);
                dots.Add(dot.transform);
            }
        }

        if (line != null) line.enabled = false;
        if (ring != null) ring.enabled = false;
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
        if (Input.GetMouseButtonDown(0)) BeginDrag(Input.mousePosition);
        else if (Input.GetMouseButton(0)) DoDrag(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0)) EndDrag(Input.mousePosition);

        if (Input.GetMouseButton(1)) Move(Input.mousePosition);
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

    private bool PointerOverUI(int fingerId = -1) => EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);

    private Vector2 ScreenToWorld(Vector2 _screenPos) => cam.ScreenToWorldPoint(_screenPos);

    private bool CanSelect(UnitSystem _unit)
    {
        var rb = _unit.GetComponent<Rigidbody2D>();
        return rb != null && rb.linearVelocity.sqrMagnitude <= 0.01f;
    }
    #endregion

    #region 드래그
    private void BeginDrag(Vector2 _pos, int _fingerId = -1)
    {
        if (PointerOverUI(_fingerId)) return;

        Vector2 world = ScreenToWorld(_pos);
        RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero, 0.01f, unitLayer);

        if (hit.collider != null && hit.collider.TryGetComponent(out UnitSystem _unit) && CanSelect(_unit))
        {
            selectedUnit = _unit;
            dragStart = world;
            isDragging = true;
            ShowAim(true);
        }
        else
        {
            selectedUnit = null;
            isDragging = false;
            ShowAim(false);
        }
    }

    private void DoDrag(Vector2 _pos)
    {
        if (!isDragging || selectedUnit == null) return;
        UpdateAim(ScreenToWorld(_pos));
    }

    private void EndDrag(Vector2 _pos)
    {
        if (!isDragging || selectedUnit == null)
        {
            isDragging = false;
            ShowAim(false);
            return;
        }

        Vector2 endWorld = ScreenToWorld(_pos);
        Vector2 drag = endWorld - dragStart;
        Vector2 shotDir = -drag;

        float dist = Mathf.Min(shotDir.magnitude, maxPower);

        Vector2 impulse = shotDir.normalized * dist * powerCoef;
        selectedUnit.Shoot(impulse);
        StartCoroutine(Respawn());

        isDragging = false;
        ShowAim(false);
        selectedUnit = null;
    }
    #endregion

    #region 조준
    private void ShowAim(bool _on)
    {
        for (int i = 0; i < dots.Count; i++)
            dots[i].gameObject.SetActive(_on);

        if (line != null)
        {
            line.enabled = _on;
            if (!_on) line.positionCount = 0;
        }

        if (ring != null)
        {
            ring.enabled = _on;
            if (!_on) ring.positionCount = 0;
        }
    }

    private void UpdateAim(Vector3 _pos)
    {
        if (!isDragging || selectedUnit == null) return;

        var rb = selectedUnit.GetComponent<Rigidbody2D>();
        Vector3 start = rb != null ? (Vector3)rb.worldCenterOfMass : selectedUnit.transform.position;

        Vector3 dirRaw = (start - _pos);
        float dist = Mathf.Min(dirRaw.magnitude, maxPower);
        Vector3 dir = dirRaw.normalized * dist;
        Vector3 ringCenter = start - dir;

        if (dist <= Mathf.Epsilon)
        {
            for (int i = 0; i < dots.Count; i++) dots[i].gameObject.SetActive(false);
            if (line != null) line.enabled = false;
            if (ring != null) ring.enabled = false;
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
            line.enabled = true;
            line.positionCount = 2;
            line.SetPosition(0, start);
            line.SetPosition(1, ringCenter + dir.normalized * ringRadius);
        }

        if (ring != null)
        {
            ring.enabled = true;
            ring.positionCount = ringSegments + 1;
            for (int i = 0; i <= ringSegments; i++)
            {
                float t = (float)i / ringSegments * Mathf.PI * 2f;
                Vector3 r = new Vector3(Mathf.Cos(t), Mathf.Sin(t), 0f) * ringRadius;
                ring.SetPosition(i, ringCenter + r);
            }
        }
    }
    #endregion

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(1f);
        SpawnManager.Instance.Spawn(Random.Range(1, 5));
    }

#if UNITY_EDITOR
    private void Move(Vector2 _pos)
    {
        Vector2 world = ScreenToWorld(_pos);
        RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero, 0.01f, unitLayer);

        UnitSystem target = null;
        if (hit.collider != null && hit.collider.TryGetComponent(out UnitSystem hitUnit))
            target = hitUnit;

        if (target == null) return;

        var rb = target.GetComponent<Rigidbody2D>();
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
#endif
}
