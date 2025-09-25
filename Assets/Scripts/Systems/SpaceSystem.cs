using UnityEngine;

[ExecuteAlways]
public class SpaceSystem : MonoBehaviour
{
    private Camera cam;
    private SpriteRenderer sr;

    private int lastW, lastH;
    private float lastAspect, lastOrthoSize;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (enabled) Fit();
    }
#endif

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        Fit();
    }

    private void OnEnable() => Fit();

    private void Update()
    {
        if (cam == null) cam = Camera.main;

        if (Screen.width != lastW ||
            Screen.height != lastH ||
            (cam != null && (!Mathf.Approximately(cam.aspect, lastAspect) ||
                             !Mathf.Approximately(cam.orthographicSize, lastOrthoSize))))
        {
            Fit();
        }
    }

    private void Fit()
    {
        if (cam == null || sr == null || sr.sprite == null) return;
        if (!cam.orthographic) return;

        lastW = Screen.width;
        lastH = Screen.height;
        lastAspect = cam.aspect;
        lastOrthoSize = cam.orthographicSize;

        float worldH = cam.orthographicSize * 2f;
        float worldW = worldH * cam.aspect;

        var sp = sr.sprite;
        float spriteW = sp.rect.width / sp.pixelsPerUnit;
        float spriteH = sp.rect.height / sp.pixelsPerUnit;

        float scaleWorld = Mathf.Max(worldW / spriteW, worldH / spriteH);

        var parent = transform.parent;
        Vector3 parentLossy = parent ? parent.lossyScale : Vector3.one;
        float localX = scaleWorld / (parentLossy.x == 0f ? 1f : parentLossy.x);
        float localY = scaleWorld / (parentLossy.y == 0f ? 1f : parentLossy.y);

        transform.localScale = new Vector3(localX, localY, transform.localScale.z);
    }
}
