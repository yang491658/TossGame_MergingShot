#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;

static public class ManagerEditor
{
    static private bool IsPlaying() => !EditorApplication.isPlaying;

    #region √ ±‚»≠
    static private void ResetInspector(Component _comp)
    {
        Undo.RegisterCompleteObjectUndo(_comp, "Reset");
        Unsupported.SmartReset(_comp);
        EditorUtility.SetDirty(_comp);
        EditorSceneManager.MarkSceneDirty(_comp.gameObject.scene);
    }

    static private void ResetAll(Type _type)
    {
        var _objs = UnityEngine.Object.FindObjectsByType(
            _type, FindObjectsInactive.Include, FindObjectsSortMode.None
        );
        for (int i = 0; i < _objs.Length; i++)
        {
            var _comp = _objs[i] as Component;
            if (_comp != null) ResetInspector(_comp);
        }
    }

    [MenuItem("Tools/∏≈¥œ¿˙ √ ±‚»≠", true)]
    static private bool ResetManagers_Validate() => IsPlaying();
    [MenuItem("Tools/∏≈¥œ¿˙ √ ±‚»≠", false, 1)]
    static private void ResetManagers()
    {
        var _types = new Type[]
        {
            typeof(GameManager), typeof(SoundManager), typeof(EntityManager),
            typeof(HandleManager), typeof(UIManager), typeof(ADManager), typeof(TestManager),
        };
        for (int i = 0; i < _types.Length; i++) ResetAll(_types[i]);
    }
    #endregion

    #region ƒ—±‚/≤Ù±‚
    static private T FindSingle<T>() where T : Component
    {
        var c = UnityEngine.Object.FindFirstObjectByType<T>(FindObjectsInactive.Include);
        return c;
    }

    static private bool AnyActive<T>() where T : Component
    {
        var c = FindSingle<T>();
        if (!c) return false;
        var go = c.gameObject;
        return go && go.activeSelf;
    }

    static private void SetActive<T>(bool on, string onLabel, string offLabel) where T : Component
    {
        var c = FindSingle<T>();
        if (!c) return;
        var go = c.gameObject;
        if (!go) return;
        Undo.RegisterFullObjectHierarchyUndo(go, on ? onLabel : offLabel);
        go.SetActive(on);
        EditorUtility.SetDirty(go);
        EditorSceneManager.MarkSceneDirty(go.scene);
    }
    #endregion

    #region UI
    [MenuItem("Tools/UI ƒ—±‚", true)]
    static private bool UIOn_Validate() => IsPlaying() && !AnyActive<UIManager>();
    [MenuItem("Tools/UI ƒ—±‚", false, 101)]
    static private void UIOn() => SetActive<UIManager>(true, "UI ƒ—±‚", "UI ≤Ù±‚");

    [MenuItem("Tools/UI ≤Ù±‚", true)]
    static private bool UIOff_Validate() => IsPlaying() && AnyActive<UIManager>();
    [MenuItem("Tools/UI ≤Ù±‚", false, 102)]
    static private void UIOff() => SetActive<UIManager>(false, "UI ƒ—±‚", "UI ≤Ù±‚");
    #endregion

    #region ±§∞Ì
    [MenuItem("Tools/±§∞Ì ƒ—±‚", true)]
    static private bool AdsOn_Validate() => IsPlaying() && !AnyActive<ADManager>();
    [MenuItem("Tools/±§∞Ì ƒ—±‚", false, 201)]
    static private void AdsOn() => SetActive<ADManager>(true, "±§∞Ì ƒ—±‚", "±§∞Ì ≤Ù±‚");

    [MenuItem("Tools/±§∞Ì ≤Ù±‚", true)]
    static private bool AdsOff_Validate() => IsPlaying() && AnyActive<ADManager>();
    [MenuItem("Tools/±§∞Ì ≤Ù±‚", false, 202)]
    static private void AdsOff() => SetActive<ADManager>(false, "±§∞Ì ƒ—±‚", "±§∞Ì ≤Ù±‚");
    #endregion
}
#endif
