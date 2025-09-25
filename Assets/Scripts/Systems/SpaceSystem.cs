using UnityEngine;

[ExecuteAlways]
public class SpaceSystem : MonoBehaviour
{
    private Camera cam;
    private SpriteRenderer sr;

    private int lastW, lastH;

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
        if (Screen.width != lastW || Screen.height != lastH)
            Fit();
    }

    private void Fit()
    {
        if (cam == null || sr == null || sr.sprite == null) return;

        lastW = Screen.width; lastH = Screen.height;

        float worldH = cam.orthographicSize * 2f;
        float worldW = worldH * cam.aspect;

        var sp = sr.sprite;
        float spriteW = sp.rect.width / sp.pixelsPerUnit;
        float spriteH = sp.rect.height / sp.pixelsPerUnit;

        float scale = Mathf.Max(worldW / spriteW, worldH / spriteH);

        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
