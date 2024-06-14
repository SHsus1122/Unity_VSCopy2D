using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 실시간 UI에 보여주는 정보들의 업데이트에 사용하는 HUD 클래스입니다.
/// </summary>
public class HUD : MonoBehaviour
{
    // 정보의 종류를 담을 열거형과 타입 선언
    public enum InfoType { Exp, Level, Kill, Time, Health }
    public InfoType type;
    public Player player;

    Text myText;        // 텍스트 정보
    Slider mySlider;    // 슬라이더 정보

    private void Awake()
    {
        myText = GetComponent<Text>();
        mySlider = GetComponent<Slider>();
    }


    private void Update()
    {
        if (player == null)
            return;

        switch (type)
        {
            case InfoType.Exp:
                UpdateExp();
                break;
            case InfoType.Level:
                UpdateLevel();
                break;
            case InfoType.Kill:
                UpdateKill();
                break;
            case InfoType.Time:
                UpdateTime();
                break;
            case InfoType.Health:
                UpdateHealth();
                break;
        }
    }


    public void UpdateExp()
    {
        float curExp = player.exp;
        float maxExp = PlayerManager.instance.nextExp[Mathf.Min(player.level, PlayerManager.instance.nextExp.Length - 1)];
        mySlider.value = curExp / maxExp;
    }


    public void UpdateLevel()
    {
        // Format 각 숫자 인자값을 지정된 형태의 문자열로 만들어주는 함수
        //  첫 번쨰 인자 : 포맷을 쓸 타입 / 두 번째 인자 : 해당 포맷에 적용될 Data
        //      - {0} : 인자 값의 문자열이 들어갈 자리를 {순번} 형태로 지정합니다.
        //      - F0, F1... 이는 소숫점 자리를 지정합니다.
        myText.text = string.Format("Lv.{0:F0}", player.level);
    }


    public void UpdateKill()
    {
        myText.text = string.Format("{0:F0}", player.kill);
    }


    public void UpdateTime()
    {
        float remainTime = GameManager.instance.maxGameTime - GameManager.instance.gameTime;
        remainTime = Mathf.Max(remainTime, 0);          // 최소값을 0으로 제한

        int min = Mathf.FloorToInt(remainTime / 60);    // 분
        int sec = Mathf.FloorToInt(remainTime % 60);    // 초, 나머지 계산

        if (min <= 0 && sec <= 0)
            myText.text = "00:00";      // 시간이 0 이하로 내려간 경우 "00:00"으로 표시
        else
            myText.text = string.Format("{0:D2}:{1:D2}", min, sec); // D1, D2 ... 자릿수 고정
    }


    public void UpdateHealth()
    {
        float curHealth = player.health;
        float maxHealth = PlayerManager.instance.maxHealth;
        mySlider.value = curHealth / maxHealth;
    }
}
