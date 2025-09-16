#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(UnitData))]
public class UnitDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        UnitData data = (UnitData)target;

        string idStr = data.unitID.ToString("D2");
        string newName = $"Unit{idStr}_{data.unitName}";

        string path = AssetDatabase.GetAssetPath(data);
        string currentName = System.IO.Path.GetFileNameWithoutExtension(path);

        if (currentName != newName)
        {
            AssetDatabase.RenameAsset(path, newName);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
