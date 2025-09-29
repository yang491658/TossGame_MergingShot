using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance { get; private set; }

    [Header("Spawn Setting")]
    [SerializeField] private GameObject unitBase;
    [SerializeField] private UnitData[] unitDatas;
    [SerializeField] private Transform spawn;

    [Header("Entity Info.")]
    [SerializeField] private Transform hole;
    [SerializeField] private Transform units;
    [SerializeField] private List<int> unitCounts = new List<int>();
    [SerializeField] private List<UnitSystem> spawned = new List<UnitSystem>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (unitBase == null)
            unitBase = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UnitBase.prefab");

        if (spawn == null)
            spawn = transform.Find("SpawnPos");

        if (hole == null)
            hole = FindFirstObjectByType<HoleSystem>().transform;

        if (units == null)
            units = GameObject.Find("InGame/Units").transform;

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
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        ResetCount();
    }

    #region 소환
    private UnitData FindById(int _id)
    {
        for (int i = 0; i < unitDatas.Length; i++)
            if (unitDatas[i].unitID == _id) return unitDatas[i];

        return null;
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
        unit.transform.SetParent(units, false);
        spawned.Add(unit);

        return unit;
    }

    public void Respawn() => StartCoroutine(RespawnCoroutine());
    private IEnumerator RespawnCoroutine()
    {
        yield return new WaitForSeconds(1f);
        Spawn(Random.Range(1, 5)); // TODO 재소환 로직변경
    }
    #endregion

    #region 제거
    public void DestroyUnit(UnitSystem _unit)
    {
        spawned.Remove(_unit);
        Destroy(_unit.gameObject);
    }

    public void DestroyAll()
    {
        for (int i = spawned.Count - 1; i >= 0; i--)
            DestroyUnit(spawned[i]);

        for (int i = 0; i < unitCounts.Count; i++) unitCounts[i] = 0;

        GameManager.Instance.ResetScore();
    }
    #endregion

    #region 개수
    public void AddCount(UnitData _data) => unitCounts[_data.unitID - 1]++;

    public void ResetCount()
    {
        unitCounts.Clear();
        for (int i = 0; i < unitDatas.Length; i++) unitCounts.Add(0);
    }
    #endregion

    #region GET
    public IReadOnlyList<UnitData> GetDatas() => unitDatas;
    public IReadOnlyList<UnitSystem> GetUnits() => spawned;
    public int GetFinal() => unitDatas[unitDatas.Length - 1].unitID;
    public int GetCount(int _id) => unitCounts[_id - 1];
    public int GetTotalCount() => unitCounts.Sum();
    #endregion
}
