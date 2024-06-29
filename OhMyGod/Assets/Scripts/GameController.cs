using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour
{
    public Text countdownText; // 카운트다운 텍스트
    public GameObject[] gameObjects; // 게임 시작 시 활성화할 오브젝트들

    void Start()
    {
        // 게임 오브젝트를 초기화 시 비활성화
        foreach (GameObject obj in gameObjects)
        {
            obj.SetActive(false);
        }

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
            yield return new WaitForSeconds(1);
        }

        countdownText.text = "Start!";
        yield return new WaitForSeconds(1);

        countdownText.gameObject.SetActive(false); // 카운트다운 텍스트 비활성화

        // 게임 오브젝트 활성화
        foreach (GameObject obj in gameObjects)
        {
            obj.SetActive(true);
        }

        // 게임 시작 로직 호출
        StartGame();
    }

    void StartGame()
    {
        // 게임 시작 로직을 여기에 구현합니다.
        // 예: 게임 타이머 시작, 플레이어 컨트롤 활성화 등
        Debug.Log("Game Started"); // 디버그 로그 추가
    }
}
