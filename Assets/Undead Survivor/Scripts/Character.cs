using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    // 캐릭터 고유 속성
    public float GetSpeed()
    {
        return player.typeId == 0 ? 1.1f : 1f;
    }

    public float GetWeaponSpeed()
    {
        return player.typeId == 1 ? 1.1f : 1f;
    }

    public float GetWeaponRate()
    {
        return player.typeId == 1 ? 0.9f : 1f;
    }

    public float GetDamage()
    {
        return player.typeId == 2 ? 1.2f : 1f;
    }

    public int GetCount()
    {
        return player.typeId == 3 ? 1 : 0;
    }
}
