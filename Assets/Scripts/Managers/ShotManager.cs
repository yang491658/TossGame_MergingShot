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

    [Header("Aim Info.")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField] private int dotCount = 10;
    [SerializeField] private float dotSpacing = 0.5f;
    private readonly List<Transform> dots = new List<Transform>();

    [Header("Shot Info.")]
    [SerializeField] private float powerCoef = 3;
    [SerializeField] private float maxPower = 5;

    private UnitSystem selected;
    private Vector2 dragStart;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (dotPrefab == null)
            dotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Dot.prefab");
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
    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0)) BeginDrag(Input.mousePosition);
        else if (Input.GetMouseButton(0)) DoDrag(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0)) EndDrag(Input.mousePosition);
    }

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
            selected = _unit;
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
            ShowAim(false);
            isDragging = false;
            return;
        }

        Vector2 endWorld = ScreenToWorld(_pos);
        Vector2 drag = endWorld - dragStart;
        Vector2 shotDir = -drag;

        float dist = Mathf.Min(shotDir.magnitude, maxPower);

        Vector2 impulse = shotDir.sqrMagnitude > 0f ? shotDir.normalized * dist * powerCoef : Vector2.zero;
        selected.Shoot(impulse);
        StartCoroutine(Respawn());

        ShowAim(false);
        isDragging = false;
        selected = null;
    }
    #endregion

    #region 조준
    private void ShowAim(bool _on)
    {
        for (int i = 0; i < dots.Count; i++) dots[i].gameObject.SetActive(_on);
    }

    private void UpdateAim(Vector3 _pos)
    {
        if (selected == null) return;

        var rb = selected != null ? selected.GetComponent<Rigidbody2D>() : null;

        Vector3 start = rb != null ? (Vector3)rb.worldCenterOfMass : selected.transform.position;
        Vector3 dir = (start - _pos);

        float dist = Mathf.Min(dir.magnitude, maxPower);

        if (dist <= Mathf.Epsilon)
        {
            for (int i = 0; i < dots.Count; i++) dots[i].gameObject.SetActive(false);
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
    }
    #endregion

    private IEnumerator Respawn(int _id = 0)
    {
        yield return new WaitForSeconds(1f);
        SpawnManager.Instance.Spawn(Random.Range(1, 5));
    }
}
