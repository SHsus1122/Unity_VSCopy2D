using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        // 툴팁의 x축의 크기와 포지션을 더한 값이 캔버스 스케일러의 x축의 절반값보다 크면 툴팁이 좌즉에 오게 변경
        if (rt.anchoredPosition.x + rt.sizeDelta.x > halfWidth)
            rt.pivot = new Vector2(1, 0.5f);
        else
            rt.pivot = new Vector2(0, 1);
    }
}
