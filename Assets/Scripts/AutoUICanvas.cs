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
        Apply(false);
    }

    private void Init()
    {
        if (scaler == null) scaler = GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = res;
        }
    }

    private void Apply(bool _force)
    {
        if (scaler == null) Init();
        if (scaler == null) return;

        float w = Screen.width;
        float h = Screen.height;
        if (!_force && Mathf.Approximately(w, lastW) && Mathf.Approximately(h, lastH)) return;
        lastW = w; lastH = h;

        float current = w / h;
        float reference = res.x / res.y;
        scaler.matchWidthOrHeight = current < reference ? 0f : 1f;
    }
}
