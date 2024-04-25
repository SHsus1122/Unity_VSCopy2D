using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 업적 관련 매니저 클래스
public class AchiveManager : MonoBehaviour
{
    public GameObject[] lockCharacter;
    public GameObject[] unlockCharacter;
    public GameObject uiNotice;

    // unlockPotato : 감자 농부, unlockBean : 콩 농부
    enum Achive { unlockPotato, unlockBean }
    Achive[] achives;               // 업적 데이터 저장용 배열
    WaitForSecondsRealtime wait;

    private void Awake()
    {
        // 위에서 선언한 achives 배열 초기화 작업
        // Enum.GetValues : 주어진 열거형의 데이터를 모두 가져오는 함수, 인자값은 어떤 타입인지 명시 필요(typeof 사용)
        //                  Enum.GetValues은 반환값이 배열입니다. 따라서 (Achive[])를 통해서 배열로 변환을 해줍니다.
        achives = (Achive[])Enum.GetValues(typeof(Achive));
        wait = new WaitForSecondsRealtime(5);

        // 만약에 PlayerPrefs클래스에 MyData라는 키가 없으면 초기화 작업을 선행합니다.
        // 즉, 게임을 한 번이라도 플레이 했을 경우 초기화작업은 무시합니다(초기에 한 번만 수행하는 세이브 개념)
        if (!PlayerPrefs.HasKey("MyData"))
        {
            Init();
        }
    }

    // 데이터를 다른 디바이스(웹, 모바일 등)에서도 데이터를 저장하고 꺼내 써야합니다.
    // 그래서 이러한 경우에는 데이터를 먼저 초기화해줘야 하기 때문에 이를 위한 초기화 함수를 작성합니다.
    void Init()
    {
        // PlayerPrefs : 간단한 저장 기능을 제공하는 유니티 제공 클래스 입니다.
        PlayerPrefs.SetInt("MyData", 1);        // Key값, Data

        // 업적 초기화 작업
        foreach (Achive achive in achives)
            PlayerPrefs.SetInt(achive.ToString(), 0);  // 미해금은 0으로 저장
    }

    // Start is called before the first frame update
    void Start()
    {
        UnlockCharacter();
    }

    void UnlockCharacter()
    {
        for (int index = 0; index < lockCharacter.Length; index++)
        {
            string achiveName = achives[index].ToString();
            bool isUnlock = PlayerPrefs.GetInt(achiveName) == 1;    // 업적 달성 유무에 따라 bool값 변동
            lockCharacter[index].SetActive(!isUnlock);              // lock 캐릭터는 false로 안 보이게 상태 변경
            unlockCharacter[index].SetActive(isUnlock);             // unlock 캐릭터는 true로 보이게 상태 변경
        }
    }

    void LateUpdate()
    {
        foreach (Achive achive in achives)
        {
            CheckAchive(achive);
        }
    }

    void CheckAchive(Achive achive)
    {
        bool isAchive = false;  // 업적 달성 확인용 변수

        // 스위치문을 통해 확인용 변수의 상태를 변화시킵니다.
        switch (achive)
        { 
            case Achive.unlockPotato:
                isAchive = GameManager.Instance.kill >= 10;
                break;
            case Achive.unlockBean:
                isAchive = GameManager.Instance.gameTime == GameManager.Instance.maxGameTime;
                break;
        }

        // 업적 달성 확인, 만약 달성이 확인되면 해금합니다.
        if (isAchive && PlayerPrefs.GetInt(achive.ToString()) == 0)
        {
            PlayerPrefs.SetInt(achive.ToString(), 1);   // 업적 해금

            for (int index = 0; index < uiNotice.transform.childCount; index++)
            {
                bool isActive = index == (int)achive;
                uiNotice.transform.GetChild(index).gameObject.SetActive(isActive);
            }
            StartCoroutine(NoticeRoutine());
        }
    }

    IEnumerator NoticeRoutine()
    {
        uiNotice.SetActive(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);   // 알림 효과음 재생

        yield return wait;

        uiNotice.SetActive(false);
    }
}
