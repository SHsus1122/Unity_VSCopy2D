using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

public class HUD : MonoBehaviour
{
    // 정보의 종류를 담을 열거형과 타입 선언
    public enum InfoType { Exp, Level, Kill, Time, Health, Nickname }
    public InfoType type;

    Text myText;        // 텍스트 정보
    Slider mySlider;    // 슬라이더 정보

    private void Awake()
    {
        myText = GetComponent<Text>();
        mySlider = GetComponent<Slider>();
    }

    private void LateUpdate()
    {
        switch (type)
        {
            case InfoType.Exp:
                float curExp = GameManager.Instance.exp;
                float maxExp = GameManager.Instance.nextExp[Mathf.Min(GameManager.Instance.level, GameManager.Instance.nextExp.Length - 1)];
                mySlider.value = curExp / maxExp;
                break;
            case InfoType.Level:
                // Format 각 숫자 인자값을 지정된 형태의 문자열로 만들어주는 함수
                //  첫 번쨰 인자 : 포맷을 쓸 타입 / 두 번째 인자 : 해당 포맷에 적용될 Data
                //      - {0} : 인자 값의 문자열이 들어갈 자리를 {순번} 형태로 지정합니다.
                //      - F0, F1... 이는 소숫점 자리를 지정합니다.
                myText.text = string.Format("Lv.{0:F0}", GameManager.Instance.level);
                break;
            case InfoType.Kill:
                myText.text = string.Format("{0:F0}", GameManager.Instance.kill);
                break;
            case InfoType.Time:
                float remainTime = GameManager.Instance.maxGameTime - GameManager.Instance.gameTime;
                int min = Mathf.FloorToInt(remainTime / 60);    // 분
                int sec = Mathf.FloorToInt(remainTime % 60);    // 초, 나머지 계산
                myText.text = string.Format("{0:D2}:{1:D2}", min, sec); // D1, D2 ... 자릿수 고정
                break;
            case InfoType.Health:
                float curHealth = GameManager.Instance.health;
                float maxHealth = GameManager.Instance.maxHealth;
                mySlider.value = curHealth / maxHealth;
                break;
            case InfoType.Nickname:
                myText.text = GameManager.Instance.player.name;
                break;
        }
    }
}
