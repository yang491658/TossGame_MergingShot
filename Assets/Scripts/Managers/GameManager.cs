using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform spawn;
    [SerializeField] private Transform inGame;
    [SerializeField] private HoleSystem hole;

    public UnitData[] unitDatas;
    private readonly List<GameObject> units = new List<GameObject>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (prefab == null)
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UnitBase.prefab");

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
        Spawn(1);
    }

    #region 소환 및 제거
    public GameObject Spawn(int _id = 0, Vector2? _spawnPos = null)
    {
        if (prefab == null || unitDatas == null || unitDatas.Length == 0 || spawn == null) return null;

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

        GameObject go = Instantiate(prefab, spawnPos, Quaternion.identity);
        go.GetComponent<UnitSystem>().SetData(data.Clone());
        go.transform.SetParent(inGame);
        units.Add(go);

        RegisterToHole(go);
        return go;
    }


    private void RegisterToHole(GameObject _go) => hole.Register(_go.GetComponent<UnitSystem>());

    public void DestroyAll()
    {
        foreach (var u in units)
        {
            if (u != null) Destroy(u);
        }
        units.Clear();
    }
    #endregion

    #region GET
    public int GetFinal() => unitDatas[unitDatas.Length - 1].unitID;
    #endregion
}
