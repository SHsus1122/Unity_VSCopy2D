using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���� �� ������ ������ ���� ����ϴ� UI���� Ŭ�����Դϴ�.
/// </summary>
public class Tooltip : MonoBehaviour
{
    float halfWidth;
    RectTransform rt;

    public Text textName;
    public Text textDesc;

    private void Start()
    {
        halfWidth = GetComponentInParent<CanvasScaler>().referenceResolution.x * 0.5f;
        rt = GetComponent<RectTransform>();
    }

    public void SetupTooltip(string name, string des)
    {
        textName.text = name;
        textDesc.text = des;
    }

    private void Update()
    {
        transform.position = Input.mousePosition;

        // ������ x���� ũ��� �������� ���� ���� ĵ���� �����Ϸ��� x���� ���ݰ����� ũ�� ������ ���￡ ���� ����
        if (rt.anchoredPosition.x + rt.sizeDelta.x > halfWidth)
            rt.pivot = new Vector2(1, 0.5f);
        else
            rt.pivot = new Vector2(0.5f, 1);
    }
}