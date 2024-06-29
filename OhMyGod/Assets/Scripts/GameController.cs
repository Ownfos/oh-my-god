using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour
{
    public Text countdownText; // 카운트다운 텍스트
    public FadeController fadeController;

    void Start()
    {
        Time.timeScale = 0f; // 시간 정지
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        yield return StartCoroutine(fadeController.FadeIn());

        // 카운트다운 시작
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true); // 카운트다운 텍스트 활성화

        // 3, 2, 1 카운트다운 표시
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1);
        }

        countdownText.text = "Start!";
        yield return new WaitForSecondsRealtime(1);

        countdownText.gameObject.SetActive(false); // 카운트다운 텍스트 비활성화

        // 시간 재개
        Time.timeScale = 1f;

        // 게임 시작 로직 호출
        StartGame();
    }

    void StartGame()
    {
        // 게임 시작 로직을 여기에 구현합니다.
        Debug.Log("Game Started"); // 디버그 로그 추가
    }
}
