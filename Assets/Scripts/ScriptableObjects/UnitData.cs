using UnityEngine;

[CreateAssetMenu(fileName = "Unit", menuName = "UnitData", order = 1)]
public class UnitData : ScriptableObject
{
    public int unitID;
    public string unitName;
    public Sprite unitImage;
    public float unitMass;

    public UnitData Clone()
    {
        UnitData clone = ScriptableObject.CreateInstance<UnitData>();

        clone.unitID = this.unitID;
        clone.unitName = this.unitName;
        clone.unitImage = this.unitImage;
        clone.unitMass = this.unitMass;

        return clone;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        unitName = unitImage.name;
        unitMass = 1f + unitID / 10f;
    }
#endif
}
