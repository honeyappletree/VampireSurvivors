using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 하단 무기 슬롯 HUD. Canvas 하위 GameObject에 붙인다.
/// Start()에서 슬롯 패널 3개를 동적 생성하므로 씬에서 별도 자식 없이 동작.
/// </summary>
public class WeaponSlotUI : MonoBehaviour
{
    [Header("슬롯 설정")]
    [SerializeField] private int   slotCount  = 3;
    [SerializeField] private float slotSize   = 64f;
    [SerializeField] private float slotSpacing = 8f;

    private TextMeshProUGUI[] _labels;

    void Start()
    {
        _labels = new TextMeshProUGUI[slotCount];
        float totalWidth = slotCount * slotSize + (slotCount - 1) * slotSpacing;
        float startX     = -(totalWidth - slotSize) * 0.5f;

        for (int i = 0; i < slotCount; i++)
        {
            // 슬롯 배경 패널
            var panel = new GameObject($"WeaponSlot_{i}", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(transform, false);

            var rt = panel.GetComponent<RectTransform>();
            rt.sizeDelta        = new Vector2(slotSize, slotSize);
            rt.anchoredPosition = new Vector2(startX + i * (slotSize + slotSpacing), 0f);

            var bg = panel.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.6f);

            // 슬롯 텍스트
            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(panel.transform, false);

            var lrt = labelGO.GetComponent<RectTransform>();
            lrt.anchorMin    = Vector2.zero;
            lrt.anchorMax    = Vector2.one;
            lrt.offsetMin    = Vector2.zero;
            lrt.offsetMax    = Vector2.zero;

            var tmp = labelGO.GetComponent<TextMeshProUGUI>();
            tmp.fontSize          = 11f;
            tmp.alignment         = TextAlignmentOptions.Center;
            tmp.color             = Color.white;
            tmp.enableWordWrapping = true;

            _labels[i] = tmp;
        }
    }

    void Update()
    {
        var sm = WeaponSlotManager.Instance;
        if (sm == null || _labels == null) return;

        var slots = sm.Slots;
        for (int i = 0; i < _labels.Length; i++)
        {
            if (_labels[i] == null) continue;
            if (i < slots.Length && slots[i] != null)
                _labels[i].text = $"{slots[i].WeaponName}\nLv.{slots[i].WeaponLevel}";
            else
                _labels[i].text = "—";
        }
    }
}
