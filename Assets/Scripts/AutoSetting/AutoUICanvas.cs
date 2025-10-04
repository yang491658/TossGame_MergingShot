using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class AutoUICanvas : MonoBehaviour
{
    [SerializeField] private Vector2 res = new Vector2(1080, 1920);
    private CanvasScaler scaler;
    private float lastW, lastH;

    private void Awake()
    {
        Init();
        Apply(true);
    }

    private void OnRectTransformDimensionsChange()
    {
        if (scaler == null) Init();
        Apply(false);
    }

    private void Init()
    {
        scaler = GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.referenceResolution = res;
    }

    private void Apply(bool _force)
    {
        if (scaler == null) return;

        float w = Screen.width;
        float h = Screen.height;
        if (h == 0) return;
        if (!_force && Mathf.Approximately(w, lastW) && Mathf.Approximately(h, lastH)) return;
        lastW = w; lastH = h;

        float current = w / h;
        float reference = res.x / res.y;
        scaler.matchWidthOrHeight = current < reference ? 0f : 1f;
    }
}
