using System;

/// <summary>
/// LevelUpPanel 무기 카드에 표시할 정적 데이터
/// </summary>
[Serializable]
public class WeaponCardData
{
    public string     weaponName;
    public string     description;
    public Type       weaponType;

    public WeaponCardData(string name, string desc, Type type)
    {
        weaponName  = name;
        description = desc;
        weaponType  = type;
    }
}
