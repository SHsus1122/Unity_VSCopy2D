using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LevelUp : MonoBehaviour
{
    RectTransform rect;
    Item[] items;
    Button[] buttons;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        items = GetComponentsInChildren<Item>(true);
        buttons = GetComponentsInChildren<Button>();
    }

    private void Start()
    {
        buttons = GetComponentsInChildren<Button>();
        for (int i = 0; i < items.Length; i++)
        {
            buttons[i] = items[i].GetComponent<Button>();
            Debug.Log(items[i].name);
        }
    }

    public void CallLevelUp()
    {
        ListUpdate();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);    // 레벨업 효과음 재생
        AudioManager.instance.EffectBgm(true);
    }

    /*public void Show()
    {
        Next();
        rect.localScale = Vector3.one;
        GameManager.Instance.Stop();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);    // 레벨업 효과음 재생
        AudioManager.instance.EffectBgm(true);
    }

    public void Hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.Instance.Resume();
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);     // 선택 효과음 재생
        AudioManager.instance.EffectBgm(false);
    }*/

    public void Select(int index)
    {
        Debug.Log("==== [ LevelUp ] Select : " + index);
        Debug.Log("==== [ LevelUp ] Player Cost is : " + GameManager.Instance.player.Cost);
        if (GameManager.Instance.player.Cost < 1)
        {
            Debug.Log("==== [ LevelUp ] GameManager.Instance.player.Cost : " + GameManager.Instance.player.Cost);
            foreach (Button button in buttons)
            {
                button.interactable = false;
            }
        }
        else
        {
            bool check = items == null;
            Debug.Log("==== [ LevelUp ] items is bool? : " + check);
            items[index].OnClick();
        }
        ListUpdate();
    }

    /*void Next()
    {
        // 1. 모든 아이템 비활성화
        foreach (Item item in items)
        {
            item.gameObject.SetActive(false);
        }

        // 2. 그 중에서 랜덤 3개 아이템 활성화
        int[] ran = new int[3];
        while (true)
        {
            ran[0] = Random.Range(0, items.Length);
            ran[1] = Random.Range(0, items.Length);
            ran[2] = Random.Range(0, items.Length);

            // 랜덤 결과가 서로가 같지 않을 때 까지 반복하다가 만족시 반복문 종료
            if (ran[0] != ran[1] && ran[1] != ran[2] && ran[0] != ran[2])
                break;
        }

        for (int index = 0; index < ran.Length; index++)
        {
            Item ranItem = items[ran[index]];

            // 3. 만렙 아이템의 경우는 소비 아이템으로 대체
            if (ranItem.level == ranItem.data.damages.Length)
            {
                items[4].gameObject.SetActive(true);    // 회복포션(음료수)
            }
            else
            {
                ranItem.gameObject.SetActive(true);
            }
        }
    }*/

    void ListUpdate()
    {
        if (GameManager.Instance.player.Cost < 1)
            return;

        Debug.Log("==== [ LevelUp ] ListUpdate Call");

        // 1. 모든 아이템 비활성화
        foreach (Item item in items)
        {
            if (item.Itemlevel == item.data.damages.Length)
            {
                item.GetComponentsInChildren<Button>()[0].interactable = false;
            }
            else
            {
                item.GetComponentsInChildren<Button>()[0].interactable = true;
            }
        }
    }
}
