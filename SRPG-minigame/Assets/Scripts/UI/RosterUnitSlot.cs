using UnityEngine;
using UnityEngine.UI;

public class RosterUnitSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image highlightImage;
    [SerializeField] private TMPro.TextMeshProUGUI levelText;

    private UnitData currentData;
    private System.Action<UnitData> onSelected;

    private void Awake()
    {
        // 버튼 컴포넌트를 찾아 클릭 이벤트를 코드에서 직접 연결합니다.
        // 이렇게 하면 인스펙터에서 일일이 연결할 필요가 없습니다.
        var button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    public void Setup(UnitData data, UnitSaveData saveData, System.Action<UnitData> onSelectCallback)
    {
        currentData = data;
        onSelected = onSelectCallback;

        bool hasData = data != null;

        // 아이콘 처리
        if (iconImage != null)
        {
            if (hasData && data.unitIcon != null)
            {
                iconImage.sprite = data.unitIcon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        // 텍스트 처리 (세이브 데이터의 레벨 표시)
        if (levelText != null)
        {
            if (hasData && saveData != null)
            {
                levelText.text = $"Lv.{saveData.level}   Exp.{saveData.currentEXP}";
                levelText.enabled = true;
            }
            else
            {
                levelText.enabled = false;
            }
        }

        SetHighlight(false);
    }

    public void OnClick()
    {
        if (currentData != null)
        {
            onSelected?.Invoke(currentData);
        }
    }

    public void SetHighlight(bool isActive)
    {
        if (highlightImage != null)
        {
            highlightImage.enabled = isActive;
        }
    }
}
