using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening; // DOTween 네임스페이스 추가

public class GameController : MonoBehaviour
{
    public Text countdownText; // 카운트다운 텍스트
    public FadeController fadeController;
    public float numberScale = 3.0f; // 숫자의 최종 크기

    void Start()
    {
        Debug.Log("Game Started - Time.timeScale = " + Time.timeScale); // 디버그 로그 추가
        Time.timeScale = 0f; // 시간 정지
        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        if (fadeController == null)
        {
            Debug.LogError("fadeController가 설정되지 않았습니다.");
            yield break;
        }

        if (countdownText == null)
        {
            Debug.LogError("countdownText가 설정되지 않았습니다.");
            yield break;
        }

        Debug.Log("fadeController와 countdownText가 올바르게 설정되었습니다.");
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
            countdownText.transform.localScale = Vector3.zero; // 시작 크기 설정
            countdownText.transform.DOScale(numberScale, 1f).SetEase(Ease.OutBounce); // DOScale 애니메이션 설정
            Debug.Log("Countdown: " + i); // 디버그 로그 추가
            yield return new WaitForSecondsRealtime(1);
        }

        countdownText.text = "Start!";
        countdownText.transform.localScale = Vector3.zero; // 시작 크기 설정
        countdownText.transform.DOScale(numberScale, 1f).SetEase(Ease.OutBounce); // DOScale 애니메이션 설정
        yield return new WaitForSecondsRealtime(1);

        countdownText.gameObject.SetActive(false); // 카운트다운 텍스트 비활성화

        // 시간 재개
        Time.timeScale = 1f;
        Debug.Log("Time.timeScale = " + Time.timeScale); // 디버그 로그 추가

        // 게임 시작 로직 호출
        StartGame();
    }

    void StartGame()
    {
        // 게임 시작 로직을 여기에 구현합니다.
        Debug.Log("Game Started"); // 디버그 로그 추가

        // 오브젝트 활성화 상태와 위치를 확인하는 디버그 로그 추가
        foreach (var obj in gameObjects)
        {
            if (obj != null)
            {
                Debug.Log(obj.name + " is at position " + obj.transform.position + " and is active: " + obj.activeSelf);
            }
            else
            {
                Debug.LogError("gameObjects 배열에 null 오브젝트가 있습니다.");
            }
        }
    }
}
