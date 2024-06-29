using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using DG.Tweening;

public class GameController : MonoBehaviour
{
    public Text countdownText;
    public FadeController fadeController;
    public float numberScaleStart = 0.5f;
    public float numberScaleEnd = 1.5f;

    private PlayerMovement playerMovement;

    void Start()
    {
        Debug.Log("Game Started");

        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(false); // 입력 비활성화
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
        StartCoroutine(StartCountdown());
    }

    IEnumerator StartCountdown()
    {
        countdownText.gameObject.SetActive(true);
        Debug.Log("Countdown started.");

        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            countdownText.transform.localScale = new Vector3(numberScaleStart, numberScaleStart, numberScaleStart);
            countdownText.transform.DOScale(numberScaleEnd, 1f).SetEase(Ease.OutBounce);
            Debug.Log("Countdown: " + i);
            yield return new WaitForSeconds(1);
        }

        countdownText.text = "Start!";
        countdownText.transform.localScale = new Vector3(numberScaleStart, numberScaleStart, numberScaleStart);
        countdownText.transform.DOScale(numberScaleEnd, 1f).SetEase(Ease.OutBounce);
        yield return new WaitForSeconds(1);

        countdownText.gameObject.SetActive(false);
        Debug.Log("Countdown completed. Starting game.");

        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true); // 입력 활성화
        }

        StartGame();
    }

    void StartGame()
    {
        Debug.Log("Game Started");
    }
}
