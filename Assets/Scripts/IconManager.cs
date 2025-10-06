#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteAlways]
public class IconManager : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private UnitData[] unitDatas;
    [SerializeField] private Transform hole;
    [SerializeField] private Transform units;

    [Header("Radius")]
    [SerializeField] private float holeRadius = 3.5f;
    [SerializeField] private float unitRadius = 0.3f;
    [SerializeField] private float circleRadius = 9f;
    [SerializeField] private float startAngle = 18f;

    private bool unitsGenerated = false;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (hole == null)
            hole = transform.Find("Hole");

        if (units == null)
            units = transform.Find("Units");

        string[] guids = AssetDatabase.FindAssets("t:UnitData", new[] { "Assets/Scripts/ScriptableObjects" });
        var list = new List<UnitData>(guids.Length);
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var data = AssetDatabase.LoadAssetAtPath<UnitData>(path);
            if (data != null) list.Add(data);
        }
        unitDatas = list.OrderBy(d => d.unitID).ThenBy(d => d.unitName).ToArray();
    }
#endif

    private void Update()
    {
        ApplyHole();

        GenerateUnits();
        ApplyUnits();
    }

    private void ApplyHole()
    {
        if (hole == null) return;
        hole.localScale = Vector3.one * (holeRadius * 2f);
    }

    private void GenerateUnits()
    {
        if (units == null || unitDatas == null || unitsGenerated) return;
        unitsGenerated = true;

#if UNITY_EDITOR
        for (int i = units.childCount - 1; i >= 0; i--)
            DestroyImmediate(units.GetChild(i).gameObject);
#endif

        for (int i = 0; i < unitDatas.Length; i++)
        {
            GameObject obj = new GameObject(unitDatas[i].unitName);
            obj.transform.SetParent(units);
            obj.transform.localPosition = Vector3.zero;

            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = unitDatas[i].unitImage;
        }
    }

    private void ApplyUnits()
    {
        if (units == null) return;

        for (int i = 0; i < units.childCount; i++)
        {
            Transform unit = units.GetChild(i);
            if (unit == null) continue;
            unit.localScale = Vector3.one * (unitRadius * 2f);
        }

        ArrangeUnits();
    }

    private void ArrangeUnits()
    {
        if (units == null) return;
        int count = units.childCount;
        if (count == 0) return;

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            Transform unit = units.GetChild(i);
            if (unit == null) continue;

            float angle = (startAngle - i * angleStep) * Mathf.Deg2Rad;
            Vector3 dir = Quaternion.Euler(0, 0, startAngle - i * angleStep) * Vector3.up;
            Vector3 pos = dir * circleRadius;
            unit.position = (hole != null) ? hole.position + pos : pos;
        }
    }
}
#endif
