using UnityEngine;

// 결과창 UI 관련 클래스
public class Result : MonoBehaviour
{
    public GameObject[] titles;

    public void Lose()  // 패배 UI 활성화
    {
        titles[0].SetActive(true);
        titles[1].SetActive(false);
    }

    public void Win()   // 승리 UI 활성화
    {
        titles[1].SetActive(true);
        titles[0].SetActive(false);
    }
}
