using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Spawn Setting")]
    [SerializeField] private GameObject unitBase;
    [SerializeField] private UnitData[] unitDatas;
    [SerializeField] private Transform spawn;
    [SerializeField] private Transform inGame;

    [Header("Unit Info.")]
    [SerializeField] private HoleSystem hole;
    [SerializeField] private List<UnitSystem> units = new List<UnitSystem>();
    [SerializeField] private List<int> counts = new List<int>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (unitBase == null)
            unitBase = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UnitBase.prefab");

        if (spawn == null)
            spawn = transform.Find("SpawnPos");

        if (inGame == null)
            inGame = GameObject.Find("InGame").transform;

        if (hole == null)
            hole = FindFirstObjectByType<HoleSystem>();

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    #region 소환
    private UnitData FindById(int id)
    {
#if UNITY_EDITOR
        return System.Linq.Enumerable.FirstOrDefault(unitDatas, d => d.unitID == id);
#else
    for (int i = 0; i < unitDatas.Length; i++)
        if (unitDatas[i].unitID == id) return unitDatas[i];
    return null;
#endif
    }

    public UnitSystem Spawn(int _id = 0, Vector2? _spawnPos = null)
    {
        UnitData data = (_id == 0)
            ? unitDatas[Random.Range(0, unitDatas.Length / 2)]
            : FindById(_id);

        if (data == null) return null;

        Vector2 spawnPos = _spawnPos ?? (Vector2)spawn.position;

        UnitSystem unit = Instantiate(unitBase, spawnPos, Quaternion.identity)
            .GetComponent<UnitSystem>();

        unit.SetData(data.Clone());
        unit.transform.SetParent(inGame, false);
        units.Add(unit);

        return unit;
    }
    #endregion

    #region 제거
    public void DestroyUnit(UnitSystem _unit)
    {
        units.Remove(_unit);
        Destroy(_unit.gameObject);
    }

    public void DestroyAll()
    {
        for (int i = units.Count - 1; i >= 0; i--)
            DestroyUnit(units[i]);

        GameManager.Instance.ResetScore();
    }
    #endregion

    #region GET
    public IReadOnlyList<UnitSystem> GetUnits() => units;
    public int GetFinal() => unitDatas[unitDatas.Length - 1].unitID;
    #endregion
}
