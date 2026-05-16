using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 레벨업 시 3개의 무기 카드를 제시하는 패널.
/// 이미 보유 중인 무기는 레벨업, 미보유는 신규 획득.
/// </summary>
public class LevelUpPanel : MonoBehaviour
{
    public static LevelUpPanel Instance { get; private set; }

    [Header("UI 선택 버튼 (3개)")]
    public Button[]             optionButtons;
    public TextMeshProUGUI[]    optionTitles;
    public TextMeshProUGUI[]    optionDescs;

    [Header("패널 헤더")]
    public TextMeshProUGUI headerText;

    static readonly WeaponCardData[] Cards = new WeaponCardData[]
    {
        new WeaponCardData("Dark Bolt",   "가장 가까운 적을 조준해\n탄환을 발사하여 피해를 가함" + "(데미지 " + DarkBolt.damageLevels[DarkBolt.weaponLevel] + ")",typeof(DarkBolt)),
        new WeaponCardData("Poison Cloud",   "플레이어 위치에 독가스를 방출해\n범위 내 적에게 지속 독 피해 부여 (데미지 8)",      typeof(PoisonCloud)),
        new WeaponCardData("Collapse Edge", "플레이어 전방 부채꼴 범위의\n적을 베어 광역 피해를 가함 (데미지 30)",         typeof(CollapseEdge)),
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        gameObject.SetActive(false);
    }

    /// <summary>레벨업 패널을 열고 무기 카드 3개를 표시</summary>
    public void Show()
    {
        gameObject.SetActive(true);

        if (headerText != null && PlayerStats.Instance != null)
            headerText.text = $"레벨 {PlayerStats.level} 달성!\n무기를 선택하세요";

        var sm = WeaponSlotManager.Instance;

        for (int i = 0; i < Cards.Length; i++)
        {
            if (i >= optionButtons.Length) break;

            int         idx  = i;
            WeaponBase  owned = FindOwned(sm, Cards[i].weaponType);
            bool        maxed = owned != null && owned.IsMaxLevel;

            string levelTag;
            if (owned == null)   levelTag = "[신규]";
            else if (maxed)      levelTag = $"[Lv.{owned.WeaponLevel} 최대]";
            else                 levelTag = $"[Lv.{owned.WeaponLevel} → {owned.WeaponLevel + 1}]";

            if (optionTitles != null && i < optionTitles.Length && optionTitles[i] != null)
                optionTitles[i].text = Cards[i].weaponName + "  " + levelTag;

            if (optionDescs != null && i < optionDescs.Length && optionDescs[i] != null)
                optionDescs[i].text = Cards[i].description;

            if (optionButtons[i] != null)
            {
                optionButtons[i].interactable = !maxed;
                optionButtons[i].onClick.RemoveAllListeners();
                optionButtons[i].onClick.AddListener(() => Select(idx));
            }
        }
    }

    void Select(int index)
    {
        WeaponSlotManager.Instance?.AddWeapon(Cards[index].weaponType);
        gameObject.SetActive(false);
        GameManager.Instance?.ResumePlaying();
    }

    WeaponBase FindOwned(WeaponSlotManager sm, System.Type type)
    {
        if (sm == null) return null;
        foreach (var w in sm.Slots)
            if (w != null && w.GetType() == type) return w;
        return null;
    }

    /// <summary>WeaponSlotManager.RegisterWeaponAbilities() 호환용 — 이 패널에서는 사용하지 않음</summary>
    public void RegisterAbility(string name, string desc, System.Action apply) { }
}
