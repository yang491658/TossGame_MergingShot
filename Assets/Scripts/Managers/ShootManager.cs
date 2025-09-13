using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShootManager : MonoBehaviour
{
    private Camera cam => Camera.main;
    private LineRenderer aimLine => GetComponent<LineRenderer>();

    private LayerMask unitLayer => LayerMask.GetMask("Unit");
    [Space]
    [SerializeField] private float powerCoef = 5f;
    [SerializeField] private float maxPower = 3f;

    private UnitSystem selected;
    private Vector2 dragStart;

    private void Awake()
    {
        if (aimLine != null)
        {
            aimLine.positionCount = 2;
            aimLine.enabled = false;
        }
    }

    private void Update()
    {
#if UNITY_EDITOR
        HandleMouse();
#else
        HandleTouch();
#endif
    }

    #region 입력
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
        if (_unit == null) return false;

        var rb = _unit.GetComponent<Rigidbody2D>();
        return rb != null && rb.linearVelocity.sqrMagnitude <= 0.01f;
    }
    #endregion

    #region 드래그
    private void BeginDrag(Vector2 _screenPos, int _fingerId = -1)
    {
        if (PointerOverUI(_fingerId)) return;

        Vector2 world = ScreenToWorld(_screenPos);
        RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero, 0.01f, unitLayer);

        if (hit.collider != null && hit.collider.TryGetComponent(out UnitSystem _unit) && CanSelect(_unit))
        {
            if (aimLine != null) aimLine.enabled = true;

            selected = _unit;
            selected.SetSelected(true);

            dragStart = world;
        }
    }

    private void DoDrag(Vector2 _screenPos)
    {
        if (selected == null) return;

        UpdateAimLine();
    }

    private void EndDrag(Vector2 _screenPos)
    {
        if (aimLine != null) aimLine.enabled = false;

        Vector2 endWorld = ScreenToWorld(_screenPos);
        Vector2 drag = endWorld - dragStart;
        Vector2 shotDir = -drag;

        float dist = Mathf.Min(shotDir.magnitude, maxPower);

        if (selected != null)
        {
            Vector2 impulse = shotDir.sqrMagnitude > 0f ? shotDir.normalized * dist * powerCoef : Vector2.zero;

            selected.Shoot(impulse);
            selected.SetSelected(false);

            StartCoroutine(Respawn());

            selected = null;
        }
    }
    #endregion

    #region 조준
    private void UpdateAimLine()
    {
        if (aimLine == null || selected == null) return;

        Vector3 start;
        var rb = selected.GetComponent<Rigidbody2D>();
        if (rb != null) start = rb.worldCenterOfMass;
        else start = selected.transform.position;

#if UNITY_EDITOR
        Vector3 cur = ScreenToWorld(Input.mousePosition);
#else
        Vector3 cur = ScreenToWorld(Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : (Vector3)Input.mousePosition);
#endif

        Vector3 dir = (start - cur);
        float dist = Mathf.Min(dir.magnitude, maxPower);
        Vector3 end = start + dir.normalized * dist;

        aimLine.SetPosition(0, start);
        aimLine.SetPosition(1, end);
    }
    #endregion

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(1f);
        GameManager.Instance.Spawn();
    }
}
