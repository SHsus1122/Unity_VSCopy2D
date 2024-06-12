using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 게임 내 간단한 설명을 위해 사용하는 UI관련 매니저 클래스입니다.
/// </summary>
public class TooltipControll : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Tooltip tooltip;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Item item = GetComponent<Item>();

        if (item != null)
        {
            tooltip.gameObject.SetActive(true);
            tooltip.SetupTooltip(item.textName.text, item.textDesc.text);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.gameObject.SetActive(false);
    }
}
