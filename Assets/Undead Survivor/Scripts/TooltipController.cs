using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ���� �� ������ ������ ���� ����ϴ� UI���� �Ŵ��� Ŭ�����Դϴ�.
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