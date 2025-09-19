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

    private void Start()
    {
        // TODO 시작하면 유닛 소환
    }

    #region 소환
    public UnitSystem Spawn(int _id = 0, Vector2? _spawnPos = null)
    {
        if (unitBase == null || unitDatas == null || unitDatas.Length == 0 || spawn == null) return null;

        UnitData data = null;

        if (_id == 0)
            data = unitDatas[Random.Range(0, unitDatas.Length)];
        else
        {
#if UNITY_EDITOR
            data = unitDatas.FirstOrDefault(d => d.unitID == _id);
#else
        for (int i = 0; i < unitDatas.Length; i++)
            if (unitDatas[i].unitID == _id) { data = unitDatas[i]; break; }
#endif
        }

        if (data == null) return null;

        Vector2 spawnPos = _spawnPos ?? (Vector2)spawn.position;

        UnitSystem unit = Instantiate(unitBase, spawnPos, Quaternion.identity)
            .GetComponent<UnitSystem>();

        unit.SetData(data.Clone());
        unit.transform.SetParent(inGame);
        units.Add(unit);

        Debug.Log(unit.gameObject.name + " 소환 : " + spawnPos);

        return unit;
    }
    #endregion

    #region 제거
    public void DestroyUnit(UnitSystem _unit)
    {
        if (_unit == null) return;
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
    public int GetFinal()
    {
        if (unitDatas == null || unitDatas.Length == 0) return -1;
        return unitDatas[unitDatas.Length - 1].unitID;
    }
    #endregion
}
