using UnityEngine;

/// <summary>
/// 각 캐릭터별로 고유 수치(값)의 적용을 위한 클래스입니다.
/// </summary>
public class Character : MonoBehaviour
{
    public Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    // 캐릭터 고유 속성에 따라 무기나 캐릭터 관련한 수치들이 다르게 적용됩니다.
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
