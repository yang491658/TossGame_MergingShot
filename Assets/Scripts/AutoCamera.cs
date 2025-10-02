using UnityEngine;

[RequireComponent(typeof(Camera))]
public class AutoCamera : MonoBehaviour
{
    [SerializeField] private Vector2 res = new Vector2(1080, 1920);
    [SerializeField] private float baseSize = 12f;
    [SerializeField] private float minSize = 12f;

    private Camera cam;
    private int lw, lh;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        Apply(true);
    }

    private void Update()
    {
        if (Screen.width != lw || Screen.height != lh) Apply(false);
    }

    private void Apply(bool force)
    {
        lw = Screen.width; lh = Screen.height;

        float currentAspect = (float)lw / lh;
        float refAspect = res.x / res.y;

        float size = baseSize * (refAspect / currentAspect);
        size = Mathf.Max(minSize, size);

        cam.orthographicSize = size;

        EntityManager.Instance?.SetEntity();
    }
}
