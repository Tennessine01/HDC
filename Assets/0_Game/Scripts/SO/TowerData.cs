using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameData/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("Thông tin hiển thị")]
    public string towerName;
    public Sprite towerIcon;
    public BulletType bulletType;
    [Header("Prefab Tower trong game")]
    public GameObject towerPrefab;

    [Header("Các thông số cho từng cấp (tối đa 3)")]
    public float[] damages;
    public float[] ranges;
    public float[] fireRates;

    [Header("Chỉ số giá")]
    public int baseCost;

    public Sprite GetIcon()
    {
        return towerIcon;
    }
    public string GetName()
    {
        return towerName;
    }
    public int GetBaseCost() { return baseCost; }

    public float GetDamageByLevel(int towerLevel)
    {
        // Lưu ý: towerLevel-1 phải >=0, <damages.Length
        return damages[towerLevel - 1];
    }

    public float GetRangeByLevel(int towerLevel)
    {
        return ranges[towerLevel - 1];
    }

    public float GetFireRateByLevel(int towerLevel)
    {
        return fireRates[towerLevel - 1];
    }
}

