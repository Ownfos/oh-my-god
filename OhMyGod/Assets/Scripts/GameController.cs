using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening; // DOTween 네임스페이스 추가

public class GameController : MonoBehaviour
{
    public Text countdownText; // 카운트다운 텍스트
    public FadeController fadeController;
    public float numberScaleStart = 0.5f; // 애니메이션 시작 크기
    public float numberScaleEnd = 1.5f; // 애니메이션 끝 크기
    public PlayerMovement playerMovement;
    public NPCMovement[] npcMovements; // 모든 NPC의 움직임을 제어하기 위한 배열

    void Start()
    {
        Debug.Log("Game Started"); // 디버그 로그 추가

        // Null 체크
        if (playerMovement == null)
        {
            Debug.LogError("playerMovement가 설정되지 않았습니다.");
            return;
        }

        if (npcMovements == null || npcMovements.Length == 0)
        {
            Debug.LogError("npcMovements 배열이 설정되지 않았습니다.");
            return;
        }

        // 플레이어와 NPC 움직임 비활성화
        playerMovement.SetCanMove(false);
        foreach (var npc in npcMovements)
        {
            npc.SetCanMove(false);
        }

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

        Debug.Log("Fade In completed. Starting countdown.");
        // 카운트다운 시작
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true); // 카운트다운 텍스트 활성화
        Debug.Log("Countdown started.");

        // 3, 2, 1 카운트다운 표시
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            countdownText.transform.localScale = new Vector3(numberScaleStart, numberScaleStart, numberScaleStart); // 시작 크기 설정
            countdownText.transform.DOScale(numberScaleEnd, 1f).SetEase(Ease.OutBounce); // DOScale 애니메이션 설정
            Debug.Log("Countdown: " + i); // 디버그 로그 추가
            yield return new WaitForSeconds(1);
        }

        countdownText.text = "Start!";
        countdownText.transform.localScale = new Vector3(numberScaleStart, numberScaleStart, numberScaleStart); // 시작 크기 설정
        countdownText.transform.DOScale(numberScaleEnd, 1f).SetEase(Ease.OutBounce); // DOScale 애니메이션 설정
        yield return new WaitForSeconds(1);

        countdownText.gameObject.SetActive(false); // 카운트다운 텍스트 비활성화
        Debug.Log("Countdown completed. Starting game.");

        // 플레이어와 NPC 움직임 활성화
        playerMovement.SetCanMove(true);
        foreach (var npc in npcMovements)
        {
            npc.SetCanMove(true);
        }

        // 게임 시작 로직 호출
        StartGame();
    }

    void StartGame()
    {
        // 게임 시작 로직을 여기에 구현합니다.
        Debug.Log("Game Started"); // 디버그 로그 추가
    }
}
